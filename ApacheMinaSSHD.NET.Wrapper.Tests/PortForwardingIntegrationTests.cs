// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Integration")]
public sealed class PortForwardingIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AMNetSshServer _server;
    private int _port;

    public PortForwardingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SshForwardIntTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);

        _server = AMNetSshServer.SetUpDefaultServer();
        _server.Host = "127.0.0.1";
        _server.Port = 0;

        _server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
            Path.Combine(_tempDir, "hostkey.ser")));

        var userHome = Path.Combine(_tempDir, "home");
        Directory.CreateDirectory(userHome);
        _server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(userHome));

        _server.SetFixedPasswordAuthenticator("testuser", "testpass");
    }

    public void Dispose()
    {
        try { if (_server.IsStarted()) _server.Stop(true); } catch { }
        _server.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private void StartServer()
    {
        _server.Start();
        _port = _server.Port;
    }

    [Fact]
    public void Server_starts_with_forwarding_policy_All()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);
        StartServer();
        Assert.True(_server.IsStarted());
        Assert.NotEqual(0, _port);
    }

    [Fact]
    public void Server_starts_with_forwarding_policy_None()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.None);
        StartServer();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_forwarding_policy_Local()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.Local);
        StartServer();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_forwarding_policy_Remote()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.Remote);
        StartServer();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_custom_tcp_forwarding_filter()
    {
        _server.setTcpForwardingFilter(new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.All));
        StartServer();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Server_starts_with_composite_forwarding_filter()
    {
        _server.setForwardingFilter(AMNetForwardingFilter.FromPolicy(AMNetTcpForwardingPolicy.All));
        StartServer();
        Assert.True(_server.IsStarted());
    }

    [Fact]
    public void Ssh_client_can_connect_with_password_auth_and_All_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);
        StartServer();

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        Assert.True(client.IsConnected);
        client.Disconnect();
    }

    [Fact]
    public void Ssh_client_can_connect_with_password_auth_and_None_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.None);
        StartServer();

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        Assert.True(client.IsConnected);
        client.Disconnect();
    }

    [Fact]
    public void ForwardedPortRemote_succeeds_with_All_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);
        StartServer();

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();

        using var port = new ForwardedPortRemote("127.0.0.1", 0u, "127.0.0.1", 2222u);
        client.AddForwardedPort(port);
        port.Start();

        Assert.True(port.IsStarted);
        port.Stop();
        client.Disconnect();
    }

    [Fact]
    public void ForwardedPortRemote_succeeds_with_Remote_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.Remote);
        StartServer();

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();

        using var port = new ForwardedPortRemote("127.0.0.1", 0u, "127.0.0.1", 2222u);
        client.AddForwardedPort(port);
        port.Start();

        Assert.True(port.IsStarted);
        port.Stop();
        client.Disconnect();
    }

    [Fact]
    public void ForwardedPortRemote_fails_with_None_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.None);
        StartServer();

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();

        using var port = new ForwardedPortRemote("127.0.0.1", 0u, "127.0.0.1", 2222u);
        client.AddForwardedPort(port);
        Assert.Throws<SshException>(() => port.Start());
        client.Disconnect();
    }

    [Fact]
    public void ForwardedPortRemote_succeeds_with_Local_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.Local);
        StartServer();

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();

        using var port = new ForwardedPortRemote("127.0.0.1", 0u, "127.0.0.1", 2222u);
        client.AddForwardedPort(port);
        port.Start();

        Assert.True(port.IsStarted);
        port.Stop();
        client.Disconnect();
    }

    [Fact]
    public void ForwardedPortLocal_data_flow_works_with_All_policy()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);
        StartServer();

        using var echoServer = new TcpListener(IPAddress.Loopback, 0);
        echoServer.Start();
        int echoPort = ((IPEndPoint)echoServer.LocalEndpoint).Port;
        var echoTask = Task.Run(async () =>
        {
            using var tcp = await echoServer.AcceptTcpClientAsync();
            var buffer = new byte[4096];
            using (tcp)
            {
                var stream = tcp.GetStream();
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                await stream.WriteAsync(buffer, 0, read);
            }
        });

        using var client = new SshClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();

        using var port = new ForwardedPortLocal("127.0.0.1", 0u, "127.0.0.1", (uint)echoPort);
        client.AddForwardedPort(port);
        port.Start();

        Assert.True(port.IsStarted);
        int localForwardPort = (int)port.BoundPort;

        using var tcp = new TcpClient();
        tcp.Connect(IPAddress.Loopback, localForwardPort);
        var stream = tcp.GetStream();
        byte[] sent = [1, 2, 3, 4];
        stream.Write(sent, 0, sent.Length);
        byte[] received = new byte[4];
        int read = stream.Read(received, 0, received.Length);

        Assert.Equal(sent.Length, read);
        Assert.Equal(sent, received);

        port.Stop();
        client.Disconnect();
        echoServer.Stop();
    }
}
