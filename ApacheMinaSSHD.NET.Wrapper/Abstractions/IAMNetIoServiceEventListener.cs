using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Receives low-level connection events before or around SSH session creation.
    /// </summary>
    public interface IAMNetIoServiceEventListener
    {
        /// <summary>
        /// Called when a new client connects. Return <c>false</c> to block or disconnect the connection immediately.
        /// </summary>
        /// <param name="context">Connection metadata for the accepted connection.</param>
        /// <returns><c>true</c> to allow the connection; otherwise <c>false</c>.</returns>
        bool OnConnectionAccepted(ISshServiceConnection context);

        /// <summary>
        /// Called if an accepted connection is closed before it's fully established.
        /// </summary>
        /// <param name="context">Connection metadata and the failure exception.</param>
        void OnConnectionAborted(ISshServiceConnection context);

        /// <summary>
        /// Only relevant if your server makes outbound connections (Forwarding).
        /// </summary>
        /// <param name="context">Connection metadata for the outbound connection.</param>
        void OnOutboundConnectionEstablished(ISshServiceConnection context);

        /// <summary>
        /// Only relevant if an outbound connection attempt fails.
        /// </summary>
        /// <param name="context">Connection metadata and the failure exception.</param>
        void OnOutboundConnectionAborted(ISshServiceConnection context);
    }
}
