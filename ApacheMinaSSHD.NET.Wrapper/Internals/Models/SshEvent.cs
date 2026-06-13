// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshEvent : ISshEvent
    {
        public ISshSession Session { get; set; } = null!;

        public string RemoteHandle { get; set; } = string.Empty;

        public ISshHandle SshHandle { get; set; } = null!;

        public Exception Exception { get; set; } = null!;
    }
}
