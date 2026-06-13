using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetForwardingFilter : IAMNetForwardingFilter
    {
        private readonly IAMNetTcpForwardingFilter? _tcp;
        private readonly IAMNetAgentForwardingFilter? _agent;
        private readonly IAMNetX11ForwardingFilter? _x11;

        public AMNetForwardingFilter(
            IAMNetTcpForwardingFilter? tcp = null,
            IAMNetAgentForwardingFilter? agent = null,
            IAMNetX11ForwardingFilter? x11 = null)
        {
            _tcp = tcp;
            _agent = agent;
            _x11 = x11;
        }

        public bool CanListen(string host, int port, ISshSession session)
            => _tcp?.CanListen(host, port, session) ?? true;

        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
            => _tcp?.CanConnect(type, host, port, session) ?? true;

        public bool CanForwardAgent(ISshSession session, string requestType)
            => _agent?.CanForwardAgent(session, requestType) ?? false;

        public bool CanForwardX11(ISshSession session, string requestType)
            => _x11?.CanForwardX11(session, requestType) ?? false;

        public static AMNetForwardingFilter AcceptAll => new(
            AMNetTcpForwardingFilter.AcceptAll, null, null
        );

        public static AMNetForwardingFilter RejectAll => new(
            AMNetTcpForwardingFilter.RejectAll, null, null
        );

        public static AMNetForwardingFilter FromPolicy(AMNetTcpForwardingPolicy policy)
            => new(new AMNetTcpForwardingFilter(policy), null, null);
    }
}
