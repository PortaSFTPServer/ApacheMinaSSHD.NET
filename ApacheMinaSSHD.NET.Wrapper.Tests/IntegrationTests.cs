using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class IntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AMNetSshServer _server;
    private readonly int _port;

    public IntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SshIntegrationTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);

        _server = AMNetSshServer.SetUpDefaultServer();
        _server.Host = "127.0.0.1";
        _server.Port = 0;
        _server.Config.ApplyProductionDefaults();
        _server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
            Path.Combine(_tempDir, "hostkey.ser")));

        var userHome = Path.Combine(_tempDir, "home");
        Directory.CreateDirectory(userHome);
        _server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(userHome));

        _port = _server.Port;
    }

    public void Dispose()
    {
        try
        {
            if (_server.IsStarted())
                _server.Stop(true);
        }
        catch { }
        _server.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void Server_starts_and_stops()
    {
        _server.Start();
        Assert.True(_server.IsStarted());
        Assert.False(_server.IsClosed());

        _server.Stop();
        Assert.True(_server.IsClosed());
    }

    [Fact]
    public void Server_starts_with_password_auth()
    {
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_publickey_auth()
    {
        _server.setFingerprintPublickeyAuthenticator("testuser", "SHA256:abc123");
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_sftp_subsystem()
    {
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.setSubsystemFactories(new AMNetSftpSubsystemFactory());
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_scp_command()
    {
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.setCommandFactory(new AMNetScpCommandFactory());
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_full_configuration()
    {
        _server.Config.ApplyModernAlgorithmDefaults();
        _server.Config.WELCOME_BANNER = "Integration Test Server";
        _server.Config.AUTH_METHODS = "publickey,password";
        _server.Config.MAX_AUTH_REQUESTS = 3;
        _server.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(30);
        _server.Config.IDLE_TIMEOUT = TimeSpan.FromMinutes(5);
        _server.Config.HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(30);
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.setSubsystemFactories(new AMNetSftpSubsystemFactory());
        _server.setCommandFactory(new AMNetScpCommandFactory());
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_multiple_start_stop_cycles()
    {
        for (int i = 0; i < 3; i++)
        {
            using var server = AMNetSshServer.SetUpDefaultServer();
            server.Host = "127.0.0.1";
            server.Port = 0;
            server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
                Path.Combine(_tempDir, $"hostkey_{i}.ser")));
            server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(
                Path.Combine(_tempDir, "home")));
            server.Start();
            Assert.True(server.IsStarted());
            server.Stop(true);
            Assert.True(server.IsClosed());
        }
    }

    [Fact]
    public void Server_stop_before_start_does_not_throw()
    {
        _server.Stop();
    }

    [Fact]
    public void Server_dispose_stops_if_running()
    {
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.Start();
        Assert.True(_server.IsStarted());
        _server.Dispose();
    }

    [Fact]
    public void Server_config_roundtrip_all_properties()
    {
        _server.Config.ApplyProductionDefaults();
        _server.Config.ApplyModernAlgorithmDefaults();

        Assert.Equal(5, _server.Config.MAX_AUTH_REQUESTS);
        Assert.Equal(TimeSpan.FromSeconds(60), _server.Config.AUTH_TIMEOUT);
        Assert.Equal(10, _server.Config.MAX_CONCURRENT_SESSIONS);
        Assert.Equal(10, _server.Config.MAX_CONCURRENT_CHANNELS);
        Assert.Equal(TimeSpan.FromMinutes(10), _server.Config.IDLE_TIMEOUT);
        Assert.Equal(TimeSpan.FromSeconds(45), _server.Config.HEARTBEAT_INTERVAL);
        Assert.NotEmpty(_server.Config.CIPHERS);
        Assert.NotEmpty(_server.Config.MACS);
        Assert.NotEmpty(_server.Config.KEX_ALGORITHMS);
        Assert.NotEmpty(_server.Config.HOST_KEY_ALGORITHMS);
    }

    [Fact]
    public void Server_auth_methods_config()
    {
        _server.Config.SetAuthenticationMethods(
            AMNetSshAuthenticationMethods.PublicKey,
            AMNetSshAuthenticationMethods.Password);

        var methods = _server.GetConfiguredAuthenticationMethods();
        Assert.Equal(2, methods.Count);
        Assert.Equal(["publickey"], methods[0]);
        Assert.Equal(["password"], methods[1]);
    }

    [Fact]
    public void Server_host_and_port_persist()
    {
        _server.Host = "127.0.0.1";
        _server.Port = 2222;
        Assert.Equal("127.0.0.1", _server.Host);
        Assert.Equal(2222, _server.Port);
    }

    [Fact]
    public void Server_with_proxy_acceptor()
    {
        _server.setServerProxyAcceptor(new AMNetServerProxyAcceptor());
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_with_session_listener()
    {
        _server.addSessionListener(new AMNetSessionListener());
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_with_io_service_listener()
    {
        _server.setIoServiceEventListener(new AMNetIoServiceEventListener());
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_with_custom_filesystem_accessor()
    {
        var sftpFactory = new AMNetSftpSubsystemFactory();
        sftpFactory.setFileSystemAccessor(new CustomSftpAccessor());
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
        _server.setSubsystemFactories(sftpFactory);
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_composite_password_auth()
    {
        _server.setCompositePasswordAuthenticator(
            new AMNetFixedPasswordAuthenticator("admin", "adminpass"),
            new AMNetFixedPasswordAuthenticator("user", "userpass"));
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_composite_publickey_auth()
    {
        _server.setCompositePublickeyAuthenticator(
            new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:key1"),
            new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:key2"));
        _server.Start();
        Assert.True(_server.IsStarted());
    }

    private sealed class CustomSftpAccessor : AMNetSftpFileSystemAccessor
    {
        public override string ResolveLocalFilePath(ISshFileSystemAccess context, string resolvedLocalPath)
        {
            return resolvedLocalPath;
        }
    }
}
