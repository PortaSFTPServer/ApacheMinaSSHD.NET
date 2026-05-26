using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshReadWrite : ISshReadWrite
    {
        public long Offset { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public ISshSession Session { get; set; } = null!;
        public string RemoteHandle { get; set; } = string.Empty;

        // public string LocalPath { get; set; }
        public ISshHandle SshHandle { get; set; } = null!;
        public Exception Exception { get; set; } = null!;

    }
}
