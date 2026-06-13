using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetX11ForwardingFilter
    {
        bool CanForwardX11(ISshSession session, string requestType);
    }
}
