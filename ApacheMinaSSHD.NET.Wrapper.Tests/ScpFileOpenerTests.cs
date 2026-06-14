// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class ScpFileOpenerTests : IDisposable
{
    private readonly string _tempDir;

    public ScpFileOpenerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ScpOpenerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private sealed class MockScpFileAccess : ISshScpFileAccess
    {
        public SshScpFileOperation Operation { get; set; }
        public ISshSession? Session { get; set; }
        public string? RootPath { get; set; }
        public string? LocalPath { get; set; }
        public string? RequestedPath { get; set; }
        public string? FileName { get; set; }
        public string? Pattern { get; set; }
        public string? Command { get; set; }
        public bool Recursive { get; set; }
        public bool ShouldBeDirectory { get; set; }
        public bool PreserveTimestamp { get; set; }
        public bool IsDirectory { get; set; }
        public long Length { get; set; }
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }

    private sealed class TestableScpFileOpener : AMNetScpFileOpener
    {
        public TestableScpFileOpener(string? rootPath = null) : base(rootPath) { }

        public IReadOnlySet<string> HiddenNamesOverride { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "secret_data" };
        protected override IReadOnlySet<string> HiddenNames => HiddenNamesOverride;
    }

    [Fact]
    public void Constructor_no_root()
    {
        var opener = new AMNetScpFileOpener();
        Assert.Null(opener.RootPath);
    }

    [Fact]
    public void Constructor_with_root_normalizes()
    {
        var opener = new AMNetScpFileOpener(_tempDir);
        Assert.Equal(Path.GetFullPath(_tempDir), opener.RootPath);
    }

    [Fact]
    public void Constructor_with_null_root_becomes_null()
    {
        var opener = new AMNetScpFileOpener(null);
        Assert.Null(opener.RootPath);
    }

    [Fact]
    public void Constructor_with_whitespace_root_becomes_null()
    {
        var opener = new AMNetScpFileOpener("   ");
        Assert.Null(opener.RootPath);
    }

    [Fact]
    public void IsPathAllowed_null_localPath_returns_true()
    {
        var opener = new AMNetScpFileOpener();
        var ctx = new MockScpFileAccess { LocalPath = null };
        Assert.True(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_empty_localPath_returns_true()
    {
        var opener = new AMNetScpFileOpener();
        var ctx = new MockScpFileAccess { LocalPath = "" };
        Assert.True(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_excludes_dotfiles()
    {
        var opener = new AMNetScpFileOpener();
        string file = Path.Combine(_tempDir, ".ssh_config");
        File.WriteAllText(file, "");
        var ctx = new MockScpFileAccess { LocalPath = file };
        Assert.False(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_excludes_hidden_name_exact()
    {
        var opener = new AMNetScpFileOpener();
        string file = Path.Combine(_tempDir, "secret_data");
        File.WriteAllText(file, "");
        var ctx = new MockScpFileAccess { LocalPath = file };
        Assert.False(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_excludes_hidden_name_prefix_dot()
    {
        var opener = new AMNetScpFileOpener();
        string file = Path.Combine(_tempDir, "secret_data.backup");
        File.WriteAllText(file, "");
        var ctx = new MockScpFileAccess { LocalPath = file };
        Assert.False(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_excludes_hidden_file_attribute()
    {
        var opener = new AMNetScpFileOpener();
        string file = Path.Combine(_tempDir, "normal_name.txt");
        File.WriteAllText(file, "");
        File.SetAttributes(file, FileAttributes.Hidden);
        var ctx = new MockScpFileAccess { LocalPath = file };
        Assert.False(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_with_root_in_jail()
    {
        var opener = new AMNetScpFileOpener(_tempDir);
        string file = Path.Combine(_tempDir, "allowed.txt");
        File.WriteAllText(file, "");
        var ctx = new MockScpFileAccess { LocalPath = file };
        Assert.True(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_with_root_outside_jail()
    {
        var opener = new AMNetScpFileOpener(Path.Combine(_tempDir, "jail"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "jail"));
        string file = Path.Combine(_tempDir, "outside.txt");
        File.WriteAllText(file, "");
        var ctx = new MockScpFileAccess { LocalPath = file };
        Assert.False(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void IsPathAllowed_with_root_exact_path()
    {
        var opener = new AMNetScpFileOpener(_tempDir);
        var ctx = new MockScpFileAccess { LocalPath = _tempDir };
        Assert.True(opener.IsPathAllowed(ctx));
    }

    [Fact]
    public void ShouldIncludeDirectoryEntry_delegates_visibility()
    {
        var opener = new AMNetScpFileOpener();
        string normal = Path.Combine(_tempDir, "readme.txt");
        File.WriteAllText(normal, "");
        Assert.True(opener.ShouldIncludeDirectoryEntry(new MockScpFileAccess { LocalPath = normal }));

        string hidden = Path.Combine(_tempDir, ".config");
        File.WriteAllText(hidden, "");
        Assert.False(opener.ShouldIncludeDirectoryEntry(new MockScpFileAccess { LocalPath = hidden }));
    }

    [Fact]
    public void ResolveLocalPath_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        var ctx = new MockScpFileAccess();
        Assert.Equal("/original", opener.ResolveLocalPath(ctx, "/original"));
    }

    [Fact]
    public void ResolveIncomingFilePath_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        var ctx = new MockScpFileAccess();
        Assert.Equal("/incoming", opener.ResolveIncomingFilePath(ctx, "/incoming"));
    }

    [Fact]
    public void ResolveOutgoingFilePath_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        var ctx = new MockScpFileAccess();
        Assert.Equal("/outgoing", opener.ResolveOutgoingFilePath(ctx, "/outgoing"));
    }

    [Fact]
    public void ShouldSendAsRegularFile_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        Assert.True(opener.ShouldSendAsRegularFile(new MockScpFileAccess(), true));
        Assert.False(opener.ShouldSendAsRegularFile(new MockScpFileAccess(), false));
    }

    [Fact]
    public void ShouldSendAsDirectory_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        Assert.True(opener.ShouldSendAsDirectory(new MockScpFileAccess(), true));
        Assert.False(opener.ShouldSendAsDirectory(new MockScpFileAccess(), false));
    }

    [Fact]
    public void ReadLocalBasicFileAttributes_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        var attrs = new Dictionary<string, object> { ["size"] = 100L };
        var result = opener.ReadLocalBasicFileAttributes(new MockScpFileAccess(), attrs);
        Assert.Equal(100L, result["size"]);
    }

    [Fact]
    public void GetLocalFilePermissions_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        var perms = new List<string> { "rwxr-xr-x" };
        var result = opener.GetLocalFilePermissions(new MockScpFileAccess(), perms);
        Assert.Equal(perms, result);
    }

    [Fact]
    public void GetMatchingFilesToSend_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        var paths = new List<string> { "file1.txt", "file2.txt" };
        var result = opener.GetMatchingFilesToSend(new MockScpFileAccess(), paths);
        Assert.Equal(paths, result);
    }

    [Fact]
    public void ResolveIncomingReceiveLocation_passthrough()
    {
        var opener = new AMNetScpFileOpener();
        Assert.Equal("/incoming", opener.ResolveIncomingReceiveLocation(new MockScpFileAccess(), "/incoming"));
    }

    [Fact]
    public void OpenRead_noop()
    {
        new AMNetScpFileOpener().OpenRead(new MockScpFileAccess());
    }
}
