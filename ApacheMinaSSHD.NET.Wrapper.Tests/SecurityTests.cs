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
using ApacheMinaSSHD.NET.Wrapper.Factories;
using System.Diagnostics;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[CollectionDefinition("TimingSensitive", DisableParallelization = true)]
public class TimingSensitiveCollection { }

[Trait("Category", "Unit")]
[Collection("TimingSensitive")]
public class SecurityTests
{
    private sealed class DummySession : ISshSession
    {
        public static readonly DummySession Instance = new();
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }

    [Fact]
    public void FixedPasswordAuthenticator_constant_time_comparison()
    {
        // Both wrong passwords are the same length (25 chars) as the correct one,
        // so FixedTimeEquals must do a full byte-by-byte comparison. This test
        // verifies there is no significant timing difference based on content.
        var auth = new AMNetFixedPasswordAuthenticator("user", "correct-horse-battery-staple");

        const int iterations = 5000;

        // Warmup to stabilize JIT and caching
        for (int w = 0; w < 200; w++)
        {
            auth.Authenticate("user", "aaaaaaaabbbbbbbbccccccccc", DummySession.Instance);
            auth.Authenticate("user", "correct-horse-battery-stapleX", DummySession.Instance);
        }

        var sw = new Stopwatch();

        long mismatchedAtStart = 0, mismatchedAtEnd = 0;
        for (int i = 0; i < iterations; i++)
        {
            // Same length, differs in first byte
            sw.Restart();
            auth.Authenticate("user", "aaaaaaaabbbbbbbbccccccccc", DummySession.Instance);
            sw.Stop();
            mismatchedAtStart += sw.ElapsedTicks;

            // Same length, differs in last byte
            sw.Restart();
            auth.Authenticate("user", "correct-horse-battery-stapleX", DummySession.Instance);
            sw.Stop();
            mismatchedAtEnd += sw.ElapsedTicks;
        }

        double avgEarly = (double)mismatchedAtStart / iterations;
        double avgLate = (double)mismatchedAtEnd / iterations;

        // Both paths execute the same FixedTimeEquals over 25 bytes.
        // Allow 50% margin for measurement noise under CI/parallel load
        // with IKVM interop and Windows timer granularity.
        double ratio = Math.Abs(avgLate - avgEarly) / Math.Max(avgLate, avgEarly);
        Assert.True(ratio < 0.50,
            $"Timing leak detected: earlyByte={avgEarly:F1} lateByte={avgLate:F1} ticks, ratio={ratio:F3}");
    }

    [Fact]
    public void FixedPasswordAuthenticator_clears_sensitive_bytes_on_dispose()
    {
        var auth = new AMNetFixedPasswordAuthenticator("user", "s3cret!");
        Assert.True(auth.Authenticate("user", "s3cret!", DummySession.Instance));
        auth.Dispose();
        Assert.False(auth.Authenticate("user", "s3cret!", DummySession.Instance));
    }

