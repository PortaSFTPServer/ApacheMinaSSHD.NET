using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.util;
using org.apache.sshd.common.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalSessionListener : java.lang.Object, SessionListener
    {
        private readonly IAMNetSessionListener sessionListener;

        public InternalSessionListener(IAMNetSessionListener sessionListener)
        {
            this.sessionListener = sessionListener;
        }

        public void sessionClosed(Session session)
        {
            sessionListener.OnSessionClosed(ToSshSession(session));
        }

        public void sessionCreated(Session session)
        {
            sessionListener.OnSessionCreated(ToSshSession(session));
        }

        public void sessionDisconnect(Session session, int reason, string msg, string language, bool initiator)
        {
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
            sessionListener.OnSessionEstablished(ToSshSession(session));
        }

        public void sessionEvent(Session session, SessionListener.Event @event)
        {
            sessionListener.OnSessionEvent(CreateContext(session) with { EventName = @event.name() });
        }

        public void sessionException(Session session, Exception t)
        {
            sessionListener.OnSessionException(CreateContext(session) with { Exception = t });
        }

        public void sessionNegotiationEnd(
            Session session,
            Map clientProposal,
            Map serverProposal,
            Map negotiatedOptions,
            Exception reason)
        {
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
            sessionListener.OnSessionNegotiationOptionsCreated(CreateContext(session) with
            {
                Proposal = ToStringDictionary(proposal)
            });
        }

        public void sessionNegotiationStart(Session session, Map clientProposal, Map serverProposal)
        {
            sessionListener.OnSessionNegotiationStart(CreateContext(session) with
            {
                ClientProposal = ToStringDictionary(clientProposal),
                ServerProposal = ToStringDictionary(serverProposal)
            });
        }

        public void sessionPeerIdentificationLine(Session session, string line, List extraLines)
        {
            sessionListener.OnSessionPeerIdentificationLine(CreateContext(session) with
            {
                Version = line,
                ExtraLines = ToStringList(extraLines)
            });
        }

        public void sessionPeerIdentificationReceived(Session session, string version, List extraLines)
        {
            sessionListener.OnSessionPeerIdentificationReceived(CreateContext(session) with
            {
                Version = version,
                ExtraLines = ToStringList(extraLines)
            });
        }

        public void sessionPeerIdentificationSend(Session session, string version, List extraLines)
        {
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
