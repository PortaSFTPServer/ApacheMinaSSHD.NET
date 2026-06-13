// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SSH session lifecycle and negotiation events.
    /// </summary>
    public interface ISshSessionEvent
    {
        /// <summary>Gets the session associated with the event.</summary>
        ISshSession Session { get; }

        /// <summary>Gets the event name when available.</summary>
        string? EventName { get; }

        /// <summary>Gets the disconnect or event reason code when available.</summary>
        int? Reason { get; }

        /// <summary>Gets the event message when available.</summary>
        string? Message { get; }

        /// <summary>Gets the language tag associated with the event message when available.</summary>
        string? Language { get; }

        /// <summary>Gets whether the local side initiated the event when available.</summary>
        bool? Initiator { get; }

        /// <summary>Gets the peer identification version string when available.</summary>
        string? Version { get; }

        /// <summary>Gets extra peer identification lines when available.</summary>
        IReadOnlyList<string> ExtraLines { get; }

        /// <summary>Gets the client algorithm proposal when available.</summary>
        IReadOnlyDictionary<string, string> ClientProposal { get; }

        /// <summary>Gets the server algorithm proposal when available.</summary>
        IReadOnlyDictionary<string, string> ServerProposal { get; }

        /// <summary>Gets negotiated algorithm options when available.</summary>
        IReadOnlyDictionary<string, string> NegotiatedOptions { get; }

        /// <summary>Gets a generic proposal map when available.</summary>
        IReadOnlyDictionary<string, string> Proposal { get; }

        /// <summary>Gets the exception associated with the event when available.</summary>
        Exception? Exception { get; }
    }
}
