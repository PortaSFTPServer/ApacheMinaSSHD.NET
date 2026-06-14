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
