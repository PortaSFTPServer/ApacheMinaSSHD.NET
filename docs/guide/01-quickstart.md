# Quick Start: Build Your First .NET SFTP Server

This guide walks you through creating a working SFTP/SCP server with ApacheMinaSSHD.NET in under five minutes.

## Prerequisites

- .NET 10.0 SDK or later
- An existing .NET project (console, web, Windows Forms, or WPF)

## Step 1: Install the Package

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

No additional packages are needed — the Wrapper bundles all IKVM bindings, Apache MINA SSHD, SLF4J, and Bouncy Castle assemblies.

## Step 2: Create the Server

```csharp
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;
```

## Step 3: Configure Host Keys

```csharp
var hostKeys = new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser");
hostKeys.Algorithm = AMNetSshAlgorithms.HostKeyAlgorithms.Rsa;
hostKeys.KeySize = 3072;

server.setKeyPairProvider(hostKeys);
```

The host key is automatically persisted to `hostkey.ser` and reused across restarts. On first run, a new RSA-3072 key pair is generated.

## Step 4: Set Up Authentication

```csharp
// Fixed password authenticator (for testing)
server.SetFixedPasswordAuthenticator("demo", "your-password-here");
```

For production, implement `IAMNetPasswordAuthenticator` against your identity store — see the [Authentication guide](03-authentication.md).

## Step 5: Configure the Filesystem

```csharp
string rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
var fsFactory = new AMNetVirtualFileSystemFactory(rootPath);
server.setFileSystemFactory(fsFactory);
```

Each authenticated user gets a subdirectory under `sftp-root` named after their username.

## Step 6: Enable SFTP

```csharp
var sftp = new AMNetSftpSubsystemFactory();
server.setSubsystemFactories(sftp);
```

## Step 7: Start the Server

```csharp
server.Start();

Console.WriteLine($"SFTP server listening on {server.Host}:{server.Port}");
Console.ReadKey();
```

## Full Example

```csharp
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;

var hostKeys = new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser");
hostKeys.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
hostKeys.setKeySize(3072);
server.setKeyPairProvider(hostKeys);

server.SetFixedPasswordAuthenticator("demo", "your-password-here");

string rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(rootPath));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());

server.Start();
Console.WriteLine("SFTP server running on port 2222");
Console.ReadKey();
server.Stop();
```

## Testing the Connection

```powershell
sftp -P 2222 demo@127.0.0.1
```

Enter your password and try:
```
sftp> pwd
sftp> ls
sftp> put test-file.txt
```

---

**Next:** [Server Configuration](02-configuration.md) — tune timeouts, limits, and crypto algorithms.

---

*ApacheMinaSSHD.NET is developed by **SERALYNX LLC**. For a production-ready portable SFTP server with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
