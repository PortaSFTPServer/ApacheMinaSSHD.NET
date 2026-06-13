using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetPortForwardingEventListener
    {
        void OnEstablishingTunnel(string host, int port, bool isLocalForwarding, ISshSession session) { }
        void OnEstablishedTunnel(string host, int port, bool isLocalForwarding, string boundAddress, ISshSession session) { }
        void OnTearingDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session) { }
        void OnTornDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session) { }
    }
}
