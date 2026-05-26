using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshFileSystemAccess : ISshFileSystemAccess
    {
        public SshFileSystemOperation Operation { get; init; }
        public ISshSession? Session { get; init; }
        public string? RootPath { get; init; }
        public string? RemotePath { get; init; }
        public string? LocalPath { get; init; }
        public string? SourcePath { get; init; }
        public string? DestinationPath { get; init; }
        public string? RemoteHandle { get; init; }
        public string? RemoteName { get; init; }
        public string? Extension { get; init; }
        public string? FileAttributeView { get; init; }
        public string? FileAttributeName { get; init; }
        public string? Owner { get; init; }
        public string? Group { get; init; }
        public object? Value { get; init; }
        public bool IsDirectory { get; init; }
        public bool IsSymbolicLink { get; init; }
        public bool ShortName { get; init; }
        public bool FollowLinks { get; init; }
        public bool SharedLock { get; init; }
        public int Command { get; init; }
        public long Offset { get; init; }
        public long Length { get; init; }
        public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, object> Attributes { get; init; } =
            new Dictionary<string, object>();
    }
}
