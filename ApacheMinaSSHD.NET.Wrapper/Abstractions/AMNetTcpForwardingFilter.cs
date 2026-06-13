// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetTcpForwardingFilter : IAMNetTcpForwardingFilter
    {
        private readonly AMNetTcpForwardingPolicy _policy;

        public AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy policy)
        {
            _policy = policy;
        }

        public virtual bool CanListen(string host, int port, ISshSession session)
        {
            return _policy != AMNetTcpForwardingPolicy.None;
        }

        public virtual bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
        {
            return type switch
            {
                AMNetForwardingType.Direct => _policy is AMNetTcpForwardingPolicy.All or AMNetTcpForwardingPolicy.Local,
                AMNetForwardingType.Forwarded => _policy is AMNetTcpForwardingPolicy.All or AMNetTcpForwardingPolicy.Remote,
                _ => false
            };
        }

        public static AMNetTcpForwardingFilter AcceptAll => new(AMNetTcpForwardingPolicy.All);
        public static AMNetTcpForwardingFilter RejectAll => new(AMNetTcpForwardingPolicy.None);
    }
}
