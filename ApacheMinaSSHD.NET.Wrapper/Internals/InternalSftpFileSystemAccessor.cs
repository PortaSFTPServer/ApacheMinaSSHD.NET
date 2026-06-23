// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.FileSystem;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using java.nio.channels;
using java.nio.file;
using java.nio.file.attribute;
using java.security;
using java.util;
using Microsoft.Win32.SafeHandles;
using org.apache.sshd.sftp.server;
using System.Runtime.InteropServices;
using System.Text;
using Path = java.nio.file.Path;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalSftpFileSystemAccessor : SftpFileSystemAccessor
    {
        private readonly SftpFileSystemAccessor accessorDelegate;
        private readonly IAMNetSftpFileSystemAccessor fileSystemAccessor;
        static readonly IAMNetLogger logger = new AMNetLogger(typeof(InternalSftpFileSystemAccessor), AMNetLogger.LogLevel.Info);

        public InternalSftpFileSystemAccessor(IAMNetSftpFileSystemAccessor fileSystemAccessor, AMNetSftpSubsystemFactory factory)
        {
            accessorDelegate = factory.JavaFactory.getFileSystemAccessor()
                ?? throw new InvalidOperationException(
                    "Failed to obtain the default SftpFileSystemAccessor from SftpSubsystemFactory.");

            this.fileSystemAccessor = fileSystemAccessor
                ?? throw new ArgumentNullException(nameof(fileSystemAccessor));
        }

        public Path resolveLocalFilePath(SftpSubsystemProxy subsystem, Path rootDir, string remotePath)
        {
            // Keep MINA's default resolution first, then let managed policy rewrite or reject the path.
            Path resolvedPath = accessorDelegate.resolveLocalFilePath(subsystem, rootDir, remotePath);

            var context = CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveLocalFilePath,
                path: resolvedPath,
                rootDir: rootDir,
                remotePath: remotePath);

            string managedPath = fileSystemAccessor.ResolveLocalFilePath(context, resolvedPath.toString());

            Path finalPath = ToPath(managedPath, resolvedPath);

            // Use real Windows paths for symlink containment when available.
            // VirtualFileSystemView paths like "/outside-link.txt" are virtual and
            // cannot be used with File.ResolveLinkTarget, FindFirstFile, etc.
            ValidateSymlinkWithRealPath(subsystem, finalPath, rootDir, remotePath);

            var validationContext = CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveLocalFilePath,
                path: finalPath,
                rootDir: rootDir,
                remotePath: remotePath);

            if (!fileSystemAccessor.IsPathAllowed(validationContext))
            {
                throw new NoSuchFileException(null, null, "File or directory is not allowed.");
            }

            return finalPath;
        }

        public LinkOption[] resolveFileAccessLinkOptions(
            SftpSubsystemProxy subsystem,
            Path file,
            int cmd,
            string extension,
            bool followLinks)
        {
            LinkOption[] resolvedOptions = accessorDelegate.resolveFileAccessLinkOptions(
                subsystem,
                file,
                cmd,
                extension,
                followLinks);

            var context = CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveFileAccessLinkOptions,
                path: file,
                command: cmd,
                extension: extension,
                followLinks: followLinks,
                options: ToOptionList(resolvedOptions));

            IReadOnlyList<string> managedOptions = fileSystemAccessor.ResolveFileAccessLinkOptions(
                context,
                ToOptionList(resolvedOptions));

            return ToLinkOptions(managedOptions, resolvedOptions);
        }

        public NavigableMap resolveReportedFileAttributes(
            SftpSubsystemProxy subsystem,
            Path file,
            int flags,
            NavigableMap attrs,
            params LinkOption[] options)
        {
            NavigableMap resolvedAttributes = accessorDelegate.resolveReportedFileAttributes(
                subsystem,
                file,
                flags,
                attrs,
                options);
            IReadOnlyDictionary<string, object> resolvedDictionary = ToDictionary(resolvedAttributes);

            var context = CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveReportedFileAttributes,
                path: file,
                command: flags,
                options: ToOptionList(options),
                attributes: resolvedDictionary);

            IReadOnlyDictionary<string, object> managedAttributes =
                fileSystemAccessor.ResolveReportedFileAttributes(context, resolvedDictionary);

            return ReferenceEquals(managedAttributes, resolvedDictionary)
                ? resolvedAttributes
                : ToNavigableMap(managedAttributes);
        }

        public void applyExtensionFileAttributes(
            SftpSubsystemProxy subsystem,
            Path file,
            Map extensions,
            params LinkOption[] options)
        {
            fileSystemAccessor.ApplyExtensionFileAttributes(CreateContext(
                subsystem,
                SshFileSystemOperation.ApplyExtensionFileAttributes,
                path: file,
                options: ToOptionList(options),
                attrs: extensions));

            accessorDelegate.applyExtensionFileAttributes(subsystem, file, extensions, options);
        }

        public void putRemoteFileName(
            SftpSubsystemProxy subsystem,
            Path path,
            org.apache.sshd.common.util.buffer.Buffer buf,
            string name,
            bool shortName)
        {
            fileSystemAccessor.PutRemoteFileName(CreateContext(
                subsystem,
                SshFileSystemOperation.PutRemoteFileName,
                path: path,
                remoteName: name,
                shortName: shortName));

            accessorDelegate.putRemoteFileName(subsystem, path, buf, name, shortName);
        }

        public SeekableByteChannel openFile(
            SftpSubsystemProxy subsystem,
            FileHandle fileHandle,
            Path file,
            string handle,
            Set options,
            params FileAttribute[] attrs)
        {
            logger.Debug($"openFile({file})");
            fileSystemAccessor.OpenFile(CreateContext(
                subsystem,
                SshFileSystemOperation.OpenFile,
                path: file,
                remoteHandle: handle,
                options: ToOptionList(options),
                attributes: ToDictionary(attrs)));

            try
            {
                // Bypass accessorDelegate.openFile() — the default MINA implementation calls
                // Files.exists(file) whose IOException catch is lost through IKVM translation,
                // causing NoSuchFileException to propagate for new files.
                return FileChannel.open(file, options, attrs);
            }
            catch (java.lang.UnsupportedOperationException) when (attrs?.Length > 0)
            {
                // Some Windows providers reject create-time attributes sent by clients such as WinSCP.
                // Retry without attributes so the open can succeed while later policy hooks still run.
                return FileChannel.open(file, options);
            }
            catch (NotSupportedException) when (attrs?.Length > 0)
            {
                // Same compatibility path as the Java exception case, surfaced through IKVM as .NET.
                return FileChannel.open(file, options);
            }
        }

        public FileLock tryLock(
            SftpSubsystemProxy subsystem,
            FileHandle fileHandle,
            Path file,
            string handle,
            Channel channel,
            long position,
            long size,
            bool shared)
        {
            fileSystemAccessor.TryLock(CreateContext(
                subsystem,
                SshFileSystemOperation.TryLock,
                path: file,
                remoteHandle: handle,
                offset: position,
                length: size,
                sharedLock: shared));

            return accessorDelegate.tryLock(subsystem, fileHandle, file, handle, channel, position, size, shared);
        }

        public void syncFileData(
            SftpSubsystemProxy subsystem,
            FileHandle fileHandle,
            Path file,
            string handle,
            Channel channel)
        {
            fileSystemAccessor.SyncFileData(CreateContext(
                subsystem,
                SshFileSystemOperation.SyncFileData,
                path: file,
                remoteHandle: handle));

            accessorDelegate.syncFileData(subsystem, fileHandle, file, handle, channel);
        }

        public void closeFile(
            SftpSubsystemProxy subsystem,
            FileHandle fileHandle,
            Path file,
            string handle,
            Channel channel,
            Set options)
        {
            logger.Debug($"closeFile({file})");
            fileSystemAccessor.CloseFile(CreateContext(
                subsystem,
                SshFileSystemOperation.CloseFile,
                path: file,
                remoteHandle: handle,
                options: ToOptionList(options)));

            accessorDelegate.closeFile(subsystem, fileHandle, file, handle, channel, options);
        }

        public DirectoryStream openDirectory(
            SftpSubsystemProxy subsystem,
            DirectoryHandle dirHandle,
            Path dir,
            string handle,
            params LinkOption[] linkOptions)
        {
            fileSystemAccessor.OpenDirectory(CreateContext(
                subsystem,
                SshFileSystemOperation.OpenDirectory,
                path: dir,
                remoteHandle: handle,
                isDirectory: true,
                options: ToOptionList(linkOptions)));

            DirectoryStream originalStream = accessorDelegate.openDirectory(subsystem, dirHandle, dir, handle, linkOptions);

            return new FilteredDirectoryStream(
                originalStream,
                entry => fileSystemAccessor.ShouldIncludeDirectoryEntry(CreateContext(
                    subsystem,
                    SshFileSystemOperation.DirectoryEntry,
                    path: entry,
                    remoteHandle: handle,
                    isDirectory: Directory.Exists(entry.toString()))));
        }

        public void closeDirectory(
            SftpSubsystemProxy subsystem,
            DirectoryHandle dirHandle,
            Path dir,
            string handle,
            DirectoryStream ds)
        {
            fileSystemAccessor.CloseDirectory(CreateContext(
                subsystem,
                SshFileSystemOperation.CloseDirectory,
                path: dir,
                remoteHandle: handle,
                isDirectory: true));

            accessorDelegate.closeDirectory(subsystem, dirHandle, dir, handle, ds);
        }

        public Map readFileAttributes(
            SftpSubsystemProxy subsystem,
            Path file,
            string view,
            params LinkOption[] options)
        {
            Map resolvedAttributes = accessorDelegate.readFileAttributes(subsystem, file, view, options);
            IReadOnlyDictionary<string, object> resolvedDictionary = ToDictionary(resolvedAttributes);

            var context = CreateContext(
                subsystem,
                SshFileSystemOperation.ReadFileAttributes,
                path: file,
                fileAttributeView: view,
                options: ToOptionList(options),
                attributes: resolvedDictionary);

            IReadOnlyDictionary<string, object> managedAttributes =
                fileSystemAccessor.ReadFileAttributes(context, resolvedDictionary);

            return ReferenceEquals(managedAttributes, resolvedDictionary)
                ? resolvedAttributes
                : ToMap(managedAttributes);
        }

        public void setFileAttribute(
            SftpSubsystemProxy subsystem,
            Path file,
            string view,
            string attribute,
            object value,
            params LinkOption[] options)
        {
            fileSystemAccessor.SetFileAttribute(CreateContext(
                subsystem,
                SshFileSystemOperation.SetFileAttribute,
                path: file,
                fileAttributeView: view,
                fileAttributeName: attribute,
                value: value,
                options: ToOptionList(options)));

            accessorDelegate.setFileAttribute(subsystem, file, view, attribute, value, options);
        }

        public UserPrincipal resolveFileOwner(
            SftpSubsystemProxy subsystem,
            Path file,
            UserPrincipal name)
        {
            fileSystemAccessor.ResolveFileOwner(CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveFileOwner,
                path: file,
                owner: name?.getName()));

            return accessorDelegate.resolveFileOwner(subsystem, file, name);
        }

        public void setFileOwner(
            SftpSubsystemProxy subsystem,
            Path file,
            Principal value,
            params LinkOption[] options)
        {
            fileSystemAccessor.SetFileOwner(CreateContext(
                subsystem,
                SshFileSystemOperation.SetFileOwner,
                path: file,
                owner: value?.getName(),
                options: ToOptionList(options)));

            accessorDelegate.setFileOwner(subsystem, file, value, options);
        }

        public GroupPrincipal resolveGroupOwner(
            SftpSubsystemProxy subsystem,
            Path file,
            GroupPrincipal name)
        {
            fileSystemAccessor.ResolveGroupOwner(CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveGroupOwner,
                path: file,
                group: name?.getName()));

            return accessorDelegate.resolveGroupOwner(subsystem, file, name);
        }

        public void setGroupOwner(
            SftpSubsystemProxy subsystem,
            Path file,
            Principal value,
            params LinkOption[] options)
        {
            fileSystemAccessor.SetGroupOwner(CreateContext(
                subsystem,
                SshFileSystemOperation.SetGroupOwner,
                path: file,
                group: value?.getName(),
                options: ToOptionList(options)));

            accessorDelegate.setGroupOwner(subsystem, file, value, options);
        }

        public void setFilePermissions(
            SftpSubsystemProxy subsystem,
            Path file,
            Set perms,
            params LinkOption[] options)
        {
            fileSystemAccessor.SetFilePermissions(CreateContext(
                subsystem,
                SshFileSystemOperation.SetFilePermissions,
                path: file,
                options: ToOptionList(options),
                attributes: ToNamedValues("permissions", ToOptionList(perms))));

            accessorDelegate.setFilePermissions(subsystem, file, perms, options);
        }

        public void setFileAccessControl(
            SftpSubsystemProxy subsystem,
            Path file,
            List acl,
            params LinkOption[] options)
        {
            fileSystemAccessor.SetFileAccessControl(CreateContext(
                subsystem,
                SshFileSystemOperation.SetFileAccessControl,
                path: file,
                options: ToOptionList(options),
                attributes: ToNamedValues("acl", ToOptionList(acl))));

            accessorDelegate.setFileAccessControl(subsystem, file, acl, options);
        }

        public void createDirectory(SftpSubsystemProxy subsystem, Path path)
        {
            logger.Info($"createDirectory({path})");
            fileSystemAccessor.CreateDirectory(CreateContext(
                subsystem,
                SshFileSystemOperation.CreateDirectory,
                path: path,
                isDirectory: true));

            accessorDelegate.createDirectory(subsystem, path);
        }

        public void createLink(SftpSubsystemProxy subsystem, Path link, Path existing, bool symLink)
        {
            logger.Debug($"createLink({link} -> {existing}, symLink={symLink})");
            fileSystemAccessor.CreateLink(CreateContext(
                subsystem,
                SshFileSystemOperation.CreateLink,
                path: link,
                sourcePath: existing?.toString(),
                destinationPath: link?.toString(),
                isSymbolicLink: symLink));

            accessorDelegate.createLink(subsystem, link, existing, symLink);
        }

        public string resolveLinkTarget(SftpSubsystemProxy subsystem, Path link)
        {
            string resolvedTarget = accessorDelegate.resolveLinkTarget(subsystem, link);

            return fileSystemAccessor.ResolveLinkTarget(CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveLinkTarget,
                path: link,
                destinationPath: resolvedTarget),
                resolvedTarget);
        }

        public void renameFile(SftpSubsystemProxy subsystem, Path oldPath, Path newPath, Collection opts)
        {
            logger.Debug($"renameFile({oldPath} -> {newPath})");
            fileSystemAccessor.RenameFile(CreateContext(
                subsystem,
                SshFileSystemOperation.RenameFile,
                path: oldPath,
                sourcePath: oldPath?.toString(),
                destinationPath: newPath?.toString(),
                options: ToOptionList(opts)));

            accessorDelegate.renameFile(subsystem, oldPath, newPath, opts);
        }

        public void copyFile(SftpSubsystemProxy subsystem, Path src, Path dst, Collection opts)
        {
            logger.Debug($"copyFile({src} -> {dst})");
            fileSystemAccessor.CopyFile(CreateContext(
                subsystem,
                SshFileSystemOperation.CopyFile,
                path: src,
                sourcePath: src?.toString(),
                destinationPath: dst?.toString(),
                options: ToOptionList(opts)));

            accessorDelegate.copyFile(subsystem, src, dst, opts);
        }

        public void removeFile(SftpSubsystemProxy subsystem, Path path, bool isDirectory)
        {
            logger.Debug($"removeFile({path}, isDirectory={isDirectory})");
            fileSystemAccessor.RemoveFile(CreateContext(
                subsystem,
                SshFileSystemOperation.RemoveFile,
                path: path,
                isDirectory: isDirectory));

            accessorDelegate.removeFile(subsystem, path, isDirectory);
        }

        public bool noFollow(Collection opts)
        {
            bool defaultNoFollow = accessorDelegate.noFollow(opts);
            var context = new SshFileSystemAccess
            {
                Operation = SshFileSystemOperation.NoFollow,
                Options = ToOptionList(opts)
            };

            return fileSystemAccessor.NoFollow(context, defaultNoFollow);
        }

        private void ValidateSymlinkWithRealPath(SftpSubsystemProxy subsystem, Path finalPath, Path rootDir, string remotePath)
        {
            Path effectivePath = finalPath;
            Path effectiveRoot = rootDir;

            try
            {
                if (InternalVirtualFileSystemFactory.RealUserHomes.TryGetValue(
                        subsystem.getSession().getUsername(), out string? realUserHome))
                {
                    string pathStr = effectivePath.toString();
                    // Convert virtual paths (Unix-style starting with '/') to real paths.
                    // On Windows, real paths start with a drive letter so '/' always means virtual.
                    // On Linux, both virtual and real paths start with '/', so we verify the path
                    // is not already within the real root to avoid double-conversion.
                    if (pathStr.StartsWith('/') && IsVirtualNotRealPath(pathStr, realUserHome))
                    {
                        string relativePath = pathStr.TrimStart('/');
                        string realFilePath = System.IO.Path.GetFullPath(
                            System.IO.Path.Combine(realUserHome, relativePath));
                        string realRootPath = System.IO.Path.GetFullPath(realUserHome);

                        effectivePath = Paths.get(realFilePath);
                        effectiveRoot = Paths.get(realRootPath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(InternalSftpFileSystemAccessor)}] Failed to resolve real user home: {ex.Message}");
            }

            ValidateSymlinkContainment(effectivePath, effectiveRoot);
        }

        /// <summary>
        /// Determines whether a Unix-style path (starting with '/') is a virtual path
        /// rather than an already-resolved real path. On Windows, all paths starting with
        /// '/' are virtual. On Linux, we check that the path does not already fall within
        /// the real user home directory.
        /// </summary>
        private static bool IsVirtualNotRealPath(string path, string realUserHome)
        {
            if (OperatingSystem.IsWindows())
            {
                return true;
            }

            // On Linux, real paths also start with '/'. Check if this path is already
            // a real path by verifying it isn't the real root or a child of it.
            if (path.Length <= realUserHome.Length)
            {
                return !path.Equals(realUserHome, StringComparison.Ordinal);
            }

            return !path.StartsWith(realUserHome + "/", StringComparison.Ordinal);
        }

        private void ValidateSymlinkContainment(Path filePath, Path rootDir)
        {
            string pathStr = filePath.toString();
            string rootStr = rootDir.toAbsolutePath().normalize().toString();

            try
            {
                Path resolvedPath = filePath.toRealPath();
                if (!IsPathWithinRoot(resolvedPath, rootDir))
                {
                    throw new NoSuchFileException(null, null,
                        "Resolved path is outside the allowed root directory.");
                }

                if (!resolvedPath.equals(filePath))
                {
                    return;
                }
            }
            catch (java.io.IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(InternalSftpFileSystemAccessor)}] toRealPath() failed for '{pathStr}': {ex.getMessage()}");
            }

            if (OperatingSystem.IsWindows() && TryResolveSymlinkTargetViaNativeApi(pathStr, out string? nativeTarget))
            {
                if (nativeTarget == null || !IsTargetWithinRoot(nativeTarget, rootStr))
                {
                    throw new NoSuchFileException(null, null,
                        "Symlink target is outside the allowed root directory.");
                }

                return;
            }

            try
            {
                if (Files.isSymbolicLink(filePath))
                {
                    Path linkTarget = Files.readSymbolicLink(filePath);
                    if (!linkTarget.isAbsolute())
                    {
                        linkTarget = filePath.getParent().resolve(linkTarget).normalize();
                    }
                    if (!IsPathWithinRoot(linkTarget, rootDir))
                    {
                        throw new NoSuchFileException(null, null,
                            "Symlink target is outside the allowed root directory.");
                    }
                }
            }
            catch (java.io.IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(InternalSftpFileSystemAccessor)}] readSymbolicLink() failed for '{pathStr}': {ex.getMessage()}");
            }

            // .NET-based symlink detection (most reliable on Windows for IKVM interop)
            string? dotNetResolvedTarget = null;
            try
            {
                string dotNetPath = filePath.toString();
                var symlinkTarget = File.ResolveLinkTarget(dotNetPath, true);
                if (symlinkTarget != null)
                {
                    dotNetResolvedTarget = System.IO.Path.GetFullPath(symlinkTarget.FullName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(InternalSftpFileSystemAccessor)}] .NET symlink resolution failed for '{filePath}': {ex.Message}");
            }

            if (dotNetResolvedTarget != null)
            {
                string normalizedRoot = EnsureSeparatorSuffix(System.IO.Path.GetFullPath(rootStr));
                if (!dotNetResolvedTarget.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new NoSuchFileException(null, null,
                        "Symlink target is outside the allowed root directory.");
                }
            }
        }

        private static bool TryResolveSymlinkTargetViaNativeApi(string path, out string? target)
        {
            target = null;

            WIN32_FIND_DATA findData;
            IntPtr hFind = FindFirstFile(path, out findData);
            if (hFind == INVALID_HANDLE_VALUE)
            {
                return false;
            }

            FindClose(hFind);

            if ((findData.dwFileAttributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
            {
                return false;
            }

            string? detectedTarget = null;

            if (findData.dwReserved0 == IO_REPARSE_TAG_SYMLINK)
            {
                detectedTarget = ResolveSymlinkTargetViaDeviceIoControl(path);
            }

            if (detectedTarget == null)
            {
                return false;
            }

            string resolvedSubstituteName = ResolveNtPathName(detectedTarget);
            string dir = System.IO.Path.GetDirectoryName(path) ?? string.Empty;
            target = System.IO.Path.GetFullPath(resolvedSubstituteName, dir);
            return true;
        }

        private static string? ResolveSymlinkTargetViaDeviceIoControl(string path)
        {
            SafeFileHandle? handle = null;
            try
            {
                handle = CreateOpenReparsePoint(path);
                if (handle.IsInvalid)
                {
                    return null;
                }

                var outBuf = new byte[REPARSE_DATA_BUFFER_SIZE];
                GCHandle pin = GCHandle.Alloc(outBuf, GCHandleType.Pinned);
                try
                {
                    uint bytesReturned;
                    bool result = DeviceIoControl(
                        handle,
                        FSCTL_GET_REPARSE_POINT,
                        IntPtr.Zero,
                        0,
                        pin.AddrOfPinnedObject(),
                        REPARSE_DATA_BUFFER_SIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                    if (!result || bytesReturned < REPARSE_DATA_HEADER_SIZE)
                    {
                        return null;
                    }

                    uint reparseTag = BitConverter.ToUInt32(outBuf, 0);
                    if (reparseTag != IO_REPARSE_TAG_SYMLINK)
                    {
                        return null;
                    }

                    ushort substituteNameOffset = BitConverter.ToUInt16(outBuf, SYMLINK_SUBST_NAME_OFFSET);
                    ushort substituteNameLength = BitConverter.ToUInt16(outBuf, SYMLINK_SUBST_NAME_LENGTH);
                    return Encoding.Unicode.GetString(
                        outBuf, SYMLINK_PATH_BUFFER_OFFSET + substituteNameOffset, substituteNameLength);
                }
                finally
                {
                    pin.Free();
                }
            }
            finally
            {
                handle?.Dispose();
            }
        }

        private static SafeFileHandle CreateOpenReparsePoint(string path)
        {
            return CreateFile(
                path,
                GENERIC_READ,
                FILE_SHARE_READ,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_FLAG_OPEN_REPARSE_POINT,
                IntPtr.Zero);
        }

        private static string ResolveNtPathName(string ntPath)
        {
            if (ntPath.StartsWith(@"\??\", StringComparison.Ordinal))
            {
                return ntPath.Substring(4);
            }

            if (ntPath.StartsWith(@"\GLOBAL??\", StringComparison.Ordinal))
            {
                return ntPath.Substring(9);
            }

            if (ntPath.StartsWith(@"\\?\", StringComparison.Ordinal))
            {
                return ntPath.Substring(4);
            }

            return ntPath;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public uint ftCreationTime_dwLowDateTime;
            public uint ftCreationTime_dwHighDateTime;
            public uint ftLastAccessTime_dwLowDateTime;
            public uint ftLastAccessTime_dwHighDateTime;
            public uint ftLastWriteTime_dwLowDateTime;
            public uint ftLastWriteTime_dwHighDateTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        private const uint FSCTL_GET_REPARSE_POINT = 0x000900A8;
        private const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
        private const uint REPARSE_DATA_BUFFER_SIZE = 16384;
        private const int SYMLINK_REPARSE_BUFFER_OFFSET = 8;
        private const int SYMLINK_SUBST_NAME_OFFSET = SYMLINK_REPARSE_BUFFER_OFFSET + 0;
        private const int SYMLINK_SUBST_NAME_LENGTH = SYMLINK_REPARSE_BUFFER_OFFSET + 2;
        private const int SYMLINK_PATH_BUFFER_OFFSET = SYMLINK_REPARSE_BUFFER_OFFSET + 12;
        private const int REPARSE_DATA_HEADER_SIZE = 8;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private static bool IsTargetWithinRoot(string targetPath, string rootPath)
        {
            rootPath = rootPath.TrimEnd('/', '\\');

            if (!targetPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (targetPath.Length == rootPath.Length)
            {
                return true;
            }

            char next = targetPath[rootPath.Length];
            return next == '/' || next == '\\';
        }

        private static string EnsureSeparatorSuffix(string path)
        {
            if (path.Length > 0 && path[path.Length - 1] != '\\' && path[path.Length - 1] != '/')
            {
                return path + System.IO.Path.DirectorySeparatorChar;
            }

            return path;
        }

        private static bool IsPathWithinRoot(Path path, Path rootDir)
        {
            string pathStr = path.toAbsolutePath().normalize().toString();
            string rootStr = rootDir.toAbsolutePath().normalize().toString()
                .TrimEnd('/', '\\');

            if (!pathStr.StartsWith(rootStr, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (pathStr.Length == rootStr.Length)
            {
                return true;
            }

            char next = pathStr[rootStr.Length];
            return next == '/' || next == '\\';
        }

        private static Path ToPath(string managedPath, Path fallback)
        {
            if (string.IsNullOrWhiteSpace(managedPath) || managedPath == fallback.toString())
            {
                return fallback;
            }

            return Paths.get(managedPath);
        }

        private static LinkOption[] ToLinkOptions(IReadOnlyList<string> options, LinkOption[] fallback)
        {
            if (options.SequenceEqual(ToOptionList(fallback)))
            {
                return fallback;
            }

            var mapped = new System.Collections.Generic.List<LinkOption>();
            foreach (string option in options)
            {
                if (option.Contains("NOFOLLOW", StringComparison.OrdinalIgnoreCase))
                {
                    mapped.Add(LinkOption.NOFOLLOW_LINKS);
                }
            }

            if (mapped.Count != options.Count)
            {
                return fallback;
            }

            return mapped.ToArray();
        }

        private static IReadOnlyList<string> ToOptionList(params LinkOption[] options)
        {
            return options?.Select(option => option?.ToString() ?? string.Empty)
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .ToArray() ?? Array.Empty<string>();
        }

        private static IReadOnlyList<string> ToOptionList(Collection options)
        {
            if (options == null)
            {
                return Array.Empty<string>();
            }

            var list = new System.Collections.Generic.List<string>();
            var iterator = options.iterator();
            while (iterator.hasNext())
            {
                list.Add(iterator.next()?.ToString() ?? string.Empty);
            }

            return list;
        }

        private static IReadOnlyDictionary<string, object> ToDictionary(Map? map)
        {
            var dict = new Dictionary<string, object>();
            if (map == null)
            {
                return dict;
            }

            var iterator = map.entrySet().iterator();
            while (iterator.hasNext())
            {
                var entry = (Map.Entry)iterator.next();
                dict[entry.getKey()?.ToString() ?? string.Empty] = entry.getValue();
            }

            return dict;
        }

        private static IReadOnlyDictionary<string, object> ToDictionary(params FileAttribute[] attrs)
        {
            var dict = new Dictionary<string, object>();
            if (attrs == null)
            {
                return dict;
            }

            foreach (FileAttribute attr in attrs)
            {
                dict[attr.name()] = attr.value();
            }

            return dict;
        }

        private static IReadOnlyDictionary<string, object> ToNamedValues(string key, IReadOnlyList<string> values)
        {
            return new Dictionary<string, object>
            {
                [key] = values
            };
        }

        private static Map ToMap(IReadOnlyDictionary<string, object> values)
        {
            Map map = new TreeMap();
            foreach (var pair in values)
            {
                map.put(pair.Key, pair.Value);
            }

            return map;
        }

        private static NavigableMap ToNavigableMap(IReadOnlyDictionary<string, object> values)
        {
            NavigableMap map = new TreeMap();
            foreach (var pair in values)
            {
                map.put(pair.Key, pair.Value);
            }

            return map;
        }

        private static ISshSession? TryCreateSession(SftpSubsystemProxy subsystem)
        {
            try
            {
                return subsystem?.getSession() == null
                    ? null
                    : new SshSession(subsystem.getSession());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(InternalSftpFileSystemAccessor)}] Failed to create session wrapper: {ex.Message}");
                return null;
            }
        }

        private static ISshFileSystemAccess CreateContext(
            SftpSubsystemProxy subsystem,
            SshFileSystemOperation operation,
            Path? path = null,
            Path? rootDir = null,
            string? remotePath = null,
            string? remoteHandle = null,
            string? sourcePath = null,
            string? destinationPath = null,
            string? remoteName = null,
            string? extension = null,
            string? fileAttributeView = null,
            string? fileAttributeName = null,
            string? owner = null,
            string? group = null,
            object? value = null,
            bool isDirectory = false,
            bool isSymbolicLink = false,
            bool shortName = false,
            bool followLinks = false,
            bool sharedLock = false,
            int command = 0,
            long offset = 0,
            long length = 0,
            IReadOnlyList<string>? options = null,
            Map? attrs = null,
            IReadOnlyDictionary<string, object>? attributes = null)
        {
            return new SshFileSystemAccess
            {
                Operation = operation,
                Session = TryCreateSession(subsystem),
                RootPath = rootDir?.toString(),
                RemotePath = remotePath,
                LocalPath = path?.toString(),
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                RemoteHandle = remoteHandle,
                RemoteName = remoteName,
                Extension = extension,
                FileAttributeView = fileAttributeView,
                FileAttributeName = fileAttributeName,
                Owner = owner,
                Group = group,
                Value = value,
                IsDirectory = isDirectory,
                IsSymbolicLink = isSymbolicLink,
                ShortName = shortName,
                FollowLinks = followLinks,
                SharedLock = sharedLock,
                Command = command,
                Offset = offset,
                Length = length,
                Options = options ?? Array.Empty<string>(),
                Attributes = attributes ?? ToDictionary(attrs)
            };
        }
    }
}
