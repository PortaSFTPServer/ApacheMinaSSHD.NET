using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshReadEntries : ISshReadEntries
    {
        public ISshSession Session { get; set; } = null!;

        public string RemoteHandle { get; set; } = string.Empty;

        public ISshDirectoryHandle DirectoryHandle { get; set; } = null!;

        public IReadOnlyDictionary<string, object> Entries { get; set; } =
            new Dictionary<string, object>();

        public Exception Exception { get; set; } = null!;
    }
}
