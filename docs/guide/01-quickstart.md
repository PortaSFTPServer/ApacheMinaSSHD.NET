# Quick Start: Build Your First .NET SFTP Server

This guide walks you through creating a working SFTP/SCP server with ApacheMinaSSHD.NET — the best open source .NET SFTP server library — in under five minutes.

## Prerequisites

- .NET 10.0 SDK or later
- An existing .NET project (console, web, Windows Forms, or WPF)

## Step 1: Install the Package

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

No additional packages are needed — the Wrapper bundles all IKVM bindings, Apache MINA SSHD 2.18.0, SLF4J, and Bouncy Castle assemblies. Compatible with .NET 9.0 and 10.0.

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
hostKeys.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
hostKeys.setKeySize(3072);

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
```text
sftp> pwd
sftp> ls
sftp> put test-file.txt
```

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| Connection refused | Port already in use or firewall | Verify port is free with `netstat -ano`, check firewall rules |
| Host key regenerates on every restart | Key file directory not writable | Ensure `hostkey.ser` directory has write permissions |
| "Authentication failed" | Authenticator not configured or wrong credentials | Call `SetFixedPasswordAuthenticator` or a custom authenticator before `Start()` |

## Next Steps

Your server works — now harden it for real use:

1. **[Configure production limits](02-configuration.md)** — apply `ApplyProductionDefaults()` and tune timeouts, limits, and algorithms
2. **[Add proper authentication](03-authentication.md)** — replace the hardcoded password with delegate or custom authenticators against your identity store
3. **[Set up a virtual filesystem](04-virtual-filesystem.md)** — enable root jail isolation so users cannot escape their home directories
4. **[Enable audit logging](08-logging.md)** — attach SFTP event listeners to track file operations

See [MinimalServer](../../Sample/MinimalServer/) for a complete, runnable version of this example.

---

**Next:** [Server Configuration](02-configuration.md) — tune timeouts, limits, and crypto algorithms.

---

*ApacheMinaSSHD.NET is developed by **SERALYNX LLC**. For a production-ready portable SFTP server with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
