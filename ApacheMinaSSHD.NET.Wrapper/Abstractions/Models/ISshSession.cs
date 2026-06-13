// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides safe session metadata for authentication, event, and file operation callbacks.
    /// </summary>
    public interface ISshSession
    {
        /// <summary>Gets the remote client address.</summary>
        string RemoteAddress { get; }
        /// <summary>Gets the unique session identifier assigned by the wrapper.</summary>
        Guid SessionId { get; }
    }
}
