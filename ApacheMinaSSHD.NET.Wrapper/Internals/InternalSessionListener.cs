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
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using java.util;
using org.apache.sshd.common.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalSessionListener : java.lang.Object, SessionListener
    {
        private readonly IAMNetSessionListener sessionListener;
        static readonly IAMNetLogger logger = new AMNetLogger(typeof(InternalSessionListener), AMNetLogger.LogLevel.Info);

        public InternalSessionListener(IAMNetSessionListener sessionListener)
        {
            this.sessionListener = sessionListener;
        }

        private static string SessionInfo(Session session)
        {
            var user = session.getUsername() ?? "?";
            string addr;
            try { addr = session.getIoSession()?.getRemoteAddress()?.toString() ?? "?"; } catch { addr = "?"; }
            return $"{user}@{addr}";
        }

        private static string SessionInfoShort(Session session)
        {
            var user = session.getUsername() ?? "?";
            return user;
        }

        public void sessionClosed(Session session)
        {
            var sshSession = ToSshSession(session);
            logger.Debug($"[{SessionInfo(session)}] Session closed");
            sessionListener.OnSessionClosed(sshSession);
        }

        public void sessionCreated(Session session)
        {
            var sshSession = ToSshSession(session);
            logger.Info($"[{SessionInfo(session)}] Session created");
            sessionListener.OnSessionCreated(sshSession);
        }

        public void sessionDisconnect(Session session, int reason, string msg, string language, bool initiator)
        {
            var sshSession = ToSshSession(session);
            logger.Info($"[{SessionInfo(session)}] Disconnected: {msg} (reason={reason})");
            sessionListener.OnSessionDisconnect(CreateContext(session) with
            {
                Reason = reason,
                Message = msg,
                Language = language,
                Initiator = initiator
            });
        }

        public void sessionEstablished(Session session)
        {
            var sshSession = ToSshSession(session);
            logger.Info($"[{SessionInfo(session)}] Session established");
            sessionListener.OnSessionEstablished(sshSession);
        }

        public void sessionEvent(Session session, SessionListener.Event @event)
        {
            logger.Debug($"[{SessionInfo(session)}] Event: {@event.name()}");
            sessionListener.OnSessionEvent(CreateContext(session) with { EventName = @event.name() });
        }

        public void sessionException(Session session, Exception t)
        {
            logger.Error($"[{SessionInfo(session)}] Exception: {t?.Message}", t);
            sessionListener.OnSessionException(CreateContext(session) with { Exception = t });
        }

        public void sessionNegotiationEnd(
            Session session,
            Map clientProposal,
            Map serverProposal,
            Map negotiatedOptions,
            Exception reason)
        {
            logger.Debug($"[{SessionInfoShort(session)}] Negotiation ended");
            var sshSession = ToSshSession(session);
            if (negotiatedOptions != null)
            {
                var iterator = negotiatedOptions.entrySet().iterator();
                while (iterator.hasNext())
                {
                    var entry = (Map.Entry)iterator.next();
                    var key = entry.getKey()?.ToString();
                    if (key != null && key.Equals("C2SENC", StringComparison.OrdinalIgnoreCase))
                    {
                        sshSession.SetNegotiatedCipher(entry.getValue()?.ToString());
                        break;
                    }
                }
            }
            sessionListener.OnSessionNegotiationEnd(CreateContext(session) with
            {
                ClientProposal = ToStringDictionary(clientProposal),
                ServerProposal = ToStringDictionary(serverProposal),
                NegotiatedOptions = ToStringDictionary(negotiatedOptions),
                Exception = reason
            });
        }

        public void sessionNegotiationOptionsCreated(Session session, Map proposal)
        {
            logger.Debug($"[{SessionInfoShort(session)}] Negotiation options created");
            sessionListener.OnSessionNegotiationOptionsCreated(CreateContext(session) with
            {
                Proposal = ToStringDictionary(proposal)
            });
        }

        public void sessionNegotiationStart(Session session, Map clientProposal, Map serverProposal)
        {
            logger.Debug($"[{SessionInfoShort(session)}] Negotiation started");
            sessionListener.OnSessionNegotiationStart(CreateContext(session) with
            {
                ClientProposal = ToStringDictionary(clientProposal),
                ServerProposal = ToStringDictionary(serverProposal)
            });
        }

        public void sessionPeerIdentificationLine(Session session, string line, List extraLines)
        {
            logger.Debug($"[{SessionInfoShort(session)}] Peer id line: {line}");
            sessionListener.OnSessionPeerIdentificationLine(CreateContext(session) with
            {
                Version = line,
                ExtraLines = ToStringList(extraLines)
            });
        }

        public void sessionPeerIdentificationReceived(Session session, string version, List extraLines)
        {
            logger.Debug($"[{SessionInfoShort(session)}] Peer id received: {version}");
            sessionListener.OnSessionPeerIdentificationReceived(CreateContext(session) with
            {
                Version = version,
                ExtraLines = ToStringList(extraLines)
            });
        }

        public void sessionPeerIdentificationSend(Session session, string version, List extraLines)
        {
            logger.Debug($"[{SessionInfoShort(session)}] Peer id sent: {version}");
            sessionListener.OnSessionPeerIdentificationSend(CreateContext(session) with
            {
                Version = version,
                ExtraLines = ToStringList(extraLines)
            });
        }

        private static SshSession ToSshSession(Session session)
        {
            return new SshSession((org.apache.sshd.server.session.ServerSession)session);
        }

        private static SshSessionEvent CreateContext(Session session)
        {
            return new SshSessionEvent
            {
                Session = ToSshSession(session)
            };
        }

        private static IReadOnlyDictionary<string, string> ToStringDictionary(Map? map)
        {
            var result = new Dictionary<string, string>();
            if (map == null)
            {
                return result;
            }

            var iterator = map.entrySet().iterator();
            while (iterator.hasNext())
            {
                var entry = (Map.Entry)iterator.next();
                result[entry.getKey()?.ToString() ?? string.Empty] = entry.getValue()?.ToString() ?? string.Empty;
            }

            return result;
        }

        private static IReadOnlyList<string> ToStringList(List? values)
        {
            if (values == null)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            var iterator = values.iterator();
            while (iterator.hasNext())
            {
                result.Add(iterator.next()?.ToString() ?? string.Empty);
            }

            return result;
        }
    }
}
