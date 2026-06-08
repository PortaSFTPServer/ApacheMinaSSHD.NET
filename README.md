# ApacheMinaSSHD.NET — Open Source .NET SFTP Server Library from SERALYNX LLC

[![NuGet](https://img.shields.io/nuget/v/ApacheMinaSSHD.NET.Wrapper)](https://www.nuget.org/packages/ApacheMinaSSHD.NET.Wrapper/)

> **⚠️ Security disclaimer:** This is a wrapper library around Apache MINA SSHD — it wraps SFTP and SCP only. This is to support a secure transfer in transit by default. It does not implement FTP, FTPS, TFTP, or any other insecure protocol. Like any wrapper library, security depends on how you use it. You are responsible for configuring authentication, encryption algorithms, access controls, and limits appropriately for your environment. Review the [Security Best Practices guide](docs/guide/07-security.md) before deploying to production.

**[SERALYNX LLC](https://portasftpserver.com)** is a Critical Infrastructure Engineering firm
supporting Physical and Technological Advancement. One of its products is
**[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)** —
the best portable SFTP server for Windows and Linux, free SFTP server, and instant SFTP server
for critical infrastructure. Porta SFTP Server serves **civil engineering, construction,
IT administrators, and developers** worldwide.

ApacheMinaSSHD.NET is a **free and open source .NET SFTP server library** — a C# wrapper around Apache MINA SSHD,
created by SERALYNX LLC. It follows the same architectural paradigm as Porta SFTP Server —
exposing the full power of Apache MINA SSHD through a clean .NET API, zero Java dependencies. Whether you need
an **open source SFTP library for C#**, a **free SFTP server library for .NET**, or an **embeddable SFTP server**,
ApacheMinaSSHD.NET delivers enterprise-grade secure file transfer.

Whether you need secure file transfer for engineering file exchange, a portable SFTP server for
Windows and Linux, or a free SFTP server for automation workflows, ApacheMinaSSHD.NET and
Porta SFTP Server deliver enterprise-grade file transfer for critical infrastructure.

![SERALYNX LLC - Porta SFTP Server for Critical Infrastructure](https://raw.githubusercontent.com/PortaSFTPServer/ApacheMinaSSHD.NET/main/docs/images/seralynx-banner.png)

## Authentication Modules

The public authentication API stays .NET-only. Developers can implement the
interfaces directly or compose the built-in modules:

- `AMNetFixedPasswordAuthenticator` for a single username/password pair.
- `AMNetDelegatePasswordAuthenticator` for application callbacks or identity stores.
- `AMNetCompositePasswordAuthenticator` to try multiple password modules in order.
- `AMNetFingerprintPublickeyAuthenticator` for database or configuration-backed key fingerprints.
- `AMNetPublickeyAuthenticator` for the legacy `Authorized_Keys` directory pattern.
- `AMNetAuthorizedKeysAuthenticator` for OpenSSH-style `authorized_keys` files.
- `AMNetDelegatePublickeyAuthenticator` and `AMNetCompositePublickeyAuthenticator` for custom key policies.
- `AMNetDelegateKeyboardInteractiveAuthenticator` and `AMNetFixedKeyboardInteractiveAuthenticator` for keyboard-interactive prompts.

Keyboard-interactive authentication is also modular, but it is challenge-based
rather than a single credential check. For advanced routing, use one delegate
authenticator and dispatch to your own per-user or per-tenant modules inside
that callback.

`AMNetPasswordAuthenticator` and `AMNetKeyboardInteractiveAuthenticator` deny by
default. Override them or use the delegate/fixed implementations when enabling
those authentication methods.

Multi-step authentication policy can also be expressed without raw SSH strings:

```csharp
server.SetAuthorizedKeysAuthenticator("authorized_keys");
server.SetCompositePasswordAuthenticator(
    new AMNetDelegatePasswordAuthenticator((username, password, session) => false),
    new AMNetFixedPasswordAuthenticator("fallback", "<your-password>"));

server.SetAuthenticationMethods(
    AMNetSshAuthenticationMethods.PublicKey,
    AMNetSshAuthenticationMethods.RequireAll(
        AMNetSshAuthenticationMethods.Password,
        AMNetSshAuthenticationMethods.KeyboardInteractive));
```

## Use Cases

Use ApacheMinaSSHD.NET when you need:

- An open source **.NET SFTP server library** for C# applications — embed secure file transfer
  without Java dependencies.
- **Secure file transfer** for automated SCP server workflows.
- **Free SFTP server** features from Apache MINA SSHD through .NET and IKVM.
- **Portable SFTP server deployment** — for IT administrators who need a
  zero-install, USB-drive-friendly SFTP server.
- **File exchange for engineering firms** — share CAD files, 3D models,
  BIM data, and contract documents with root jail isolation.
- **Custom password authentication, public key authentication,** `authorized_keys`
  handling, virtual file systems, root jail behavior, hidden-file filtering,
  SFTP hooks, SCP hooks, audit events, and SSH algorithm configuration.

## Version Compatibility

| Component | Version |
|-----------|---------|
| .NET | 6.0, 8.0, 9.0, 10.0 |
| Bundled Apache MINA SSHD | 2.18.0 |
| IKVM | 8.12.0+ |

## Sample Projects

Browse ready-to-run C# SFTP server examples that demonstrate real scenarios:

| Sample | What it shows |
|--------|--------------|
| [MinimalServer](Sample/MinimalServer) | Minimal C# SFTP server — simplest way to embed an SFTP server in .NET (~15 lines) |
| [AuthenticationServer](Sample/AuthenticationServer) | Password, public key, fingerprint, authorized_keys, keyboard-interactive, and MFA auth — pick via CLI arg |
| [AvaloniaSftpServer](Sample/AvaloniaSftpServer) | Cross-platform Avalonia UI SFTP server manager — start/stop, sessions, live log |
| [ConsoleSftpServer](Sample/ConsoleSftpServer) | Lightweight console-based SFTP server |
| [VirtualFileSystemServer](Sample/VirtualFileSystemServer) | Root jail isolation, hidden-file filtering, path access control for SFTP and SCP |
| [ScpServer](Sample/ScpServer) | SCP server with custom file opener, transfer audit, and directory filtering |
| [SftpEventServer](Sample/SftpEventServer) | SFTP event listeners — track open/close/read/write/create/remove/move operations |
| [SessionMonitorServer](Sample/SessionMonitorServer) | Session lifecycle, connection monitoring, proxy metadata inspection |
| [DareSftpServer](Sample/DareSftpServer) | Data-at-rest encryption with AES-256-GCM chunked encryption |
| [PhotinoSftpServer](Sample/PhotinoSftpServer) | Cross-platform Photino Blazor desktop app — Blazor UI, start/stop, sessions, live log |
| [ProductionServer](Sample/ProductionServer) | Production-ready: external JSON config, algorithm selection, combined listeners |
| [SimpleSSHDSever](Sample/SimpleSSHDSever) | Windows Forms GUI server with integration test harness |

All samples target .NET 10 and reference the wrapper NuGet package directly.

## Developer Guide

For complete, organized documentation on building SFTP/SCP servers with ApacheMinaSSHD.NET, see the [Developer Guide](docs/guide/README.md):

- [Quick Start](docs/guide/01-quickstart.md) — Build your first server in 5 minutes
- [Server Configuration](docs/guide/02-configuration.md) — Timeouts, limits, algorithms
- [Authentication](docs/guide/03-authentication.md) — Password, public key, MFA
- [Virtual Filesystem](docs/guide/04-virtual-filesystem.md) — Root jail, path containment
- [SFTP Subsystem](docs/guide/05-sftp-subsystem.md) — File operations, event hooks
- [SCP Subsystem](docs/guide/06-scp-subsystem.md) — Secure copy, transfer events
- [Security Best Practices](docs/guide/07-security.md) — Hardening, symlink containment
- [Logging & Monitoring](docs/guide/08-logging.md) — SLF4J bridge, audit events
- [Production Deployment](docs/guide/09-production-deployment.md) — Windows Service, Docker, CI/CD

## Quick Start

```csharp
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;

var server = AMNetSshServer.SetUpDefaultServer();
server.setHost("127.0.0.1");
server.setPort(2222);

server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();

var hostKeys = new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser");
hostKeys.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
hostKeys.setKeySize(3072);
server.setKeyPairProvider(hostKeys);

var rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");

server.setPasswordAuthenticator(new AMNetFixedPasswordAuthenticator("admin", "changeme"));
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(rootPath));

var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new MyFileAccessor());
server.setSubsystemFactories(sftp);

server.setCommandFactory(new AMNetScpCommandFactory(
    new MyScpFileOpener(rootPath)));

server.start();
```

## Directory Entry Filtering

Control which files and directories are visible over SFTP/SCP by overriding
`ShouldIncludeDirectoryEntry` on the file system accessor:

```csharp
class MyFileAccessor : AMNetSftpFileSystemAccessor
{
    public List<string> HiddenExtensions { get; set; } = [".log", ".tmp"];
    public List<string> HiddenNames { get; set; } = ["secret_data"];
    public bool HideDotFiles { get; set; } = true;

    public override bool ShouldIncludeDirectoryEntry(ISshFileSystemAccess context)
    {
        var name = Path.GetFileName(context.Path);
        if (string.IsNullOrWhiteSpace(name)) return true;

        if (HideDotFiles && name.StartsWith(".")) return false;
        if (HiddenNames.Contains(name, StringComparer.OrdinalIgnoreCase)) return false;

        var ext = Path.GetExtension(name);
        if (HiddenExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) return false;

        return base.ShouldIncludeDirectoryEntry(context);
    }
}

// Same for SCP:
class MyScpOpener : AMNetScpFileOpener
{
    public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
    {
        var name = Path.GetFileName(access.Path);
        if (name != null && name.StartsWith(".")) return false;
        return base.ShouldIncludeDirectoryEntry(access);
    }
}
```

## Algorithm Configuration

Developers can inspect supported algorithms and configure allowed algorithms
without Java imports:

```csharp
IReadOnlyList<string> ciphers = server.Config.GetSupportedCiphers();
IReadOnlyList<string> macs = server.Config.GetSupportedMacs();
IReadOnlyList<string> kex = server.Config.GetSupportedKeyExchangeAlgorithms();
IReadOnlyList<string> hostKeys = server.Config.GetSupportedHostKeyAlgorithms();

server.Config.SetCiphers(
    AMNetSshAlgorithms.Ciphers.Aes256Ctr,
    AMNetSshAlgorithms.Ciphers.Aes128Ctr);

server.Config.SetMacs(
    AMNetSshAlgorithms.Macs.HmacSha512,
    AMNetSshAlgorithms.Macs.HmacSha256);

server.Config.SetKeyExchangeAlgorithms(
    AMNetSshAlgorithms.KeyExchange.Curve25519Sha256,
    AMNetSshAlgorithms.KeyExchange.EcdhNistp256);

server.Config.SetHostKeyAlgorithms(
    AMNetSshAlgorithms.HostKeys.RsaSha512,
    AMNetSshAlgorithms.HostKeys.RsaSha256);
```

`ApplyModernAlgorithmDefaults()` applies a modern preference order filtered to
algorithms supported by the current runtime. The raw string properties remain
available for advanced scenarios.

## License and Third-Party Notices

Project code is MIT licensed. See `LICENSE`, `THIRD-PARTY-NOTICES.md`, and the
`licenses/` folder before publishing source or NuGet packages.

[![Apache MINA SSHD](https://mina.apache.org/assets/img/header-sshd.png)](https://mina.apache.org/sshd-project/) ApacheMinaSSHD.NET is a **wrapper** around **[Apache MINA SSHD](https://mina.apache.org/sshd-project/)**, a trademark of the **Apache Software Foundation**. This project is not affiliated with or endorsed by the Apache Software Foundation.

ApacheMinaSSHD.NET is maintained by SERALYNX LLC. For a ready-to-use portable SFTP server, see [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).
