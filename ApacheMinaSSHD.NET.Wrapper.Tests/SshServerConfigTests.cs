// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class SshServerConfigTests : IDisposable
{
    private readonly AMNetSshServer _server;
    private readonly AMNetSshServerConfig _config;

    public SshServerConfigTests()
    {
        _server = AMNetSshServer.SetUpDefaultServer();
        _config = _server.Config;
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    [Fact]
    public void ApplyProductionDefaults_sets_expected_values()
    {
        _config.ApplyProductionDefaults();

        Assert.Equal(5, _config.MAX_AUTH_REQUESTS);
        Assert.Equal(TimeSpan.FromSeconds(60), _config.AUTH_TIMEOUT);
        Assert.Equal(10, _config.MAX_CONCURRENT_SESSIONS);
        Assert.Equal(10, _config.MAX_CONCURRENT_CHANNELS);
        Assert.Equal(Environment.ProcessorCount * 2, _config.NIO_WORKERS);
        Assert.Equal(TimeSpan.FromMinutes(10), _config.IDLE_TIMEOUT);
        Assert.Equal(TimeSpan.FromSeconds(45), _config.HEARTBEAT_INTERVAL);
        Assert.Equal(1024L * 1024L * 1024L, _config.REKEY_BYTES_LIMIT);
        Assert.Equal(TimeSpan.FromHours(1), _config.REKEY_TIME_LIMIT);
    }

    [Fact]
    public void Default_AUTH_METHODS_is_empty()
    {
        Assert.Equal("", _config.AUTH_METHODS);
    }

    [Fact]
    public void Set_and_get_AUTH_METHODS()
    {
        _config.AUTH_METHODS = "publickey,password";
        Assert.Equal("publickey,password", _config.AUTH_METHODS);
    }

    [Fact]
    public void SetAuthenticationMethods_formats_correctly()
    {
        _config.SetAuthenticationMethods(
            AMNetSshAuthenticationMethods.PublicKey,
            AMNetSshAuthenticationMethods.RequireAll(
                AMNetSshAuthenticationMethods.Password,
                AMNetSshAuthenticationMethods.KeyboardInteractive));

        Assert.Equal("publickey password,keyboard-interactive", _config.AUTH_METHODS);
    }

    [Fact]
    public void SetAuthenticationMethodGroups_formats_correctly()
    {
        _config.SetAuthenticationMethodGroups(
            new[] { AMNetSshAuthenticationMethods.PublicKey },
            new[] { AMNetSshAuthenticationMethods.Password });

        Assert.Equal("publickey password", _config.AUTH_METHODS);
    }

    [Fact]
    public void GetConfiguredAuthenticationMethods_parses()
    {
        _config.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
        var methods = _config.GetConfiguredAuthenticationMethods();
        Assert.Single(methods);
        Assert.Equal(["publickey"], methods[0]);
    }

    [Fact]
    public void MAX_AUTH_REQUESTS_default()
    {
        Assert.Equal(10, _config.MAX_AUTH_REQUESTS);
    }

    [Fact]
    public void MAX_AUTH_REQUESTS_roundtrip()
    {
        _config.MAX_AUTH_REQUESTS = 3;
        Assert.Equal(3, _config.MAX_AUTH_REQUESTS);
    }

    [Fact]
    public void AUTH_TIMEOUT_default()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(120000), _config.AUTH_TIMEOUT);
    }

    [Fact]
    public void AUTH_TIMEOUT_roundtrip()
    {
        _config.AUTH_TIMEOUT = TimeSpan.FromSeconds(30);
        Assert.Equal(TimeSpan.FromSeconds(30), _config.AUTH_TIMEOUT);
    }

    [Fact]
    public void MAX_CONCURRENT_SESSIONS_default()
    {
        Assert.Equal(10, _config.MAX_CONCURRENT_SESSIONS);
    }

    [Fact]
    public void MAX_CONCURRENT_SESSIONS_roundtrip()
    {
        _config.MAX_CONCURRENT_SESSIONS = 5;
        Assert.Equal(5, _config.MAX_CONCURRENT_SESSIONS);
    }

    [Fact]
    public void MAX_CONCURRENT_CHANNELS_default()
    {
        Assert.Equal(10, _config.MAX_CONCURRENT_CHANNELS);
    }

    [Fact]
    public void MAX_CONCURRENT_CHANNELS_roundtrip()
    {
        _config.MAX_CONCURRENT_CHANNELS = 20;
        Assert.Equal(20, _config.MAX_CONCURRENT_CHANNELS);
    }

    [Fact]
    public void NIO_WORKERS_default()
    {
        Assert.Equal(Environment.ProcessorCount * 2, _config.NIO_WORKERS);
    }

    [Fact]
    public void NIO_WORKERS_roundtrip()
    {
        _config.NIO_WORKERS = 8;
        Assert.Equal(8, _config.NIO_WORKERS);
    }

    [Fact]
    public void NIO_WORKERS_zero_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _config.NIO_WORKERS = 0);
    }

    [Fact]
    public void NIO_WORKERS_negative_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _config.NIO_WORKERS = -1);
    }

    [Fact]
    public void SOCKET_BACKLOG_default()
    {
        Assert.Equal(0, _config.SOCKET_BACKLOG);
    }

    [Fact]
    public void SOCKET_BACKLOG_roundtrip()
    {
        _config.SOCKET_BACKLOG = 128;
        Assert.Equal(128, _config.SOCKET_BACKLOG);
    }

    [Fact]
    public void SOCKET_BACKLOG_negative_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _config.SOCKET_BACKLOG = -1);
    }

    [Fact]
    public void SOCKET_KEEPALIVE_default()
    {
        Assert.False(_config.SOCKET_KEEPALIVE);
    }

    [Fact]
    public void SOCKET_KEEPALIVE_roundtrip()
    {
        _config.SOCKET_KEEPALIVE = true;
        Assert.True(_config.SOCKET_KEEPALIVE);
        _config.SOCKET_KEEPALIVE = false;
        Assert.False(_config.SOCKET_KEEPALIVE);
    }

    [Fact]
    public void TCP_NODELAY_default()
    {
        Assert.True(_config.TCP_NODELAY);
    }

    [Fact]
    public void TCP_NODELAY_roundtrip()
    {
        _config.TCP_NODELAY = false;
        Assert.False(_config.TCP_NODELAY);
        _config.TCP_NODELAY = true;
        Assert.True(_config.TCP_NODELAY);
    }

    [Fact]
    public void IDLE_TIMEOUT_default()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(600000), _config.IDLE_TIMEOUT);
    }

    [Fact]
    public void IDLE_TIMEOUT_roundtrip()
    {
        _config.IDLE_TIMEOUT = TimeSpan.FromMinutes(5);
        Assert.Equal(TimeSpan.FromMinutes(5), _config.IDLE_TIMEOUT);
    }

    [Fact]
    public void HEARTBEAT_INTERVAL_default()
    {
        Assert.Equal(TimeSpan.FromSeconds(45), _config.HEARTBEAT_INTERVAL);
    }

    [Fact]
    public void HEARTBEAT_INTERVAL_can_be_disabled()
    {
        _config.HEARTBEAT_INTERVAL = TimeSpan.Zero;
        Assert.Equal(TimeSpan.Zero, _config.HEARTBEAT_INTERVAL);
    }

    [Fact]
    public void HEARTBEAT_INTERVAL_roundtrip()
    {
        _config.HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(30);
        Assert.Equal(TimeSpan.FromSeconds(30), _config.HEARTBEAT_INTERVAL);
    }

    [Fact]
    public void REKEY_BYTES_LIMIT_default()
    {
        Assert.Equal(1024L * 1024L * 1024L, _config.REKEY_BYTES_LIMIT);
    }

    [Fact]
    public void REKEY_BYTES_LIMIT_roundtrip()
    {
        _config.REKEY_BYTES_LIMIT = 512 * 1024 * 1024;
        Assert.Equal(512 * 1024 * 1024, _config.REKEY_BYTES_LIMIT);
    }

    [Fact]
    public void REKEY_TIME_LIMIT_default()
    {
        Assert.Equal(TimeSpan.FromHours(1), _config.REKEY_TIME_LIMIT);
    }

    [Fact]
    public void REKEY_TIME_LIMIT_roundtrip()
    {
        _config.REKEY_TIME_LIMIT = TimeSpan.FromMinutes(30);
        Assert.Equal(TimeSpan.FromMinutes(30), _config.REKEY_TIME_LIMIT);
    }

    [Fact]
    public void WELCOME_BANNER_default_empty()
    {
        Assert.Equal("", _config.WELCOME_BANNER);
    }

    [Fact]
    public void WELCOME_BANNER_roundtrip()
    {
        _config.WELCOME_BANNER = "Welcome to SFTP server";
        Assert.Equal("Welcome to SFTP server", _config.WELCOME_BANNER);
    }

    [Fact]
    public void SERVER_IDENTIFICATION_default_empty()
    {
        Assert.Equal("", _config.SERVER_IDENTIFICATION);
    }

    [Fact]
    public void SERVER_IDENTIFICATION_roundtrip()
    {
        _config.SERVER_IDENTIFICATION = "SSH-2.0-MyServer";
        Assert.Equal("SSH-2.0-MyServer", _config.SERVER_IDENTIFICATION);
    }

    [Fact]
    public void CIPHERS_default_not_empty()
    {
        Assert.NotEmpty(_config.CIPHERS);
    }

    [Fact]
    public void CIPHERS_roundtrip()
    {
        string cipher = AMNetSshAlgorithms.Ciphers.Aes128Ctr;
        _config.SetCiphers(cipher);
        Assert.Equal(cipher, _config.CIPHERS);
    }

    [Fact]
    public void SetCiphers_unsupported_throws()
    {
        Assert.Throws<ArgumentException>(() => _config.SetCiphers("nonexistent-cipher"));
    }

    [Fact]
    public void MACS_default_not_empty()
    {
        Assert.NotEmpty(_config.MACS);
    }

    [Fact]
    public void MACS_roundtrip()
    {
        string mac = AMNetSshAlgorithms.Macs.HmacSha256;
        _config.SetMacs(mac);
        Assert.Equal(mac, _config.MACS);
    }

    [Fact]
    public void SetMacs_unsupported_throws()
    {
        Assert.Throws<ArgumentException>(() => _config.SetMacs("nonexistent-mac"));
    }

    [Fact]
    public void KEX_ALGORITHMS_default_not_empty()
    {
        Assert.NotEmpty(_config.KEX_ALGORITHMS);
    }

    [Fact]
    public void KEX_ALGORITHMS_roundtrip()
    {
        string kex = _config.GetSupportedKeyExchangeAlgorithms()[0];
        _config.SetKeyExchangeAlgorithms(kex);
        Assert.Equal(kex, _config.KEX_ALGORITHMS);
    }

    [Fact]
    public void SetKeyExchangeAlgorithms_unsupported_throws()
    {
        Assert.Throws<ArgumentException>(() => _config.SetKeyExchangeAlgorithms("nonexistent-kex"));
    }

    [Fact]
    public void HOST_KEY_ALGORITHMS_default_not_empty()
    {
        Assert.NotEmpty(_config.HOST_KEY_ALGORITHMS);
    }

    [Fact]
    public void HOST_KEY_ALGORITHMS_roundtrip()
    {
        string hostKey = _config.GetSupportedHostKeyAlgorithms()[0];
        _config.SetHostKeyAlgorithms(hostKey);
        Assert.Equal(hostKey, _config.HOST_KEY_ALGORITHMS);
    }

    [Fact]
    public void SetHostKeyAlgorithms_unsupported_throws()
    {
        Assert.Throws<ArgumentException>(() => _config.SetHostKeyAlgorithms("nonexistent-hostkey"));
    }

    [Fact]
    public void GetSupportedCiphers_not_empty()
    {
        Assert.NotEmpty(_config.GetSupportedCiphers());
    }

    [Fact]
    public void GetSupportedMacs_not_empty()
    {
        Assert.NotEmpty(_config.GetSupportedMacs());
    }

    [Fact]
    public void GetSupportedKeyExchangeAlgorithms_not_empty()
    {
        Assert.NotEmpty(_config.GetSupportedKeyExchangeAlgorithms());
    }

    [Fact]
    public void GetSupportedHostKeyAlgorithms_not_empty()
    {
        Assert.NotEmpty(_config.GetSupportedHostKeyAlgorithms());
    }

    [Fact]
    public void GetConfiguredCiphers_returns_list()
    {
        _config.SetCiphers(AMNetSshAlgorithms.Ciphers.Aes128Ctr, AMNetSshAlgorithms.Ciphers.Aes256Ctr);
        var ciphers = _config.GetConfiguredCiphers();
        Assert.Contains(AMNetSshAlgorithms.Ciphers.Aes128Ctr, ciphers);
        Assert.Contains(AMNetSshAlgorithms.Ciphers.Aes256Ctr, ciphers);
    }

    [Fact]
    public void Multiple_ciphers_are_comma_separated()
    {
        _config.SetCiphers(AMNetSshAlgorithms.Ciphers.Aes128Ctr, AMNetSshAlgorithms.Ciphers.Aes256Ctr);
        Assert.Contains(AMNetSshAlgorithms.Ciphers.Aes128Ctr, _config.CIPHERS);
        Assert.Contains(AMNetSshAlgorithms.Ciphers.Aes256Ctr, _config.CIPHERS);
    }

    [Fact]
    public void ApplyModernAlgorithmDefaults_sets_ciphers()
    {
        _config.ApplyModernAlgorithmDefaults();
        Assert.NotEmpty(_config.CIPHERS);
        Assert.NotEmpty(_config.MACS);
        Assert.NotEmpty(_config.KEX_ALGORITHMS);
        Assert.NotEmpty(_config.HOST_KEY_ALGORITHMS);
    }

    [Fact]
    public void SetCiphers_empty_throws()
    {
        Assert.Throws<ArgumentException>(() => _config.SetCiphers(Array.Empty<string>()));
    }

    [Fact]
    public void SetCiphers_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _config.SetCiphers((IEnumerable<string>)null!));
    }

    [Fact]
    public void SetAuthenticationMethodGroups_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _config.SetAuthenticationMethodGroups(null!));
    }

    [Fact]
    public void SetAuthenticationMethods_duplicate_values()
    {
        _config.SetCiphers(
            AMNetSshAlgorithms.Ciphers.Aes128Ctr,
            AMNetSshAlgorithms.Ciphers.Aes128Ctr,
            AMNetSshAlgorithms.Ciphers.Aes256Ctr);

        var ciphers = _config.GetConfiguredCiphers();
        Assert.Equal(2, ciphers.Count);
    }
}
