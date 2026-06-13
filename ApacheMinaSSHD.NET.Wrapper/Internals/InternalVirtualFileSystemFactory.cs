// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Factories;
using java.nio.file;
using org.apache.sshd.common.file;
using org.apache.sshd.common.file.virtualfs;
using org.apache.sshd.common.session;
using System.Collections.Concurrent;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalVirtualFileSystemFactory : java.lang.Object, FileSystemFactory
    {
        private readonly AMNetVirtualFileSystemFactory fileSystemFactory;

        /// <summary>
        /// Maps authenticated usernames to their real (Windows) home directory paths.
        /// Used by <see cref="InternalSftpFileSystemAccessor"/> to convert virtual paths to
        /// real filesystem paths for symlink containment validation.
        /// </summary>
        internal static readonly ConcurrentDictionary<string, string> RealUserHomes = new();

        public InternalVirtualFileSystemFactory(AMNetVirtualFileSystemFactory fileSystemFactory)
        {
            this.fileSystemFactory = fileSystemFactory;
        }

        java.nio.file.FileSystem FileSystemFactory.createFileSystem(SessionContext sessionContext)
        {
            // Store the real user home path so that symlink detection can use real Windows paths.
            string userHome = fileSystemFactory.ResolveUserHomeDirectory(sessionContext.getUsername());
            RealUserHomes[sessionContext.getUsername()] = userHome;

            return CreateVirtualFileSystemFactory(sessionContext).createFileSystem(sessionContext);
        }

        public java.nio.file.Path getUserHomeDir(SessionContext sessionContext)
        {
            return CreateVirtualFileSystemFactory(sessionContext).getUserHomeDir(sessionContext);
        }

        private VirtualFileSystemFactory CreateVirtualFileSystemFactory(SessionContext sessionContext)
        {
            return new VirtualFileSystemFactory(ResolveUserHome(sessionContext));
        }

        private java.nio.file.Path ResolveUserHome(SessionContext sessionContext)
        {
            var username = sessionContext.getUsername();
            var userHome = fileSystemFactory.ResolveUserHomeDirectory(username);

            if (fileSystemFactory.CreateUserDirectory)
            {
                Directory.CreateDirectory(userHome);
            }

            return Paths.get(userHome);
        }
    }
}
