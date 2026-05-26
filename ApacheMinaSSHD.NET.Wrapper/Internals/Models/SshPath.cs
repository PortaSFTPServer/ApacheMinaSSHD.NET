using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshPath : ISshPath
    {
        public ISshSession Session { get; set; } = null!;

        public string Path { get; set; } = string.Empty;

        public bool IsDirectory { get; set; } = false;

        public IReadOnlyDictionary<string, object> Attributes { get; set; } =
            new Dictionary<string, object>();

        public Exception Exception { get; set; } = null!;
    }
}
