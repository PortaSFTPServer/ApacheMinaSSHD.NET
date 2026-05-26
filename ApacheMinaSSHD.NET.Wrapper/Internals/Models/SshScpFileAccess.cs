using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal sealed class SshScpFileAccess : ISshScpFileAccess
    {
        public SshScpFileOperation Operation { get; init; }
        public ISshSession? Session { get; init; }
        public string? RootPath { get; init; }
        public string? LocalPath { get; init; }
        public string? RequestedPath { get; init; }
        public string? FileName { get; init; }
        public string? Pattern { get; init; }
        public string? Command { get; init; }
        public bool Recursive { get; init; }
        public bool ShouldBeDirectory { get; init; }
        public bool PreserveTimestamp { get; init; }
        public bool IsDirectory { get; init; }
        public long Length { get; init; }
        public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, object> Attributes { get; init; } =
            new Dictionary<string, object>();
    }
}
