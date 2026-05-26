using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Receives SSH session lifecycle, negotiation, and error events.
    /// </summary>
    public interface IAMNetSessionListener
    {
        /// <summary>Called when a session object is created.</summary>
        /// <param name="session">The session metadata.</param>
        void OnSessionCreated(ISshSession session) { }

        /// <summary>Called when the SSH session is established.</summary>
        /// <param name="session">The session metadata.</param>
        void OnSessionEstablished(ISshSession session) { }

        /// <summary>Called when the SSH session is closed.</summary>
        /// <param name="session">The session metadata.</param>
        void OnSessionClosed(ISshSession session) { }

        /// <summary>Called when a session disconnect message is observed.</summary>
        /// <param name="context">The disconnect event metadata.</param>
        void OnSessionDisconnect(ISshSessionEvent context) { }

        /// <summary>Called for a generic session event.</summary>
        /// <param name="context">The session event metadata.</param>
        void OnSessionEvent(ISshSessionEvent context) { }

        /// <summary>Called when an exception is raised for the session.</summary>
        /// <param name="context">The session event metadata and exception.</param>
        void OnSessionException(ISshSessionEvent context) { }

        /// <summary>Called when key exchange or algorithm negotiation starts.</summary>
        /// <param name="context">The negotiation event metadata.</param>
        void OnSessionNegotiationStart(ISshSessionEvent context) { }

        /// <summary>Called when key exchange or algorithm negotiation ends.</summary>
        /// <param name="context">The negotiation event metadata.</param>
        void OnSessionNegotiationEnd(ISshSessionEvent context) { }

        /// <summary>Called after negotiation options are created.</summary>
        /// <param name="context">The negotiation options metadata.</param>
        void OnSessionNegotiationOptionsCreated(ISshSessionEvent context) { }

        /// <summary>Called when a peer identification line is read.</summary>
        /// <param name="context">The peer identification metadata.</param>
        void OnSessionPeerIdentificationLine(ISshSessionEvent context) { }

        /// <summary>Called when the peer identification string is received.</summary>
        /// <param name="context">The peer identification metadata.</param>
        void OnSessionPeerIdentificationReceived(ISshSessionEvent context) { }

        /// <summary>Called when the server sends its peer identification string.</summary>
        /// <param name="context">The peer identification metadata.</param>
        void OnSessionPeerIdentificationSend(ISshSessionEvent context) { }
    }
}
