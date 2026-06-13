// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshReceived : ISshReceived
    {
        public ISshSession SshSession { get; set; } = null!;

        public int Type { get; set; }
        public string Extension { get; set; } = string.Empty;

        public int Id { get; set; } = 0;
    }
}
