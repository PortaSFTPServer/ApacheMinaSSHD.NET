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

[Trait("Category", "Stress")]
public class StressTests : IDisposable
{
    private readonly string _tempDir;

    public StressTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SshStressTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void Repeated_start_stop_cycles()
    {
        for (int i = 0; i < 10; i++)
        {
            using var server = AMNetSshServer.SetUpDefaultServer();
            server.Host = "127.0.0.1";
            server.Port = 0;
            server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
                Path.Combine(_tempDir, $"hostkey_{i}.ser")));
            server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(
                Path.Combine(_tempDir, $"home_{i}")));
            server.SetFixedPasswordAuthenticator("user", "pass");
            server.Start();
            Assert.True(server.IsStarted());
            server.Stop(true);
            Assert.True(server.IsClosed());
        }
    }

    [Fact]
    public void Concurrent_server_instances()
    {
        var servers = new System.Collections.Concurrent.ConcurrentBag<AMNetSshServer>();
        var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        Parallel.For(0, 5, i =>
        {
            try
            {
                var server = AMNetSshServer.SetUpDefaultServer();
                server.Host = "127.0.0.1";
                server.Port = 0;
                string keyPath = Path.Combine(_tempDir, $"concurrent_key_{i}.ser");
                server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(keyPath));
                server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(
                    Path.Combine(_tempDir, $"concurrent_home_{i}")));
                server.SetFixedPasswordAuthenticator("user", "pass");
                server.Start();
                servers.Add(server);

                Assert.True(server.IsStarted());
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });

        Assert.Empty(errors);
        Assert.Equal(5, servers.Count);

        foreach (var server in servers)
        {
            try
            {
                if (server.IsStarted())
                    server.Stop(true);
                server.Dispose();
            }
            catch { }
        }
    }

    [Fact]
    public void Rapid_password_authentication_attempts()
    {
        var auth = new AMNetFixedPasswordAuthenticator("user", "correct-password");
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            auth.Authenticate("user", "wrong-password-attempt", DummySession.Instance);
        }

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 5000,
            $"1000 auth attempts took {sw.ElapsedMilliseconds}ms (expected <5000ms)");
    }

    [Fact]
    public void Rapid_publickey_authentication_attempts()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:correct-fingerprint");
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            auth.Authenticate("user", "SHA256:wrong-fingerprint-attempt", DummySession.Instance);
        }

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 5000,
            $"1000 pubkey auth attempts took {sw.ElapsedMilliseconds}ms (expected <5000ms)");
    }

    [Fact]
    public void Large_number_of_fingerprint_entries()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator();
        for (int i = 0; i < 1000; i++)
        {
            auth.AddFingerprint($"user{i}", $"SHA256:key{i}");
        }

        var sw = Stopwatch.StartNew();
        bool found = auth.Authenticate("user999", "SHA256:key999", DummySession.Instance);
        sw.Stop();

        Assert.True(found);
        Assert.True(sw.ElapsedMilliseconds < 1000,
            $"Lookup among 1000 entries took {sw.ElapsedMilliseconds}ms (expected <1000ms)");
    }

    [Fact]
    public void Composite_password_authenticator_with_many_modules()
    {
        var modules = new IAMNetPasswordAuthenticator[100];
        for (int i = 0; i < 99; i++)
        {
            int captured = i;
            modules[i] = new AMNetDelegatePasswordAuthenticator((u, p, s) =>
                u == $"user{captured}" && p == $"pass{captured}");
        }
        modules[99] = new AMNetFixedPasswordAuthenticator("target", "found");
        var composite = new AMNetCompositePasswordAuthenticator(modules);

        var sw = Stopwatch.StartNew();
        bool result = composite.Authenticate("target", "found", DummySession.Instance);
        sw.Stop();

        Assert.True(result);
        Assert.True(sw.ElapsedMilliseconds < 2000,
            $"Composite (100 modules) took {sw.ElapsedMilliseconds}ms (expected <2000ms)");
    }

    [Fact]
    public void Filesystem_accessor_isPathAllowed_under_high_load()
    {
        var acc = new AMNetSftpFileSystemAccessor();
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            var ctx = new MockSshFileSystemAccess
            {
                Operation = SshFileSystemOperation.OpenFile,
                LocalPath = Path.Combine(_tempDir, "file.txt"),
                RootPath = _tempDir,
                RemotePath = "file.txt"
            };
            acc.IsPathAllowed(ctx);
        }

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 3000,
            $"1000 IsPathAllowed calls took {sw.ElapsedMilliseconds}ms (expected <3000ms)");
    }

    [Fact]
    public void VirtualFileSystemFactory_username_sanitization_load()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 10000; i++)
        {
            factory.ResolveUserHomeDirectory($"user{i}");
        }

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 2000,
            $"10000 username resolutions took {sw.ElapsedMilliseconds}ms (expected <2000ms)");
    }

    [Fact]
    public void Config_property_roundtrip_stress()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            server.Config.MAX_AUTH_REQUESTS = i % 20;
            server.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(i % 120);
            server.Config.MAX_CONCURRENT_SESSIONS = i % 50;
            server.Config.IDLE_TIMEOUT = TimeSpan.FromMinutes(i % 30);
            int v1 = server.Config.MAX_AUTH_REQUESTS;
            var v2 = server.Config.AUTH_TIMEOUT;
            int v3 = server.Config.MAX_CONCURRENT_SESSIONS;
            var v4 = server.Config.IDLE_TIMEOUT;
        }

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 5000,
            $"100 config roundtrips took {sw.ElapsedMilliseconds}ms (expected <5000ms)");
    }

    [Fact]
    public void Rapid_server_dispose_no_crash()
    {
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 20; i++)
        {
            using var server = AMNetSshServer.SetUpDefaultServer();
            server.Host = "127.0.0.1";
            server.Port = 0;
            server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider());
            server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(
                Path.Combine(_tempDir, $"discard_home_{i}")));
            server.SetFixedPasswordAuthenticator("user", "pass");
        }

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 10000,
            $"20 server create/dispose cycles took {sw.ElapsedMilliseconds}ms (expected <10000ms)");
    }

    private sealed class DummySession : ISshSession
    {
        public static readonly DummySession Instance = new();
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
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
}
