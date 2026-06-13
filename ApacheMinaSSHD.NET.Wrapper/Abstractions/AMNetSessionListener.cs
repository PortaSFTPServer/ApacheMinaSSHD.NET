// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default session listener that logs SSH session lifecycle events.
    /// </summary>
    public class AMNetSessionListener : IAMNetSessionListener
    {
        private readonly IAMNetLogger logger;

        /// <summary>
        /// Creates a session listener using the default logger.
        /// </summary>
        public AMNetSessionListener()
            : this(new AMNetLogger(typeof(AMNetSessionListener), AMNetLogger.LogLevel.Info))
        {
        }

        /// <summary>
        /// Creates a session listener using the supplied logger.
        /// </summary>
        /// <param name="logger">The logger used by the default event handlers.</param>
        public AMNetSessionListener(IAMNetLogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public virtual void OnSessionCreated(ISshSession session)
        {
            logger.Info($"Session from IP {session.RemoteAddress} has been created.");
        }

        /// <inheritdoc />
        public virtual void OnSessionEstablished(ISshSession session)
        {
            logger.Info($"Session from IP {session.RemoteAddress} has been established.");
        }

        /// <inheritdoc />
        public virtual void OnSessionClosed(ISshSession session)
        {
            logger.Info($"Session from IP {session.RemoteAddress} has been closed.");
        }

        /// <inheritdoc />
        public virtual void OnSessionDisconnect(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} disconnected: {context.Message}");
        }

        /// <inheritdoc />
        public virtual void OnSessionEvent(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} event: {context.EventName}");
        }

        /// <inheritdoc />
        public virtual void OnSessionException(ISshSessionEvent context)
        {
            logger.Error($"Session from IP {context.Session.RemoteAddress} exception.", context.Exception);
        }

        /// <inheritdoc />
        public virtual void OnSessionNegotiationStart(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} negotiation started.");
        }

        /// <inheritdoc />
        public virtual void OnSessionNegotiationEnd(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} negotiation ended.");
        }

        /// <inheritdoc />
        public virtual void OnSessionNegotiationOptionsCreated(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} negotiation options created.");
        }

        /// <inheritdoc />
        public virtual void OnSessionPeerIdentificationLine(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} peer identification line: {context.Version}");
        }

        /// <inheritdoc />
        public virtual void OnSessionPeerIdentificationReceived(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} peer identification received: {context.Version}");
        }

        /// <inheritdoc />
        public virtual void OnSessionPeerIdentificationSend(ISshSessionEvent context)
        {
            logger.Info($"Session from IP {context.Session.RemoteAddress} peer identification sent: {context.Version}");
        }
    }
}
