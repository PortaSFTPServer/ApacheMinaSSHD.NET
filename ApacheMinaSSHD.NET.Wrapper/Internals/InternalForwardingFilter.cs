// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.common.session;
using org.apache.sshd.common.util.net;
using org.apache.sshd.server.forward;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalForwardingFilter : java.lang.Object, ForwardingFilter
    {
        private readonly IAMNetForwardingFilter? _composite;
        private readonly IAMNetTcpForwardingFilter? _tcp;
        private readonly IAMNetAgentForwardingFilter? _agent;
        private readonly IAMNetX11ForwardingFilter? _x11;

        public InternalForwardingFilter(IAMNetForwardingFilter composite)
        {
            _composite = composite;
        }

        public InternalForwardingFilter(
            IAMNetTcpForwardingFilter? tcp,
            IAMNetAgentForwardingFilter? agent,
            IAMNetX11ForwardingFilter? x11)
        {
            _tcp = tcp;
            _agent = agent;
            _x11 = x11;
        }

        public bool canConnect(TcpForwardingFilter.Type type, SshdSocketAddress address, Session session)
        {
            var netType = MapType(type);
            var host = address?.getHostName() ?? "";
            var port = address?.getPort() ?? 0;
            var sshSession = CreateSession(session);
            return _composite?.CanConnect(netType, host, port, sshSession)
                ?? _tcp?.CanConnect(netType, host, port, sshSession)
                ?? true;
        }

        public bool canForwardAgent(Session session, string requestType)
        {
            var sshSession = CreateSession(session);
            return _composite?.CanForwardAgent(sshSession, requestType)
                ?? _agent?.CanForwardAgent(sshSession, requestType)
                ?? false;
        }

        public bool canForwardX11(Session session, string requestType)
        {
            var sshSession = CreateSession(session);
            return _composite?.CanForwardX11(sshSession, requestType)
                ?? _x11?.CanForwardX11(sshSession, requestType)
                ?? false;
        }

        public bool canListen(SshdSocketAddress address, Session session)
        {
            var host = address?.getHostName() ?? "";
            var port = address?.getPort() ?? 0;
            var sshSession = CreateSession(session);
            return _composite?.CanListen(host, port, sshSession)
                ?? _tcp?.CanListen(host, port, sshSession)
                ?? true;
        }

        private static ISshSession CreateSession(Session session)
        {
            if (session is ServerSession serverSession)
                return new SshSession(serverSession);
            return new SshSession();
        }

        private static AMNetForwardingType MapType(TcpForwardingFilter.Type? type)
        {
            if (type == null) return AMNetForwardingType.Direct;
            if (type == TcpForwardingFilter.Type.Direct) return AMNetForwardingType.Direct;
            if (type == TcpForwardingFilter.Type.Forwarded) return AMNetForwardingType.Forwarded;
            return AMNetForwardingType.Direct;
        }
    }
}
