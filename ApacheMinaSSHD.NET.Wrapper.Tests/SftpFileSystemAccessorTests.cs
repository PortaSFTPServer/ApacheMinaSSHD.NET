using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class SftpFileSystemAccessorTests : IDisposable
{
    private readonly string _tempDir;

    public SftpFileSystemAccessorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SftpAccessorTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private sealed class TestableAccessor : AMNetSftpFileSystemAccessor
    {
        public IReadOnlySet<string> HiddenNamesOverride { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "secret_data" };
        protected override IReadOnlySet<string> HiddenNames => HiddenNamesOverride;

        public bool CallIsVisibleByDefault(string? localPath) => IsVisibleByDefault(localPath);
    }

    private sealed class MockSshFileSystemAccess : ISshFileSystemAccess
    {
        public SshFileSystemOperation Operation { get; set; }
        public ISshSession? Session { get; set; }
        public string? RootPath { get; set; }
        public string? RemotePath { get; set; }
        public string? LocalPath { get; set; }
        public string? SourcePath { get; set; }
        public string? DestinationPath { get; set; }
        public string? RemoteHandle { get; set; }
        public string? RemoteName { get; set; }
        public string? Extension { get; set; }
        public string? FileAttributeView { get; set; }
        public string? FileAttributeName { get; set; }
        public string? Owner { get; set; }
        public string? Group { get; set; }
        public object? Value { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsSymbolicLink { get; set; }
        public bool ShortName { get; set; }
        public bool FollowLinks { get; set; }
        public bool SharedLock { get; set; }
        public int Command { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
        public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }

    [Fact]
    public void IsVisibleByDefault_null_returns_true()
    {
        var acc = new TestableAccessor();
        Assert.True(acc.CallIsVisibleByDefault(null));
    }

    [Fact]
    public void IsVisibleByDefault_empty_returns_true()
    {
        var acc = new TestableAccessor();
        Assert.True(acc.CallIsVisibleByDefault(""));
    }

    [Fact]
    public void IsVisibleByDefault_whitespace_returns_true()
    {
        var acc = new TestableAccessor();
        Assert.True(acc.CallIsVisibleByDefault("   "));
    }

    [Fact]
    public void IsVisibleByDefault_dot_returns_true()
    {
        var acc = new TestableAccessor();
        Assert.True(acc.CallIsVisibleByDefault("."));
    }

    [Fact]
    public void IsVisibleByDefault_dotdot_returns_true()
    {
        var acc = new TestableAccessor();
        Assert.True(acc.CallIsVisibleByDefault(".."));
    }

    [Fact]
    public void IsVisibleByDefault_dotfile_returns_false()
    {
        var acc = new TestableAccessor();
        Assert.False(acc.CallIsVisibleByDefault(".hidden"));
    }

    [Fact]
    public void IsVisibleByDefault_dotfile_with_path_returns_false()
    {
        var acc = new TestableAccessor();
        Assert.False(acc.CallIsVisibleByDefault("/some/path/.secret"));
    }

    [Fact]
    public void IsVisibleByDefault_hidden_name_exact_match_returns_false()
    {
        var acc = new TestableAccessor();
        string file = Path.Combine(_tempDir, "secret_data");
        File.WriteAllText(file, "");
        Assert.False(acc.CallIsVisibleByDefault(file));
    }

    [Fact]
    public void IsVisibleByDefault_hidden_name_prefix_dot_match_returns_false()
    {
        var acc = new TestableAccessor();
        string file = Path.Combine(_tempDir, "secret_data.txt");
        File.WriteAllText(file, "");
        Assert.False(acc.CallIsVisibleByDefault(file));
    }

    [Fact]
    public void IsVisibleByDefault_custom_hidden_name_works()
    {
        var acc = new TestableAccessor();
        acc.HiddenNamesOverride = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "custom_pattern" };
        string file = Path.Combine(_tempDir, "custom_pattern.cs");
        File.WriteAllText(file, "");
        Assert.False(acc.CallIsVisibleByDefault(file));
    }

    [Fact]
    public void IsVisibleByDefault_normal_file_returns_true()
    {
        var acc = new TestableAccessor();
        string file = Path.Combine(_tempDir, "readme.txt");
        File.WriteAllText(file, "hello");
        Assert.True(acc.CallIsVisibleByDefault(file));
    }

    [Fact]
    public void IsVisibleByDefault_file_with_hidden_attribute_returns_false()
    {
        var acc = new TestableAccessor();
        string file = Path.Combine(_tempDir, "system.dat");
        File.WriteAllText(file, "");
        File.SetAttributes(file, FileAttributes.Hidden);
        Assert.False(acc.CallIsVisibleByDefault(file));
    }

    [Fact]
    public void IsVisibleByDefault_nonexistent_file_returns_true()
    {
        var acc = new TestableAccessor();
        Assert.True(acc.CallIsVisibleByDefault(Path.Combine(_tempDir, "nonexistent.txt")));
    }

    [Fact]
    public void IsPathAllowed_null_localPath_returns_true()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var ctx = new MockSshFileSystemAccess { LocalPath = null, RootPath = null, RemotePath = "some/file" };
        Assert.True(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_root_lookup_bypasses_jail()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var ctx = new MockSshFileSystemAccess
        {
            Operation = SshFileSystemOperation.ResolveLocalFilePath,
            RemotePath = "/",
            LocalPath = "/sftp/root",
            RootPath = "/sftp/root"
        };
        Assert.True(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_dot_remotePath_bypasses_jail()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var ctx = new MockSshFileSystemAccess
        {
            Operation = SshFileSystemOperation.ResolveLocalFilePath,
            RemotePath = ".",
            LocalPath = "/sftp/root",
            RootPath = "/sftp/root"
        };
        Assert.True(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_empty_remotePath_bypasses_jail()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var ctx = new MockSshFileSystemAccess
        {
            Operation = SshFileSystemOperation.ResolveLocalFilePath,
            RemotePath = "",
            LocalPath = "/sftp/root",
            RootPath = "/sftp/root"
        };
        Assert.True(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_within_root_returns_true()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        string root = _tempDir;
        string file = Path.Combine(root, "allowed.txt");
        File.WriteAllText(file, "");
        var ctx = new MockSshFileSystemAccess
        {
            Operation = SshFileSystemOperation.OpenFile,
            LocalPath = file,
            RootPath = root,
            RemotePath = "allowed.txt"
        };
        Assert.True(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_outside_root_returns_false()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        string root = Path.Combine(_tempDir, "jail");
        Directory.CreateDirectory(root);
        string file = Path.Combine(_tempDir, "outside.txt");
        File.WriteAllText(file, "");
        var ctx = new MockSshFileSystemAccess
        {
            Operation = SshFileSystemOperation.OpenFile,
            LocalPath = file,
            RootPath = root,
            RemotePath = "../outside.txt"
        };
        Assert.False(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_exact_root_path_returns_true()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        string root = _tempDir;
        var ctx = new MockSshFileSystemAccess
        {
            Operation = SshFileSystemOperation.OpenFile,
            LocalPath = root,
            RootPath = root,
            RemotePath = "."
        };
        Assert.True(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_dotfile_is_rejected()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        string file = Path.Combine(_tempDir, ".gitconfig");
        File.WriteAllText(file, "");
        var ctx = new MockSshFileSystemAccess
        {
            LocalPath = file,
            RootPath = _tempDir,
            RemotePath = ".gitconfig"
        };
        Assert.False(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_hidden_name_is_rejected()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        string file = Path.Combine(_tempDir, "secret_data.txt");
        File.WriteAllText(file, "");
        var ctx = new MockSshFileSystemAccess
        {
            LocalPath = file,
            RootPath = _tempDir,
            RemotePath = "secret_data.txt"
        };
        Assert.False(acc.IsPathAllowed(ctx));
    }

    [Fact]
    public void ShouldIncludeDirectoryEntry_delegates_to_IsVisibleByDefault()
    {
        var acc = new TestableAccessor();
        var ctx = new MockSshFileSystemAccess { LocalPath = _tempDir };
        Assert.True(acc.ShouldIncludeDirectoryEntry(ctx));

        ctx.LocalPath = Path.Combine(_tempDir, ".hidden");
        Assert.False(acc.ShouldIncludeDirectoryEntry(ctx));
    }

    [Fact]
    public void ResolveLocalFilePath_identity()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var ctx = new MockSshFileSystemAccess();
        string result = acc.ResolveLocalFilePath(ctx, "/some/path");
        Assert.Equal("/some/path", result);
    }

    [Fact]
    public void NoFollow_passthrough()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var ctx = new MockSshFileSystemAccess();
        Assert.True(acc.NoFollow(ctx, true));
        Assert.False(acc.NoFollow(ctx, false));
    }
}
