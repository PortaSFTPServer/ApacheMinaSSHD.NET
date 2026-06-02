using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default SCP transfer listener that logs transfer lifecycle events.
    /// </summary>
    public class AMNetScpTransferEventListener : IAMNetScpTransferEventListener
    {
        private readonly IAMNetLogger logger;

        /// <summary>
        /// Creates an SCP transfer listener using the default logger.
        /// </summary>
        public AMNetScpTransferEventListener()
            : this(new AMNetLogger(typeof(AMNetScpTransferEventListener), AMNetLogger.LogLevel.Info))
        {
        }

        /// <summary>
        /// Creates an SCP transfer listener using the supplied logger.
        /// </summary>
        /// <param name="logger">The logger used by the default event handlers.</param>
        public AMNetScpTransferEventListener(IAMNetLogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public virtual void OnStartFile(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} started for file {context.Path}.");
        }

        /// <inheritdoc />
        public virtual void OnEndFile(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} ended for file {context.Path}.");
        }

        /// <inheritdoc />
        public virtual void OnFileAck(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} acknowledgement for file {context.Path}: {context.AckStatusCode}");
        }

        /// <inheritdoc />
        public virtual void OnReceiveCommandAck(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP receive command acknowledgement for {context.Command}: {context.AckStatusCode}");
        }

        /// <inheritdoc />
        public virtual void OnStartFolder(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} started for folder {context.Path}.");
        }

        /// <inheritdoc />
        public virtual void OnEndFolder(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} ended for folder {context.Path}.");
        }
    }
}
