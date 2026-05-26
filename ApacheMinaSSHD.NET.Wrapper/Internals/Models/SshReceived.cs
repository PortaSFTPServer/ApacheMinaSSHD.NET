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
