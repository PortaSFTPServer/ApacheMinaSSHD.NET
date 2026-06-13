// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for an SCP transfer event.
    /// </summary>
    public interface ISshScpTransferEvent
    {
        /// <summary>Gets the session associated with the transfer.</summary>
        ISshSession Session { get; }

        /// <summary>Gets the transfer operation name.</summary>
        string Operation { get; }

        /// <summary>Gets the local or remote path associated with the transfer.</summary>
        string Path { get; }

        /// <summary>Gets the file length when available.</summary>
        long Length { get; }

        /// <summary>Gets permission names associated with the transfer.</summary>
        IReadOnlyList<string> Permissions { get; }

        /// <summary>Gets the SCP acknowledgement status code when available.</summary>
        int? AckStatusCode { get; }

        /// <summary>Gets the SCP acknowledgement line when available.</summary>
        string? AckLine { get; }

        /// <summary>Gets the SCP command text when available.</summary>
        string? Command { get; }

        /// <summary>Gets the exception associated with the transfer when available.</summary>
        Exception? Exception { get; }
    }
}
