using ApacheMinaSSHD.NET.Wrapper.Factories;
using java.nio.file;
using org.apache.sshd.common.file;
using org.apache.sshd.common.file.virtualfs;
using org.apache.sshd.common.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalVirtualFileSystemFactory : java.lang.Object, FileSystemFactory
    {
        private readonly AMNetVirtualFileSystemFactory fileSystemFactory;

        public InternalVirtualFileSystemFactory(AMNetVirtualFileSystemFactory fileSystemFactory)
        {
            this.fileSystemFactory = fileSystemFactory;
        }

        java.nio.file.FileSystem FileSystemFactory.createFileSystem(SessionContext sessionContext)
        {
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
