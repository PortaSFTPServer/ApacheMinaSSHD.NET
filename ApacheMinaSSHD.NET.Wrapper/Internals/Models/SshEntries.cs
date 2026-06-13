// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshEntries : ISshEntries
    {
        public ISshSession SshSession { get; set; } = null!;

        public string RemoteHandle { get; set; } = string.Empty;

        public ISshDirectoryHandle localHandle { get; set; } = null!;

        public IReadOnlyDictionary<string, object> Entries { get; set; } = new Dictionary<string, object>();
    }
}
