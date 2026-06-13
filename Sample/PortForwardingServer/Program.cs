// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Renci.SshNet;

// ──────────────────────────────────────────────────────────
// SSH Port Forwarding / Tunneling — Server Example
//
// Demonstrates:
//   1. Setting TCP forwarding policy (All / None / Local / Remote)
//   2. Remote port forwarding (server listens, tunnels to client)
//   3. Local port forwarding (client tunnels through server to target)
//   4. Data flow verification through the tunnel
//
// Run: dotnet run
// ──────────────────────────────────────────────────────────

string rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
string hostKeyPath = Path.Combine(AppContext.BaseDirectory, "hostkey.ser");
Directory.CreateDirectory(rootPath);

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 0; // OS-assigned port
server.Config.ApplyProductionDefaults();
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(hostKeyPath));
server.SetFixedPasswordAuthenticator("demo", "demo");
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(rootPath));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());

// ── Configure forwarding policy ──────────────────────────
// Options: All, None, Local, Remote
//   All    — can listen + can connect (direct & forwarded)
//   None   — no forwarding at all
//   Local  — can listen + direct-tcpip only
//   Remote — can listen + forwarded-tcpip only
server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);

server.Start();
int sshPort = server.Port;
Console.WriteLine($"SSH server listening on 127.0.0.1:{sshPort} (forwarding policy: All)");
Console.WriteLine();

// ── Connect SSH client ───────────────────────────────────
using var client = new SshClient("127.0.0.1", sshPort, "demo", "demo");
client.Connect();
Console.WriteLine("SSH client connected.");

// ── Demo 1: Remote port forwarding ───────────────────────
// Server listens on 127.0.0.1:<free-port> and tunnels
// connections back to the client at 127.0.0.1:9999.
Console.WriteLine();
Console.WriteLine("── Remote Port Forwarding ──");
Console.WriteLine("Server listens; connections tunnel back to client.");

// Start a simple listener on the client side to accept tunneled connections
using var clientListener = new TcpListener(IPAddress.Loopback, 0);
clientListener.Start();
int clientListenPort = ((IPEndPoint)clientListener.LocalEndpoint).Port;

using var remotePort = new ForwardedPortRemote("127.0.0.1", 0u, "127.0.0.1", (uint)clientListenPort);
client.AddForwardedPort(remotePort);
remotePort.Start();
Console.WriteLine($"Remote port active: server:127.0.0.1:{remotePort.BoundPort} -> client:127.0.0.1:{clientListenPort}");

// ── Demo 2: Local port forwarding ────────────────────────
// Client listens on 127.0.0.1:<free-port> and tunnels
// through the server to a target.
Console.WriteLine();
Console.WriteLine("── Local Port Forwarding ──");
Console.WriteLine("Client listens; connections tunnel through server to target.");

// Start an echo service as the "target"
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

using var localPort = new ForwardedPortLocal("127.0.0.1", 0u, "127.0.0.1", (uint)echoPort);
client.AddForwardedPort(localPort);
localPort.Start();
Console.WriteLine($"Local port active: client:127.0.0.1:{localPort.BoundPort} -> server -> target:127.0.0.1:{echoPort}");

// Verify data flow through the tunnel
using var tcpClient = new TcpClient();
tcpClient.Connect(IPAddress.Loopback, (int)localPort.BoundPort);
var stream = tcpClient.GetStream();
byte[] sent = "Hello through the SSH tunnel!"u8.ToArray();
stream.Write(sent, 0, sent.Length);
byte[] received = new byte[sent.Length];
int readLen = stream.Read(received, 0, received.Length);
Console.WriteLine($"Data flow verified: sent {sent.Length} bytes, received {readLen} bytes — match: {sent.SequenceEqual(received)}");

Console.WriteLine();
Console.WriteLine("Press Enter to stop the server and clean up.");
Console.ReadLine();

localPort.Stop();
remotePort.Stop();
client.Disconnect();
echoServer.Stop();
clientListener.Stop();
server.Stop();
