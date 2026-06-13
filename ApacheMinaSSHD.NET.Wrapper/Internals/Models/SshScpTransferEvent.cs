// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal sealed class SshScpTransferEvent : ISshScpTransferEvent
    {
        public required ISshSession Session { get; init; }

        public required string Operation { get; init; }

        public required string Path { get; init; }

        public long Length { get; init; }

        public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();

        public int? AckStatusCode { get; init; }

        public string? AckLine { get; init; }

        public string? Command { get; init; }

        public Exception? Exception { get; init; }
    }
}
