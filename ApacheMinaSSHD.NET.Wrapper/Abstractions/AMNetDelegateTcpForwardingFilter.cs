// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public sealed class AMNetDelegateTcpForwardingFilter : IAMNetTcpForwardingFilter
    {
        private readonly Func<string, int, ISshSession, bool> _canListen;
        private readonly Func<AMNetForwardingType, string, int, ISshSession, bool> _canConnect;

        public AMNetDelegateTcpForwardingFilter(
            Func<string, int, ISshSession, bool> canListen,
            Func<AMNetForwardingType, string, int, ISshSession, bool> canConnect)
        {
            _canListen = canListen ?? throw new ArgumentNullException(nameof(canListen));
            _canConnect = canConnect ?? throw new ArgumentNullException(nameof(canConnect));
        }

        public bool CanListen(string host, int port, ISshSession session)
            => _canListen(host, port, session);

        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
            => _canConnect(type, host, port, session);
    }
}
