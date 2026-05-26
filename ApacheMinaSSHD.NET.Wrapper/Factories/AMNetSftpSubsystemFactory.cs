using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals;
using org.apache.sshd.sftp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    /// <summary>
    /// Configures the SFTP subsystem for an <see cref="AMNetSshServer"/>.
    /// </summary>
    public class AMNetSftpSubsystemFactory
    {
        private readonly SftpSubsystemFactory factory = new();

        /// <summary>
        /// Creates an SFTP subsystem factory.
        /// </summary>
        public AMNetSftpSubsystemFactory()
        {
        }

        internal SftpSubsystemFactory JavaFactory => factory;

        /// <summary>
        /// Registers an SFTP event listener.
        /// </summary>
        /// <param name="sftpEventListener">The listener that receives SFTP lifecycle and file events.</param>
        public void addSftpEventListener(IAMNetSftpEventListener sftpEventListener)
        {
            ArgumentNullException.ThrowIfNull(sftpEventListener);
            factory.addSftpEventListener(new InternalSftpEventListener(sftpEventListener));
        }

        /// <summary>
        /// Sets the SFTP filesystem policy hook.
        /// </summary>
        /// <param name="sftpFileSystemAccessor">The SFTP policy hook for paths, attributes, and filesystem operations.</param>
        public void setFileSystemAccessor(IAMNetSftpFileSystemAccessor sftpFileSystemAccessor)
        {
            ArgumentNullException.ThrowIfNull(sftpFileSystemAccessor);
            factory.setFileSystemAccessor(new InternalSftpFileSystemAccessor(sftpFileSystemAccessor, this));
        }
    }
}
