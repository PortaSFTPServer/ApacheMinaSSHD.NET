using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetTcpForwardingFilter
    {
        bool CanListen(string host, int port, ISshSession session);
        bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session);
    }
}
