using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default SFTP event listener that logs SFTP lifecycle and file operation events.
    /// </summary>
    public class AMNetSftpEventListener : IAMNetSftpEventListener
    {
        IAMNetLogger logger = new AMNetLogger(typeof(AMNetSftpEventListener), AMNetLogger.LogLevel.info);

        /// <summary>
        /// Creates a default SFTP event listener.
        /// </summary>
        public AMNetSftpEventListener()
        {
        }

        /// <inheritdoc />
        public virtual void OnModifyingAttributes(ISshPath ctx)
        {
            logger.Info($"Modifying attributes for file {ctx.Path}");
        }
        /// <inheritdoc />
        public virtual void OnModifiedAttributes(ISshPath ctx)
        {
            logger.Info($"The file {ctx.Path} attributes has been modified");
        }

        /// <inheritdoc />
        public virtual void OnClosing(ISshEvent ctx)
        {
            logger.Info($"Closing the file {ctx.SshHandle.PhysicalPath}");
        }

        /// <inheritdoc />
        public virtual void OnClosed(ISshEvent ctx)
        {
            logger.Info($"The file {ctx.SshHandle.PhysicalPath} has been closed");
        }

        /// <inheritdoc />
        public virtual void OnOpening(ISshEvent ctx)
        {
            logger.Info($"Opening file{ctx.SshHandle.PhysicalPath}");
        }

        /// <inheritdoc />
        public virtual void OnOpen(ISshEvent ctx)
        {
            logger.Info($"The file {ctx.SshHandle.PhysicalPath} has been opened");
        }


        /// <inheritdoc />
        public virtual void OnReading(ISshReadWrite ctx)
        {
            logger.Info($"Reading the file {ctx.SshHandle.PhysicalPath}");
        }

        /// <inheritdoc />
        public virtual void OnRead(ISshReadWrite ctx)
        {
            logger.Info($"The file {ctx.SshHandle.PhysicalPath} is read");
        }

        /// <inheritdoc />
        public virtual void OnOpenFailed(ISshIOFailure ctx)
        {
            logger.Info($"Opening the file {ctx.LocalPath} is failed");
        }

        /// <inheritdoc />
        public virtual void OnWriting(ISshReadWrite ctx)
        {
            logger.Info($"Writing on file {ctx.SshHandle.PhysicalPath}");
        }

        /// <inheritdoc />
        public virtual void OnWrite(ISshReadWrite ctx)
        {
            logger.Info($"The file {ctx.SshHandle.PhysicalPath} has been written");
        }

        /// <inheritdoc />
        public virtual void OnCreating(ISshPath ctx)
        {
            logger.Info($"Creating path {ctx.Path}");
        }
        /// <inheritdoc />
        public virtual void OnCreated(ISshPath ctx)
        {
            logger.Info($"The path created on {ctx.Path}");
        }

        /// <inheritdoc />
        public virtual void OnMoving(ISshMove ctx)
        {
            logger.Info($"Moving file {ctx.SourcePath} to {ctx.DestPath}");
        }
        /// <inheritdoc />
        public virtual void OnMoved(ISshMove ctx)
        {
            logger.Info($"The file {ctx.SourcePath} has been moved to {ctx.DestPath}");
        }

        /// <inheritdoc />
        public virtual void OnRemoving(ISshPath ctx)
        {
            logger.Info($"Removing file {ctx.Path}");
        }
        /// <inheritdoc />
        public virtual void OnRemoved(ISshPath ctx)
        {
            logger.Info($"The file {ctx.Path} has been removed");
        }

        /// <inheritdoc />
        public virtual void OnLinking(ISshSysLink ctx)
        {
            logger.Info($"Linking {ctx.SourcePath} to {ctx.DestPath}");
        }

        /// <inheritdoc />
        public virtual void OnLink(ISshSysLink ctx)
        {
            logger.Info($"The file / path {ctx.SourcePath} has been linked to {ctx.DestPath}");
        }

        /// <inheritdoc />
        public virtual void OnInitialized(ISshSession sshSession, int version)
        {
            logger.Info($"Session from IP {sshSession.RemoteAddress} has been initialized.");
        }

        /// <inheritdoc />
        public virtual void OnDestroying(ISshSession sshSession)
        {

            logger.Info($"Session from IP {sshSession.RemoteAddress} is being destroyed.");

        }

        /// <inheritdoc />
        public virtual void OnReadingEntries(ISshEntries sshEntries)
        {
           
            
            logger.Info($"Session from IP {sshEntries.SshSession.RemoteAddress} is reading entries: {sshEntries.RemoteHandle}");
        }

        /// <inheritdoc />
        public virtual void OnReadEntries(ISshEntries sshEntries)
        {

            logger.Info($"Session from IP {sshEntries.SshSession.RemoteAddress} has read entries: {sshEntries.RemoteHandle} with entries: {string.Join(", ", sshEntries.Entries?.Keys!)}");

        }

        /// <inheritdoc />
        public virtual void OnExiting(ISshSession sshSession, ISshHandle sshHandle)
        {

            logger.Info($"Session from IP {sshSession.RemoteAddress} is exiting with handle: {sshHandle.PhysicalPath}");

        }

        /// <inheritdoc />
        public virtual void OnReceivedExtension(ISshReceived sshReceived)
        {

            logger.Info($"Session from IP {sshReceived.SshSession.RemoteAddress} received extension: {sshReceived.Extension} with ID: {sshReceived.Id}");

        }

        /// <inheritdoc />
        public virtual void OnReceived(ISshReceived sshReceived)
        {
            logger.Info($"Session from IP {sshReceived.SshSession.RemoteAddress} received message of type: {sshReceived.Type} with ID: {sshReceived.Id}");

        }
    }

}

