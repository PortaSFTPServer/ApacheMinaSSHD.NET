using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetPortForwardingEventListener : IAMNetPortForwardingEventListener
    {
        private readonly IAMNetLogger _logger;

        public AMNetPortForwardingEventListener(IAMNetLogger? logger = null)
        {
            _logger = logger ?? new AMNetLogger(typeof(AMNetPortForwardingEventListener), AMNetLogger.LogLevel.Info);
        }

        public virtual void OnEstablishingTunnel(string host, int port, bool isLocalForwarding, ISshSession session)
        {
            _logger.Info($"Establishing {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port}");
        }

        public virtual void OnEstablishedTunnel(string host, int port, bool isLocalForwarding, string boundAddress, ISshSession session)
        {
            _logger.Info($"Established {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port} bound at {boundAddress}");
        }

        public virtual void OnTearingDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session)
        {
            _logger.Info($"Tearing down {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port}");
        }

        public virtual void OnTornDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session)
        {
            _logger.Info($"Torn down {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port}");
        }
    }
}
