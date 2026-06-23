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

using System.Security.Cryptography;
using System.Text;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class PublicKeyAuthenticatorTests : IDisposable
{
    private readonly string _tempDir;

    public PublicKeyAuthenticatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PubkeyAuthTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void FingerprintAuthenticator_correct_fingerprint()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.True(auth.Authenticate("user", "SHA256:abc123", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_wrong_fingerprint()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.False(auth.Authenticate("user", "SHA256:wrong", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_wrong_username()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("alice", "SHA256:abc123");
        Assert.False(auth.Authenticate("bob", "SHA256:abc123", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_multiple_fingerprints_per_user()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        auth.AddFingerprint("user", "SHA256:xyz789");
        Assert.True(auth.Authenticate("user", "SHA256:abc123", DummySession.Instance));
        Assert.True(auth.Authenticate("user", "SHA256:xyz789", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_empty_fingerprint_denies()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.False(auth.Authenticate("user", "", DummySession.Instance));
        Assert.False(auth.Authenticate("user", "   ", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_case_insensitive()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:ABC123");
        Assert.True(auth.Authenticate("user", "sha256:abc123", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_trim_whitespace()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.True(auth.Authenticate("user", "  SHA256:abc123  ", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_different_users_independent()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("alice", "SHA256:abc");
        auth.AddFingerprint("bob", "SHA256:xyz");
        Assert.True(auth.Authenticate("alice", "SHA256:abc", DummySession.Instance));
        Assert.True(auth.Authenticate("bob", "SHA256:xyz", DummySession.Instance));
        Assert.False(auth.Authenticate("alice", "SHA256:xyz", DummySession.Instance));
        Assert.False(auth.Authenticate("bob", "SHA256:abc", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_chaining()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator()
            .AddFingerprint("user", "SHA256:a")
            .AddFingerprint("user", "SHA256:b");
        Assert.True(auth.Authenticate("user", "SHA256:a", DummySession.Instance));
        Assert.True(auth.Authenticate("user", "SHA256:b", DummySession.Instance));
    }

    [Fact]
    public void DelegatePublickeyAuthenticator_calls_callback()
    {
        bool called = false;
        var auth = new AMNetDelegatePublickeyAuthenticator(
            (username, fingerprint, session) =>
            {
                called = true;
                return username == "user" && fingerprint == "SHA256:abc";
            });
        Assert.True(auth.Authenticate("user", "SHA256:abc", DummySession.Instance));
        Assert.True(called);
    }

    [Fact]
    public void DelegatePublickeyAuthenticator_null_callback_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetDelegatePublickeyAuthenticator(null!));
    }

    [Fact]
    public void CompositePublickeyAuthenticator_tries_in_order()
    {
        var alwaysNo = new AMNetDelegatePublickeyAuthenticator((_, _, _) => false);
        var alwaysYes = new AMNetDelegatePublickeyAuthenticator((_, _, _) => true);
        var composite = new AMNetCompositePublickeyAuthenticator(alwaysNo, alwaysYes);

        Assert.True(composite.Authenticate("user", "fp", DummySession.Instance));
    }

    [Fact]
    public void CompositePublickeyAuthenticator_all_deny()
    {
        var alwaysNo1 = new AMNetDelegatePublickeyAuthenticator((_, _, _) => false);
        var alwaysNo2 = new AMNetDelegatePublickeyAuthenticator((_, _, _) => false);
        var composite = new AMNetCompositePublickeyAuthenticator(alwaysNo1, alwaysNo2);

        Assert.False(composite.Authenticate("user", "fp", DummySession.Instance));
    }

    [Fact]
    public void CompositePublickeyAuthenticator_empty_creates_no_authenticators()
    {
        var composite = new AMNetCompositePublickeyAuthenticator();
        Assert.Empty(composite.Authenticators);
    }

    [Fact]
    public void CompositePublickeyAuthenticator_null_entry_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetCompositePublickeyAuthenticator(
            new IAMNetPublickeyAuthenticator[] { null! }));
    }

    [Fact]
    public void DefaultPublickeyAuthenticator_denies_all()
    {
        var auth = new AMNetPublickeyAuthenticator();
        Assert.False(auth.Authenticate("any", "SHA256:anything", DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_openssh_public_key_format_accepts()
    {
        using var rsa = RSA.Create(2048);
        var sshWire = EncodeRsaToSshWire(rsa);
        var fp = SshFingerprint(sshWire);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "user.pub"), "ssh-rsa " + Convert.ToBase64String(sshWire) + " comment");
        Assert.True(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", fp, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_ssh2_public_key_format_accepts()
    {
        using var rsa = RSA.Create(2048);
        var sshWire = EncodeRsaToSshWire(rsa);
        var fp = SshFingerprint(sshWire);
        var dir = CreateAuthKeysDir();
        var b64 = Convert.ToBase64String(sshWire);
        File.WriteAllText(Path.Combine(dir, "user.pub"),
            "---- BEGIN SSH2 PUBLIC KEY ----\nComment: \"test\"\n" + b64 + "\n---- END SSH2 PUBLIC KEY ----");
        Assert.True(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", fp, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_pem_public_key_format_accepts()
    {
        using var rsa = RSA.Create(2048);
        var sshWire = EncodeRsaToSshWire(rsa);
        var fp = SshFingerprint(sshWire);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "user.pem"), rsa.ExportSubjectPublicKeyInfoPem());
        Assert.True(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", fp, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_pem_private_key_format_accepts()
    {
        using var rsa = RSA.Create(2048);
        var sshWire = EncodeRsaToSshWire(rsa);
        var fp = SshFingerprint(sshWire);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "user.key"), rsa.ExportRSAPrivateKeyPem());
        Assert.True(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", fp, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_ecdsa_key_format_accepts()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var sshWire = EncodeEcdsaToSshWire(ecdsa);
        var fp = SshFingerprint(sshWire);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "user_ecdsa.pub"), "ecdsa-sha2-nistp256 " + Convert.ToBase64String(sshWire));
        Assert.True(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", fp, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_multiple_users_isolated()
    {
        using var rsa1 = RSA.Create(2048);
        using var rsa2 = RSA.Create(2048);
        var w1 = EncodeRsaToSshWire(rsa1);
        var w2 = EncodeRsaToSshWire(rsa2);
        var fp1 = SshFingerprint(w1);
        var fp2 = SshFingerprint(w2);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "alice.pub"), "ssh-rsa " + Convert.ToBase64String(w1));
        File.WriteAllText(Path.Combine(dir, "bob.pub"), "ssh-rsa " + Convert.ToBase64String(w2));
        var auth = new AMNetPublickeyAuthenticator(_tempDir);
        Assert.True(auth.Authenticate("alice", fp1, DummySession.Instance));
        Assert.True(auth.Authenticate("bob", fp2, DummySession.Instance));
        Assert.False(auth.Authenticate("alice", fp2, DummySession.Instance));
        Assert.False(auth.Authenticate("bob", fp1, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_no_directory_returns_false()
    {
        var auth = new AMNetPublickeyAuthenticator(Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid()));
        Assert.False(auth.Authenticate("user", "SHA256:anything", DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_wrong_fingerprint_rejected()
    {
        using var rsa = RSA.Create(2048);
        var sshWire = EncodeRsaToSshWire(rsa);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "user.pub"), "ssh-rsa " + Convert.ToBase64String(sshWire));
        Assert.False(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", "SHA256:wrong", DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_no_matching_file_rejected()
    {
        using var rsa = RSA.Create(2048);
        var sshWire = EncodeRsaToSshWire(rsa);
        var fp = SshFingerprint(sshWire);
        var dir = CreateAuthKeysDir();
        File.WriteAllText(Path.Combine(dir, "alice.pub"), "ssh-rsa " + Convert.ToBase64String(sshWire));
        Assert.False(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("bob", fp, DummySession.Instance));
    }

    [Fact]
    public void DirectoryBacked_empty_directory_rejected()
    {
        CreateAuthKeysDir();
        Assert.False(new AMNetPublickeyAuthenticator(_tempDir).Authenticate("user", "SHA256:anything", DummySession.Instance));
    }

    private string CreateAuthKeysDir()
    {
        var d = Path.Combine(_tempDir, "Authorized_Keys");
        Directory.CreateDirectory(d);
        return d;
    }

    private static byte[] EncodeRsaToSshWire(RSA rsa)
    {
        var p = rsa.ExportParameters(false);
        using var ms = new MemoryStream();
        WriteSshString(ms, "ssh-rsa");
        WriteSshMpint(ms, p.Exponent!);
        WriteSshMpint(ms, p.Modulus!);
        return ms.ToArray();
    }

    private static byte[] EncodeEcdsaToSshWire(ECDsa ecdsa)
    {
        var p = ecdsa.ExportParameters(false);
        using var ms = new MemoryStream();
        WriteSshString(ms, "ecdsa-sha2-nistp256");
        WriteSshString(ms, "nistp256");
        var point = new byte[1 + p.Q.X!.Length + p.Q.Y!.Length];
        point[0] = 0x04;
        Buffer.BlockCopy(p.Q.X, 0, point, 1, p.Q.X.Length);
        Buffer.BlockCopy(p.Q.Y, 0, point, 1 + p.Q.X.Length, p.Q.Y.Length);
        WriteSshString(ms, point);
        return ms.ToArray();
    }

    private static string SshFingerprint(byte[] sshWire)
    {
        return "SHA256:" + Convert.ToBase64String(SHA256.HashData(sshWire)).Replace("=", "");
    }

    private static void WriteSshString(MemoryStream ms, string value)
    {
        var b = Encoding.ASCII.GetBytes(value);
        WriteUint32(ms, b.Length);
        ms.Write(b);
    }

    private static void WriteSshString(MemoryStream ms, byte[] value)
    {
        WriteUint32(ms, value.Length);
        ms.Write(value);
    }

    private static void WriteSshMpint(MemoryStream ms, byte[] value)
    {
        if (value.Length > 0 && (value[0] & 0x80) != 0)
        {
            WriteUint32(ms, value.Length + 1);
            ms.WriteByte(0);
            ms.Write(value);
        }
        else
        {
            WriteUint32(ms, value.Length);
            ms.Write(value);
        }
    }

    private static void WriteUint32(MemoryStream ms, int value)
    {
        var b = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(b);
        ms.Write(b);
    }

    private sealed class DummySession : ISshSession
    {
        public static readonly DummySession Instance = new();
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }
}
