// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshIOFailure : ISshIOFailure
    {
        public ISshSession Session { get; set; } = null!;
        public string RemoteHandle { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
        public Exception Exception { get; set; } = null!;
    }
}
