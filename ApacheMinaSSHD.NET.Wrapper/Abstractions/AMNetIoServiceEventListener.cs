using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default low-level connection listener that logs connection events.
    /// </summary>
    public class AMNetIoServiceEventListener : IAMNetIoServiceEventListener
    {
        IAMNetLogger logger = new AMNetLogger(typeof(AMNetIoServiceEventListener), AMNetLogger.LogLevel.info);

        /// <summary>
        /// Creates a default low-level connection listener.
        /// </summary>
        public AMNetIoServiceEventListener()
        {
        }

        /// <inheritdoc />
        public virtual void OnConnectionAborted(ISshServiceConnection context)
        {
            logger.Error(context.Exception?.Message!);
        }

        /// <inheritdoc />
        public virtual bool OnConnectionAccepted(ISshServiceConnection context)
        {
            // The developer sees clean properties via the interface
            logger.Info($"Evaluating connection from {context.RemoteEndPoint.Address}...");

            if (context.IoService.IsAcceptor)
            {
                // Access properties of the nested interface
                var currentLoad = context.IoService.BoundAddresses.Count();
            }

            return true;
        }

        /// <inheritdoc />
        public virtual void OnOutboundConnectionAborted(ISshServiceConnection context)
        {
            logger.Error(context.Exception?.Message!);

        }

        /// <inheritdoc />
        public virtual void OnOutboundConnectionEstablished(ISshServiceConnection context)
        {
            logger.Error(context.Exception?.Message!);
        }
    }
}
