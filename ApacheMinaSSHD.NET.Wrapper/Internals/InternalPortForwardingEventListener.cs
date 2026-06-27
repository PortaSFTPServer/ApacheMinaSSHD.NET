using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.common.forward;
using org.apache.sshd.common.session;
using org.apache.sshd.common.util.net;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalPortForwardingEventListener : java.lang.Object, PortForwardingEventListener
    {
        private readonly IAMNetPortForwardingEventListener listener;
        private readonly Internals.Models.SshSession? fixedSession;

        public IAMNetPortForwardingEventListener WrappedListener => listener;

        public InternalPortForwardingEventListener(IAMNetPortForwardingEventListener listener, org.apache.sshd.server.session.ServerSession? javaSession = null)
        {
            this.listener = listener ?? throw new System.ArgumentNullException(nameof(listener));
            if (javaSession != null)
                fixedSession = new Internals.Models.SshSession(javaSession);
        }

        // --- Explicit tunnels (local forwarding) ---

        public void establishingExplicitTunnel(Session session, SshdSocketAddress localAddress, SshdSocketAddress remoteAddress, bool localForwarding)
        {
            listener.OnEstablishingTunnel(
                remoteAddress?.getHostName() ?? "?",
                remoteAddress?.getPort() ?? 0,
                localForwarding,
                ToSshSession(session));
        }

        public void establishedExplicitTunnel(Session session, SshdSocketAddress localAddress, SshdSocketAddress remoteAddress, bool localForwarding, SshdSocketAddress boundAddress, Exception reason)
        {
            listener.OnEstablishedTunnel(
                remoteAddress?.getHostName() ?? "?",
                remoteAddress?.getPort() ?? 0,
                localForwarding,
                boundAddress?.toString() ?? localAddress?.toString() ?? "?",
                ToSshSession(session));
        }

        public void tearingDownExplicitTunnel(Session session, SshdSocketAddress localAddress, bool localForwarding, SshdSocketAddress remoteAddress)
        {
            listener.OnTearingDownTunnel(
                remoteAddress?.getHostName() ?? "?",
                remoteAddress?.getPort() ?? 0,
                localForwarding,
                ToSshSession(session));
        }

        public void tornDownExplicitTunnel(Session session, SshdSocketAddress localAddress, bool localForwarding, SshdSocketAddress remoteAddress, Exception reason)
        {
            listener.OnTornDownTunnel(
                remoteAddress?.getHostName() ?? "?",
                remoteAddress?.getPort() ?? 0,
                localForwarding,
                ToSshSession(session));
        }

        // --- Dynamic tunnels (SOCKS) ---

        public void establishingDynamicTunnel(Session session, SshdSocketAddress localAddress)
        {
            listener.OnEstablishingTunnel("0.0.0.0", 0, false, ToSshSession(session));
        }

        public void establishedDynamicTunnel(Session session, SshdSocketAddress localAddress, SshdSocketAddress boundAddress, Exception reason)
        {
            listener.OnEstablishedTunnel(
                "0.0.0.0",
                0,
                false,
                boundAddress?.toString() ?? localAddress?.toString() ?? "?",
                ToSshSession(session));
        }

        public void tearingDownDynamicTunnel(Session session, SshdSocketAddress localAddress)
        {
            listener.OnTearingDownTunnel("0.0.0.0", 0, false, ToSshSession(session));
        }

        public void tornDownDynamicTunnel(Session session, SshdSocketAddress localAddress, Exception reason)
        {
            listener.OnTornDownTunnel("0.0.0.0", 0, false, ToSshSession(session));
        }

        private ISshSession ToSshSession(Session session)
        {
            if (session != null)
                return new Internals.Models.SshSession((org.apache.sshd.server.session.ServerSession)session);
            return fixedSession ?? new Internals.Models.SshSession();
        }
    }
}