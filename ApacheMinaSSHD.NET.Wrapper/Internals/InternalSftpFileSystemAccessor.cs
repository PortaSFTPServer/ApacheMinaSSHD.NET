using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.FileSystem;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.nio.channels;
using java.nio.file;
using java.nio.file.attribute;
using java.security;
using java.util;
using org.apache.sshd.sftp.server;
using Path = java.nio.file.Path;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalSftpFileSystemAccessor : SftpFileSystemAccessor
    {
        private readonly SftpFileSystemAccessor accessorDelegate;
        private readonly IAMNetSftpFileSystemAccessor fileSystemAccessor;

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

            var validationContext = CreateContext(
                subsystem,
                SshFileSystemOperation.ResolveLocalFilePath,
                path: finalPath,
                rootDir: rootDir,
                remotePath: remotePath);

            if (!fileSystemAccessor.IsPathAllowed(validationContext))
            {
                throw new NoSuchFileException(finalPath.toString(), null, "File or directory is not allowed.");
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
            fileSystemAccessor.OpenFile(CreateContext(
                subsystem,
                SshFileSystemOperation.OpenFile,
                path: file,
                remoteHandle: handle,
                options: ToOptionList(options),
                attributes: ToDictionary(attrs)));

            try
            {
                return accessorDelegate.openFile(subsystem, fileHandle, file, handle, options, attrs);
            }
            catch (java.lang.UnsupportedOperationException) when (attrs?.Length > 0)
            {
                // Some Windows providers reject create-time attributes sent by clients such as WinSCP.
                // Retry without attributes so the open can succeed while later policy hooks still run.
                return accessorDelegate.openFile(subsystem, fileHandle, file, handle, options);
            }
            catch (NotSupportedException) when (attrs?.Length > 0)
            {
                // Same compatibility path as the Java exception case, surfaced through IKVM as .NET.
                return accessorDelegate.openFile(subsystem, fileHandle, file, handle, options);
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
            fileSystemAccessor.CreateDirectory(CreateContext(
                subsystem,
                SshFileSystemOperation.CreateDirectory,
                path: path,
                isDirectory: true));

            accessorDelegate.createDirectory(subsystem, path);
        }

        public void createLink(SftpSubsystemProxy subsystem, Path link, Path existing, bool symLink)
        {
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
            catch
            {
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
