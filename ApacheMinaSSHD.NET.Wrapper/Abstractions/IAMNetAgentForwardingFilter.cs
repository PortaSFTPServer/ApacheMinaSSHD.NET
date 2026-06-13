using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetAgentForwardingFilter
    {
        bool CanForwardAgent(ISshSession session, string requestType);
    }
}
