// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;
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
        static readonly IAMNetLogger logger = new AMNetLogger(typeof(InternalForwardingFilter), AMNetLogger.LogLevel.Info);

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

        private static string User(Session session)
        {
            var user = session.getUsername();
            return !string.IsNullOrEmpty(user) ? user : "?";
        }

        public bool canConnect(TcpForwardingFilter.Type type, SshdSocketAddress address, Session session)
        {
            var netType = MapType(type);
            var host = address?.getHostName() ?? "";
            var port = address?.getPort() ?? 0;
            var sshSession = CreateSession(session);
            var result = _composite?.CanConnect(netType, host, port, sshSession)
                ?? _tcp?.CanConnect(netType, host, port, sshSession)
                ?? true;
            logger.Debug($"[{User(session)}] Forward connect {host}:{port} ({netType}) -> {result}");
            return result;
        }

        public bool canForwardAgent(Session session, string requestType)
        {
            var sshSession = CreateSession(session);
            var result = _composite?.CanForwardAgent(sshSession, requestType)
                ?? _agent?.CanForwardAgent(sshSession, requestType)
                ?? false;
            logger.Debug($"[{User(session)}] Forward agent ({requestType}) -> {result}");
            return result;
        }

        public bool canForwardX11(Session session, string requestType)
        {
            var sshSession = CreateSession(session);
            var result = _composite?.CanForwardX11(sshSession, requestType)
                ?? _x11?.CanForwardX11(sshSession, requestType)
                ?? false;
            logger.Debug($"[{User(session)}] Forward X11 ({requestType}) -> {result}");
            return result;
        }

        public bool canListen(SshdSocketAddress address, Session session)
        {
            var host = address?.getHostName() ?? "";
            var port = address?.getPort() ?? 0;
            var sshSession = CreateSession(session);
            var result = _composite?.CanListen(host, port, sshSession)
                ?? _tcp?.CanListen(host, port, sshSession)
                ?? true;
            logger.Debug($"[{User(session)}] Forward listen {host}:{port} -> {result}");
            return result;
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
