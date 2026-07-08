using System.Net.Sockets;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Collection("SequentialIntegration")]
[Trait("Category", "Integration")]
public sealed class PostQuantumKexIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AMNetSshServer _server;
    private int _port;

    public PostQuantumKexIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PQKexIntTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);

        _server = AMNetSshServer.SetUpDefaultServer();
        _server.Host = "127.0.0.1";
        _server.Port = 0;
        _server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
            Path.Combine(_tempDir, "hostkey.ser")));
        _server.SetFixedPasswordAuthenticator("pqtest", "pqpass");
    }

    public void Dispose()
    {
        try { if (_server.IsStarted()) _server.Stop(true); } catch { }
        _server.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static void WaitForTcpPort(int port, int timeoutMs = 15_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            using var tcp = new TcpClient();
            try
            {
                tcp.Connect("127.0.0.1", port);
                tcp.Close();
                return;
            }
            catch (SocketException)
            {
                Thread.Sleep(200);
            }
        }
        throw new InvalidOperationException($"Server on port {port} did not start within {timeoutMs}ms");
    }

    private static void WaitForSshPort(int port, string user, string pass, int timeoutMs = 15_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            using var probe = new SshClient("127.0.0.1", port, user, pass);
            probe.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
            try
            {
                probe.Connect();
                probe.Disconnect();
                return;
            }
            catch (SocketException) { Thread.Sleep(200); }
            catch (SshOperationTimeoutException) { Thread.Sleep(500); }
            catch (SshAuthenticationException) { Thread.Sleep(500); }
            catch (SshConnectionException) { Thread.Sleep(500); }
        }
        throw new InvalidOperationException($"Server on port {port} did not accept SSH within {timeoutMs}ms");
    }

    [Fact]
    public void Server_advertises_pq_kex()
    {
        _server.Start();
        _port = _server.Port;
        WaitForTcpPort(_port);

        var supported = _server.Config.GetSupportedKeyExchangeAlgorithms();
        Assert.Contains("sntrup761x25519-sha512@openssh.com", supported);
    }

    [Fact]
    public void Client_connects_when_pq_kex_is_available()
    {
        _server.Config.ApplyProductionDefaults();
        _server.Config.SetKeyExchangeAlgorithms(
            AMNetSshAlgorithms.Presets.ModernKeyExchanges.ToArray());
        _server.Start();
        _port = _server.Port;
        WaitForTcpPort(_port);

        using var client = new SshClient("127.0.0.1", _port, "pqtest", "pqpass");
        client.Connect();
        try
        {
            Assert.True(client.IsConnected);
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Client_connects_with_only_pq_kex()
    {
        _server.Config.ApplyProductionDefaults();
        var supported = _server.Config.GetSupportedKeyExchangeAlgorithms();
        if (!supported.Contains("sntrup761x25519-sha512@openssh.com"))
            return;

        _server.Config.SetKeyExchangeAlgorithms("sntrup761x25519-sha512@openssh.com");
        _server.Start();
        _port = _server.Port;
        WaitForSshPort(_port, "pqtest", "pqpass");

        using var client = new SshClient("127.0.0.1", _port, "pqtest", "pqpass");
        client.Connect();
        try
        {
            Assert.True(client.IsConnected);
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_upload_works_with_pq_kex_available()
    {
        var homeDir = Path.Combine(_tempDir, "home");
        Directory.CreateDirectory(homeDir);
        _server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(homeDir));
        _server.setSubsystemFactories(new AMNetSftpSubsystemFactory());

        _server.Config.ApplyProductionDefaults();
        _server.Config.SetKeyExchangeAlgorithms(
            AMNetSshAlgorithms.Presets.ModernKeyExchanges.ToArray());
        _server.Start();
        _port = _server.Port;
        WaitForSshPort(_port, "pqtest", "pqpass");

        using var client = new SftpClient("127.0.0.1", _port, "pqtest", "pqpass");
        client.Connect();
        try
        {
            using var ms = new MemoryStream("PQ KEX SFTP data"u8.ToArray());
            client.UploadFile(ms, "/pq-sftp-test.txt");
            var userDir = Directory.GetDirectories(homeDir).FirstOrDefault();
            Assert.NotNull(userDir);
            var destFile = Directory.GetFiles(userDir, "pq-sftp-test.txt").FirstOrDefault();
            Assert.NotNull(destFile);
            Assert.Equal("PQ KEX SFTP data", File.ReadAllText(destFile));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Multiple_concurrent_sessions_connect_when_pq_kex_available()
    {
        _server.Config.ApplyProductionDefaults();
        _server.Config.SetKeyExchangeAlgorithms(
            AMNetSshAlgorithms.Presets.ModernKeyExchanges.ToArray());
        _server.Start();
        _port = _server.Port;
        WaitForSshPort(_port, "pqtest", "pqpass");

        const int sessionCount = 5;
        var clients = new List<SshClient>(sessionCount);
        try
        {
            for (int i = 0; i < sessionCount; i++)
            {
                var c = new SshClient("127.0.0.1", _port, "pqtest", "pqpass");
                c.Connect();
                Assert.True(c.IsConnected);
                clients.Add(c);
            }
        }
        finally
        {
            foreach (var c in clients)
            {
                try { c.Disconnect(); c.Dispose(); } catch { }
            }
        }
    }
}
