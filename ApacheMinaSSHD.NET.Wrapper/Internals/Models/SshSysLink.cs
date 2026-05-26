using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshSysLink: ISshSysLink
    {

        public ISshSession Session { get; set; } = null!;

        public string SourcePath { get; set; } = string.Empty;

        public string DestPath { get; set; } = string.Empty;

        public bool SymLink { get; set; }
        public Exception Exception { get; set; } = null!;
    }
}
