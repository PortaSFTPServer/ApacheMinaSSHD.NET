// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class SshAlgorithmsTests
{
    [Fact]
    public void Ciphers_have_expected_values()
    {
        Assert.Equal("aes128-ctr", AMNetSshAlgorithms.Ciphers.Aes128Ctr);
        Assert.Equal("aes192-ctr", AMNetSshAlgorithms.Ciphers.Aes192Ctr);
        Assert.Equal("aes256-ctr", AMNetSshAlgorithms.Ciphers.Aes256Ctr);
        Assert.Equal("aes128-gcm@openssh.com", AMNetSshAlgorithms.Ciphers.Aes128Gcm);
        Assert.Equal("aes256-gcm@openssh.com", AMNetSshAlgorithms.Ciphers.Aes256Gcm);
        Assert.Equal("chacha20-poly1305@openssh.com", AMNetSshAlgorithms.Ciphers.ChaCha20Poly1305);
    }

    [Fact]
    public void Macs_have_expected_values()
    {
        Assert.Equal("hmac-sha2-256", AMNetSshAlgorithms.Macs.HmacSha256);
        Assert.Equal("hmac-sha2-512", AMNetSshAlgorithms.Macs.HmacSha512);
        Assert.Equal("hmac-sha2-256-etm@openssh.com", AMNetSshAlgorithms.Macs.HmacSha256Etm);
        Assert.Equal("hmac-sha2-512-etm@openssh.com", AMNetSshAlgorithms.Macs.HmacSha512Etm);
    }

    [Fact]
    public void KeyExchange_have_expected_values()
    {
        Assert.Equal("diffie-hellman-group14-sha256", AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup14Sha256);
        Assert.Equal("diffie-hellman-group16-sha512", AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup16Sha512);
        Assert.Equal("diffie-hellman-group18-sha512", AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup18Sha512);
        Assert.Equal("ecdh-sha2-nistp256", AMNetSshAlgorithms.KeyExchange.EcdhNistp256);
        Assert.Equal("ecdh-sha2-nistp384", AMNetSshAlgorithms.KeyExchange.EcdhNistp384);
        Assert.Equal("ecdh-sha2-nistp521", AMNetSshAlgorithms.KeyExchange.EcdhNistp521);
        Assert.Equal("curve25519-sha256", AMNetSshAlgorithms.KeyExchange.Curve25519Sha256);
        Assert.Equal("curve25519-sha256@libssh.org", AMNetSshAlgorithms.KeyExchange.Curve25519Sha256LibSsh);
    }

    [Fact]
    public void HostKeys_have_expected_values()
    {
        Assert.Equal("ssh-ed25519", AMNetSshAlgorithms.HostKeys.Ed25519);
        Assert.Equal("ecdsa-sha2-nistp256", AMNetSshAlgorithms.HostKeys.EcdsaNistp256);
        Assert.Equal("ecdsa-sha2-nistp384", AMNetSshAlgorithms.HostKeys.EcdsaNistp384);
        Assert.Equal("ecdsa-sha2-nistp521", AMNetSshAlgorithms.HostKeys.EcdsaNistp521);
        Assert.Equal("rsa-sha2-256", AMNetSshAlgorithms.HostKeys.RsaSha256);
        Assert.Equal("rsa-sha2-512", AMNetSshAlgorithms.HostKeys.RsaSha512);
        Assert.Equal("ssh-rsa", AMNetSshAlgorithms.HostKeys.SshRsa);
    }

    [Fact]
    public void HostKeyAlgorithms_have_expected_values()
    {
        Assert.Equal("RSA", AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
        Assert.Equal("DSA", AMNetSshAlgorithms.HostKeyAlgorithms.Dsa);
        Assert.Equal("EC", AMNetSshAlgorithms.HostKeyAlgorithms.Ecdsa);
        Assert.Equal("EdDSA", AMNetSshAlgorithms.HostKeyAlgorithms.Ed25519);
    }

    [Fact]
    public void Presets_are_not_empty()
    {
        Assert.NotEmpty(AMNetSshAlgorithms.Presets.ModernCiphers);
        Assert.NotEmpty(AMNetSshAlgorithms.Presets.ModernMacs);
        Assert.NotEmpty(AMNetSshAlgorithms.Presets.ModernKeyExchanges);
        Assert.NotEmpty(AMNetSshAlgorithms.Presets.ModernHostKeys);
    }

    [Fact]
    public void ModernCiphers_prefers_chacha20()
    {
        var ciphers = AMNetSshAlgorithms.Presets.ModernCiphers;
        Assert.Equal("chacha20-poly1305@openssh.com", ciphers[0]);
    }

    [Fact]
    public void ModernKeyExchanges_prefers_curve25519()
    {
        var kex = AMNetSshAlgorithms.Presets.ModernKeyExchanges;
        Assert.Equal("curve25519-sha256", kex[0]);
    }

    [Fact]
    public void ModernHostKeys_prefers_ed25519()
    {
        var hostKeys = AMNetSshAlgorithms.Presets.ModernHostKeys;
        Assert.Equal("ssh-ed25519", hostKeys[0]);
    }

    [Fact]
    public void All_constants_are_distinct()
    {
        var all = new HashSet<string>
        {
            AMNetSshAlgorithms.Ciphers.Aes128Ctr,
            AMNetSshAlgorithms.Ciphers.Aes192Ctr,
            AMNetSshAlgorithms.Ciphers.Aes256Ctr,
            AMNetSshAlgorithms.Ciphers.Aes128Gcm,
            AMNetSshAlgorithms.Ciphers.Aes256Gcm,
            AMNetSshAlgorithms.Ciphers.ChaCha20Poly1305,
            AMNetSshAlgorithms.Macs.HmacSha256,
            AMNetSshAlgorithms.Macs.HmacSha512,
            AMNetSshAlgorithms.Macs.HmacSha256Etm,
            AMNetSshAlgorithms.Macs.HmacSha512Etm,
            AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup14Sha256,
            AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup16Sha512,
            AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup18Sha512,
            AMNetSshAlgorithms.KeyExchange.EcdhNistp256,
            AMNetSshAlgorithms.KeyExchange.EcdhNistp384,
            AMNetSshAlgorithms.KeyExchange.EcdhNistp521,
            AMNetSshAlgorithms.KeyExchange.Curve25519Sha256,
            AMNetSshAlgorithms.KeyExchange.Curve25519Sha256LibSsh,
            AMNetSshAlgorithms.HostKeys.Ed25519,
            AMNetSshAlgorithms.HostKeys.EcdsaNistp256,
            AMNetSshAlgorithms.HostKeys.EcdsaNistp384,
            AMNetSshAlgorithms.HostKeys.EcdsaNistp521,
            AMNetSshAlgorithms.HostKeys.RsaSha256,
            AMNetSshAlgorithms.HostKeys.RsaSha512,
            AMNetSshAlgorithms.HostKeys.SshRsa,
        };
        Assert.Equal(25, all.Count);
    }
}