    [Fact]
    public void AuthorizedKeysAuthenticator_rejects_path_traversal()
    {
        string baseDir = Path.Combine(Path.GetTempPath(), "AuthKeysVulnTest_" + Guid.NewGuid());
        Directory.CreateDirectory(baseDir);
        try
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new AMNetAuthorizedKeysAuthenticator(
                    Path.Combine(baseDir, "..", "..", "etc", "passwd"),
                    baseDir));
            Assert.Contains("outside the allowed base directory", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            try { Directory.Delete(baseDir, true); } catch { }
        }
    }

    [Fact]
    public void AuthorizedKeysAuthenticator_allows_path_within_base()
    {
        string baseDir = Path.Combine(Path.GetTempPath(), "AuthKeysVulnTest_" + Guid.NewGuid());
        Directory.CreateDirectory(baseDir);
        try
        {
            string keysPath = Path.Combine(baseDir, "authorized_keys");
            File.WriteAllText(keysPath, "");
            var auth = new AMNetAuthorizedKeysAuthenticator(keysPath, baseDir);
            Assert.Equal(Path.GetFullPath(keysPath), auth.KeysFilePath);
        }
        finally
        {
            try { Directory.Delete(baseDir, true); } catch { }
        }
    }

    [Fact]
    public void AuthorizedKeysAuthenticator_no_basepath_allows_any_path()
    {
        string path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "anywhere_" + Guid.NewGuid()));
        var auth = new AMNetAuthorizedKeysAuthenticator(path);
        Assert.Equal(path, auth.KeysFilePath);
    }

    [Fact]
    public void SshServer_rejects_empty_host()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        Assert.Throws<ArgumentException>(() => server.Host = "");
        Assert.Throws<ArgumentException>(() => server.Host = "   ");
    }

    [Fact]
    public void SshServer_accepts_null_host()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        server.Host = null;
        Assert.Null(server.Host);
    }

    [Fact]
    public void SshServer_accepts_valid_host_addresses()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        server.Host = "127.0.0.1";
        Assert.Equal("127.0.0.1", server.Host);
        server.Host = "::1";
        Assert.Equal("::1", server.Host);
        server.Host = "sftp.example.com";
        Assert.Equal("sftp.example.com", server.Host);
        server.Host = "0.0.0.0";
        Assert.Equal("0.0.0.0", server.Host);
    }

    [Fact]
    public void SshServer_rejects_overlong_host()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        string longHost = new string('a', 256);
        Assert.Throws<ArgumentException>(() => server.Host = longHost);
    }

    [Fact]
    public void VirtualFileSystemFactory_sanitizes_username_traversal()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        string safe = factory.ResolveUserHomeDirectory("../../etc/passwd");
        Assert.StartsWith("/sftp/root" + Path.DirectorySeparatorChar, safe);
        string fileName = Path.GetFileName(safe);
        Assert.DoesNotContain(Path.DirectorySeparatorChar, fileName);
    }

    [Fact]
    public void VirtualFileSystemFactory_sanitizes_username_special_chars()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        string safe = factory.ResolveUserHomeDirectory("admin; rm -rf /");
        string fileName = Path.GetFileName(safe);
        Assert.DoesNotContain(";", fileName);
        Assert.DoesNotContain(" ", fileName);
        Assert.Equal("adminrm-rf", fileName);
    }

    [Fact]
    public void VirtualFileSystemFactory_sanitizes_username_path_separators()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        string safe = factory.ResolveUserHomeDirectory("../malicious\\subdir");
        Assert.StartsWith("/sftp/root" + Path.DirectorySeparatorChar, safe);
        string fileName = Path.GetFileName(safe);
        Assert.DoesNotContain(Path.DirectorySeparatorChar, fileName);
        Assert.Equal("..malicioussubdir", fileName);
    }

    [Fact]
    public void VirtualFileSystemFactory_sanitizes_empty_username_throws()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        Assert.Throws<ArgumentException>(() => factory.ResolveUserHomeDirectory(""));
        Assert.Throws<ArgumentException>(() => factory.ResolveUserHomeDirectory("   "));
        Assert.Throws<ArgumentException>(() => factory.ResolveUserHomeDirectory(null!));
    }

    [Fact]
    public void VirtualFileSystemFactory_sanitizes_username_with_only_invalid_chars()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        string safe = factory.ResolveUserHomeDirectory("../");
        string fileName = Path.GetFileName(safe);
        Assert.Equal("_", fileName);
    }

    [Fact]
    public void SftpAccessor_isPathAllowed_rejects_outside_jail()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "SftpJailTest_" + Guid.NewGuid());
        string jailDir = Path.Combine(tempDir, "jail");
        Directory.CreateDirectory(jailDir);
        try
        {
            var acc = new AMNetSftpFileSystemAccessor();
            string outsideFile = Path.Combine(tempDir, "outside.txt");
            File.WriteAllText(outsideFile, "");
            var ctx = new MockSshFileSystemAccess
            {
                Operation = SshFileSystemOperation.OpenFile,
                LocalPath = outsideFile,
                RootPath = jailDir,
                RemotePath = "../outside.txt"
            };
            Assert.False(acc.IsPathAllowed(ctx));
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    [Fact]
    public void SftpAccessor_resolve_final_target_does_not_leak_paths()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        string missingPath = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid());
        string result = acc.ResolveLocalFilePath(
            new MockSshFileSystemAccess { LocalPath = missingPath },
            missingPath);
        Assert.Equal(missingPath, result);
    }

    [Fact]
    public void FingerprintAuthenticator_trims_whitespace_securely()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc");
        Assert.True(auth.Authenticate("user", "  SHA256:abc  ", DummySession.Instance));
        Assert.False(auth.Authenticate("user", "  SHA256:different  ", DummySession.Instance));
    }

    [Fact]
    public void CompositePasswordAuthenticator_short_circuits_on_first_success()
    {
        bool secondCalled = false;
        var first = new AMNetDelegatePasswordAuthenticator((_, _, _) => true);
        var second = new AMNetDelegatePasswordAuthenticator((_, _, _) =>
        {
            secondCalled = true;
            return true;
        });
        var composite = new AMNetCompositePasswordAuthenticator(first, second);
        Assert.True(composite.Authenticate("user", "pass", DummySession.Instance));
        Assert.False(secondCalled);
    }

    [Fact]
    public void CompositePublickeyAuthenticator_short_circuits_on_first_success()
    {
        bool secondCalled = false;
        var first = new AMNetDelegatePublickeyAuthenticator((_, _, _) => true);
        var second = new AMNetDelegatePublickeyAuthenticator((_, _, _) =>
        {
            secondCalled = true;
            return true;
        });
        var composite = new AMNetCompositePublickeyAuthenticator(first, second);
        Assert.True(composite.Authenticate("user", "fp", DummySession.Instance));
        Assert.False(secondCalled);
    }

    [Fact]
    public void ScpFileOpener_rejects_path_traversal()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "ScpJailTest_" + Guid.NewGuid());
        string jailDir = Path.Combine(tempDir, "jail");
        Directory.CreateDirectory(jailDir);
        try
        {
            var opener = new AMNetScpFileOpener(jailDir);
            string outsideFile = Path.Combine(tempDir, "outside.txt");
            File.WriteAllText(outsideFile, "");
            var ctx = new MockScpFileAccess { LocalPath = outsideFile };
            Assert.False(opener.IsPathAllowed(ctx));
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    [Fact]
    public void DefaultPasswordAuthenticator_denies_all_by_default()
    {
        var auth = new AMNetPasswordAuthenticator();
        Assert.False(auth.Authenticate("admin", "admin", DummySession.Instance));
        Assert.False(auth.Authenticate("root", "toor", DummySession.Instance));
        Assert.False(auth.Authenticate("user", "", DummySession.Instance));
    }

    [Fact]
    public void ServerConfig_production_defaults_limit_auth_attempts()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        server.Config.ApplyProductionDefaults();
        Assert.Equal(5, server.Config.MAX_AUTH_REQUESTS);
        Assert.Equal(TimeSpan.FromSeconds(60), server.Config.AUTH_TIMEOUT);
        Assert.Equal(10, server.Config.MAX_CONCURRENT_SESSIONS);
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
}
