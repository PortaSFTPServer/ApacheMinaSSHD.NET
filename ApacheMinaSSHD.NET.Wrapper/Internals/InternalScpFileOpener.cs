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
using ApacheMinaSSHD.NET.Wrapper.FileSystem;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using java.nio.file;
using java.nio.file.attribute;
using org.apache.sshd.common.session;
using org.apache.sshd.scp.common;
using org.apache.sshd.scp.common.helpers;
using JavaArrayList = java.util.ArrayList;
using JavaCollection = java.util.Collection;
using JavaFileSystem = java.nio.file.FileSystem;
using JavaHashSet = java.util.HashSet;
using JavaInputStream = java.io.InputStream;
using JavaIterable = java.lang.Iterable;
using JavaOutputStream = java.io.OutputStream;
using JavaSet = java.util.Set;
using Path = java.nio.file.Path;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalScpFileOpener : java.lang.Object, ScpFileOpener
    {
        private readonly ScpFileOpener openerDelegate = DefaultScpFileOpener.INSTANCE;
        private readonly IAMNetScpFileOpener fileOpener;
        static readonly IAMNetLogger logger = new AMNetLogger(typeof(InternalScpFileOpener), AMNetLogger.LogLevel.Info);

        public InternalScpFileOpener(IAMNetScpFileOpener fileOpener)
        {
            this.fileOpener = fileOpener ?? throw new ArgumentNullException(nameof(fileOpener));
        }

        public Path resolveIncomingFilePath(
            Session session,
            Path localPath,
            string name,
            bool preserve,
            JavaSet permissions,
            ScpTimestampCommandDetails time)
        {
            logger.Debug($"resolveIncomingFilePath({localPath}, name={name})");
            Path resolvedPath = openerDelegate.resolveIncomingFilePath(
                session,
                localPath,
                name,
                preserve,
                permissions,
                time);

            var access = CreateAccess(
                session,
                SshScpFileOperation.ResolveIncomingFilePath,
                localPath: resolvedPath,
                rootPath: localPath,
                fileName: name,
                preserveTimestamp: preserve,
                permissions: ToValueList(permissions),
                command: time?.toString());

            string managedPath = fileOpener.ResolveIncomingFilePath(access, resolvedPath.toString());
            Path finalPath = ToPath(managedPath, resolvedPath);
            EnsurePathAllowed(CreateAccess(
                session,
                SshScpFileOperation.ResolveIncomingFilePath,
                localPath: finalPath,
                rootPath: localPath,
                fileName: name,
                preserveTimestamp: preserve,
                permissions: ToValueList(permissions),
                command: time?.toString()));

            return finalPath;
        }

        public JavaIterable getMatchingFilesToSend(Session session, Path basedir, string pattern)
        {
            JavaIterable resolvedFiles = openerDelegate.getMatchingFilesToSend(session, basedir, pattern);
            IReadOnlyList<string> resolvedPaths = ToPathStringList(resolvedFiles);

            var access = CreateAccess(
                session,
                SshScpFileOperation.GetMatchingFilesToSend,
                rootPath: basedir,
                pattern: pattern);

            IReadOnlyList<string> managedPaths = fileOpener.GetMatchingFilesToSend(access, resolvedPaths);
            foreach (string managedPath in managedPaths)
            {
                EnsurePathAllowed(CreateAccess(
                    session,
                    SshScpFileOperation.GetMatchingFilesToSend,
                    localPath: ToPath(managedPath, basedir),
                    rootPath: basedir,
                    pattern: pattern));
            }

            return ToPathIterable(managedPaths, resolvedFiles);
        }

        public bool sendAsRegularFile(Session session, Path file, params LinkOption[] options)
        {
            var access = CreateAccess(
                session,
                SshScpFileOperation.SendAsRegularFile,
                localPath: file,
                options: ToOptionList(options));

            EnsurePathAllowed(access);

            bool defaultValue = openerDelegate.sendAsRegularFile(session, file, options);
            return fileOpener.ShouldSendAsRegularFile(access, defaultValue);
        }

        public bool sendAsDirectory(Session session, Path file, params LinkOption[] options)
        {
            var access = CreateAccess(
                session,
                SshScpFileOperation.SendAsDirectory,
                localPath: file,
                isDirectory: true,
                options: ToOptionList(options));

            EnsurePathAllowed(access);

            bool defaultValue = openerDelegate.sendAsDirectory(session, file, options);
            return fileOpener.ShouldSendAsDirectory(access, defaultValue);
        }

        public DirectoryStream getLocalFolderChildren(Session session, Path folder)
        {
            var access = CreateAccess(
                session,
                SshScpFileOperation.GetLocalFolderChildren,
                localPath: folder,
                isDirectory: true);

            EnsurePathAllowed(access);

            DirectoryStream stream = openerDelegate.getLocalFolderChildren(session, folder);
            return new FilteredDirectoryStream(
                stream,
                entry => fileOpener.ShouldIncludeDirectoryEntry(CreateAccess(
                    session,
                    SshScpFileOperation.GetLocalFolderChildren,
                    localPath: entry,
                    rootPath: folder,
                    isDirectory: Directory.Exists(entry.toString()))));
        }

        public BasicFileAttributes getLocalBasicFileAttributes(
            Session session,
            Path file,
            params LinkOption[] options)
        {
            EnsurePathAllowed(CreateAccess(
                session,
                SshScpFileOperation.GetLocalBasicFileAttributes,
                localPath: file,
                options: ToOptionList(options)));

            BasicFileAttributes attributes = openerDelegate.getLocalBasicFileAttributes(session, file, options);
            var access = CreateAccess(
                session,
                SshScpFileOperation.GetLocalBasicFileAttributes,
                localPath: file,
                isDirectory: attributes.isDirectory(),
                length: attributes.size(),
                options: ToOptionList(options),
                attributes: ToDictionary(attributes));

            fileOpener.ReadLocalBasicFileAttributes(access, access.Attributes);

            return attributes;
        }

        public JavaSet getLocalFilePermissions(Session session, Path file, params LinkOption[] options)
        {
            EnsurePathAllowed(CreateAccess(
                session,
                SshScpFileOperation.GetLocalFilePermissions,
                localPath: file,
                options: ToOptionList(options)));

            JavaSet permissions = openerDelegate.getLocalFilePermissions(session, file, options);
            var access = CreateAccess(
                session,
                SshScpFileOperation.GetLocalFilePermissions,
                localPath: file,
                permissions: ToValueList(permissions),
                options: ToOptionList(options));

            IReadOnlyList<string> managedPermissions = fileOpener.GetLocalFilePermissions(
                access,
                access.Permissions);

            return ToPermissions(managedPermissions, permissions);
        }

        public Path resolveLocalPath(Session session, JavaFileSystem fileSystem, string commandPath)
        {
            // Resolve through MINA first so SCP command syntax is handled consistently,
            // then give managed policy a clean local path to rewrite or reject.
            Path resolvedPath = openerDelegate.resolveLocalPath(session, fileSystem, commandPath);
            var access = CreateAccess(
                session,
                SshScpFileOperation.ResolveLocalPath,
                localPath: resolvedPath,
                requestedPath: commandPath);

            string managedPath = fileOpener.ResolveLocalPath(access, resolvedPath.toString());
            Path finalPath = ToPath(managedPath, resolvedPath);
            EnsurePathAllowed(CreateAccess(
                session,
                SshScpFileOperation.ResolveLocalPath,
                localPath: finalPath,
                requestedPath: commandPath));

            return finalPath;
        }

        public Path resolveIncomingReceiveLocation(
            Session session,
            Path localPath,
            bool recursive,
            bool shouldBeDir,
            bool preserve)
        {
            Path resolvedPath = openerDelegate.resolveIncomingReceiveLocation(
                session,
                localPath,
                recursive,
                shouldBeDir,
                preserve);

            var access = CreateAccess(
                session,
                SshScpFileOperation.ResolveIncomingReceiveLocation,
                localPath: resolvedPath,
                rootPath: localPath,
                recursive: recursive,
                shouldBeDirectory: shouldBeDir,
                preserveTimestamp: preserve);

            string managedPath = fileOpener.ResolveIncomingReceiveLocation(access, resolvedPath.toString());
            Path finalPath = ToPath(managedPath, resolvedPath);
            EnsurePathAllowed(CreateAccess(
                session,
                SshScpFileOperation.ResolveIncomingReceiveLocation,
                localPath: finalPath,
                rootPath: localPath,
                recursive: recursive,
                shouldBeDirectory: shouldBeDir,
                preserveTimestamp: preserve));

            return finalPath;
        }

        public Path resolveOutgoingFilePath(Session session, Path localPath, params LinkOption[] options)
        {
            Path resolvedPath = openerDelegate.resolveOutgoingFilePath(session, localPath, options);
            var access = CreateAccess(
                session,
                SshScpFileOperation.ResolveOutgoingFilePath,
                localPath: resolvedPath,
                rootPath: localPath,
                options: ToOptionList(options));

            string managedPath = fileOpener.ResolveOutgoingFilePath(access, resolvedPath.toString());
            Path finalPath = ToPath(managedPath, resolvedPath);
            EnsurePathAllowed(CreateAccess(
                session,
                SshScpFileOperation.ResolveOutgoingFilePath,
                localPath: finalPath,
                rootPath: localPath,
                options: ToOptionList(options)));

            return finalPath;
        }

        public JavaInputStream openRead(
            Session session,
            Path file,
            long size,
            JavaSet permissions,
            params OpenOption[] options)
        {
            logger.Debug($"openRead({file}, size={size})");
            var access = CreateAccess(
                session,
                SshScpFileOperation.OpenRead,
                localPath: file,
                length: size,
                permissions: ToValueList(permissions),
                options: ToOptionList(options));

            EnsurePathAllowed(access);
            fileOpener.OpenRead(access);

            return openerDelegate.openRead(session, file, size, permissions, options);
        }

        public void closeRead(
            Session session,
            Path file,
            long size,
            JavaSet permissions,
            JavaInputStream stream)
        {
            logger.Debug($"closeRead({file}, size={size})");
            var access = CreateAccess(
                session,
                SshScpFileOperation.CloseRead,
                localPath: file,
                length: size,
                permissions: ToValueList(permissions));

            try
            {
                fileOpener.CloseRead(access);
            }
            finally
            {
                openerDelegate.closeRead(session, file, size, permissions, stream);
            }
        }

        public ScpSourceStreamResolver createScpSourceStreamResolver(Session session, Path file)
        {
            var access = CreateAccess(
                session,
                SshScpFileOperation.CreateSourceStreamResolver,
                localPath: file);

            EnsurePathAllowed(access);
            fileOpener.CreateSourceStreamResolver(access);

            return openerDelegate.createScpSourceStreamResolver(session, file);
        }

        public JavaOutputStream openWrite(
            Session session,
            Path file,
            long size,
            JavaSet permissions,
            params OpenOption[] options)
        {
            logger.Debug($"openWrite({file}, size={size})");
            var access = CreateAccess(
                session,
                SshScpFileOperation.OpenWrite,
                localPath: file,
                length: size,
                permissions: ToValueList(permissions),
                options: ToOptionList(options));

            EnsurePathAllowed(access);
            fileOpener.OpenWrite(access);

            return openerDelegate.openWrite(session, file, size, permissions, options);
        }

        public void closeWrite(
            Session session,
            Path file,
            long size,
            JavaSet permissions,
            JavaOutputStream stream)
        {
            logger.Debug($"closeWrite({file}, size={size})");
            var access = CreateAccess(
                session,
                SshScpFileOperation.CloseWrite,
                localPath: file,
                length: size,
                permissions: ToValueList(permissions));

            try
            {
                fileOpener.CloseWrite(access);
            }
            finally
            {
                openerDelegate.closeWrite(session, file, size, permissions, stream);
            }
        }

        public ScpTargetStreamResolver createScpTargetStreamResolver(Session session, Path file)
        {
            var access = CreateAccess(
                session,
                SshScpFileOperation.CreateTargetStreamResolver,
                localPath: file);

            EnsurePathAllowed(access);
            fileOpener.CreateTargetStreamResolver(access);

            return openerDelegate.createScpTargetStreamResolver(session, file);
        }

        public string? checkRemoteFileName(JavaFileSystem fileSystem, string fileName)
        {
            string defaultValue = openerDelegate.checkRemoteFileName(fileSystem, fileName);
            if (defaultValue == null)
            {
                return null;
            }

            var access = CreateAccess(
                default!,
                SshScpFileOperation.CheckRemoteFileName,
                fileName: fileName);

            return fileOpener.CheckRemoteFileName(access, defaultValue) ?? defaultValue;
        }

        private void EnsurePathAllowed(ISshScpFileAccess access)
        {
            if (!fileOpener.IsPathAllowed(access))
            {
                // Throw a filesystem-shaped denial so SCP clients receive a normal transfer failure.
                throw new AccessDeniedException(
                    access.LocalPath ?? access.RequestedPath ?? string.Empty,
                    null,
                    "SCP path is not allowed.");
            }
        }

        private static Path ToPath(string? managedPath, Path fallback)
        {
            if (string.IsNullOrWhiteSpace(managedPath) || managedPath == fallback.toString())
            {
                return fallback;
            }

            return Paths.get(managedPath);
        }

        private static JavaIterable ToPathIterable(IReadOnlyList<string>? paths, JavaIterable fallback)
        {
            if (paths == null)
            {
                return fallback;
            }

            var result = new JavaArrayList();
            foreach (string path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    result.add(Paths.get(path));
                }
            }

            return result;
        }

        private static IReadOnlyList<string> ToPathStringList(JavaIterable? paths)
        {
            if (paths == null)
            {
                return Array.Empty<string>();
            }

            var result = new System.Collections.Generic.List<string>();
            var iterator = paths.iterator();
            while (iterator.hasNext())
            {
                result.Add(((Path)iterator.next()).toString());
            }

            return result;
        }

        private static IReadOnlyList<string> ToOptionList(params LinkOption[] options)
        {
            return options?.Select(option => option?.ToString() ?? string.Empty)
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .ToArray() ?? Array.Empty<string>();
        }

        private static IReadOnlyList<string> ToOptionList(params OpenOption[] options)
        {
            return options?.Select(option => option?.ToString() ?? string.Empty)
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .ToArray() ?? Array.Empty<string>();
        }

        private static IReadOnlyList<string> ToValueList(JavaCollection? values)
        {
            if (values == null)
            {
                return Array.Empty<string>();
            }

            var result = new System.Collections.Generic.List<string>();
            var iterator = values.iterator();
            while (iterator.hasNext())
            {
                result.Add(iterator.next()?.ToString() ?? string.Empty);
            }

            return result;
        }

        private static JavaSet ToPermissions(IReadOnlyList<string>? permissions, JavaSet fallback)
        {
            if (permissions == null || permissions.SequenceEqual(ToValueList(fallback)))
            {
                return fallback;
            }

            var result = new JavaHashSet();
            foreach (string permission in permissions)
            {
                try
                {
                    result.add(PosixFilePermission.valueOf(permission));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[{nameof(InternalScpFileOpener)}] Failed to parse permission '{permission}': {ex.Message}");
                    return fallback;
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, object> ToDictionary(BasicFileAttributes? attributes)
        {
            if (attributes == null)
            {
                return new Dictionary<string, object>();
            }

            return new Dictionary<string, object>
            {
                ["creationTime"] = attributes.creationTime(),
                ["fileKey"] = attributes.fileKey() ?? string.Empty,
                ["isDirectory"] = attributes.isDirectory(),
                ["isOther"] = attributes.isOther(),
                ["isRegularFile"] = attributes.isRegularFile(),
                ["isSymbolicLink"] = attributes.isSymbolicLink(),
                ["lastAccessTime"] = attributes.lastAccessTime(),
                ["lastModifiedTime"] = attributes.lastModifiedTime(),
                ["size"] = attributes.size()
            };
        }

        private static ISshSession? TryCreateSession(Session session)
        {
            try
            {
                return session is org.apache.sshd.server.session.ServerSession serverSession
                    ? new SshSession(serverSession)
                    : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(InternalScpFileOpener)}] Failed to create session wrapper: {ex.Message}");
                return null;
            }
        }

        private static ISshScpFileAccess CreateAccess(
            Session session,
            SshScpFileOperation operation,
            Path? localPath = null,
            Path? rootPath = null,
            string? requestedPath = null,
            string? fileName = null,
            string? pattern = null,
            string? command = null,
            bool recursive = false,
            bool shouldBeDirectory = false,
            bool preserveTimestamp = false,
            bool isDirectory = false,
            long length = 0,
            IReadOnlyList<string>? permissions = null,
            IReadOnlyList<string>? options = null,
            IReadOnlyDictionary<string, object>? attributes = null)
        {
            return new SshScpFileAccess
            {
                Operation = operation,
                Session = TryCreateSession(session),
                RootPath = rootPath?.toString(),
                LocalPath = localPath?.toString(),
                RequestedPath = requestedPath,
                FileName = fileName,
                Pattern = pattern,
                Command = command,
                Recursive = recursive,
                ShouldBeDirectory = shouldBeDirectory,
                PreserveTimestamp = preserveTimestamp,
                IsDirectory = isDirectory,
                Length = length,
                Permissions = permissions ?? Array.Empty<string>(),
                Options = options ?? Array.Empty<string>(),
                Attributes = attributes ?? new Dictionary<string, object>()
            };
        }
    }
}
