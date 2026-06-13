// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshMove : ISshMove
    {
        public ISshSession Session {  get; set; } = null!;

        public string SourcePath { get; set; } = string.Empty;

        public string DestPath { get; set; } = string.Empty;
        public IEnumerable<string> Options { get; set; } = Array.Empty<string>();

        public Exception Exception { get; set; } = null!;
    }
}
