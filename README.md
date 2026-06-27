# ApacheMinaSSHD.NET — The Best Open Source .NET SFTP Server Library

[![NuGet](https://img.shields.io/nuget/v/ApacheMinaSSHD.NET.Wrapper)](https://www.nuget.org/packages/ApacheMinaSSHD.NET.Wrapper)

> **🔒 FIPS 140-2/140-3 compliant** — Enable FIPS mode via `AMNSecurityUtils.SetFipsMode()` for NIST-approved cryptographic algorithms. Call once at application startup before creating any server instance.

> **⚠️ Security disclaimer:** This is a wrapper library around Apache MINA SSHD — it wraps SFTP and SCP only. This is to support a secure transfer in transit by default. It does not implement FTP, FTPS, TFTP, or any other insecure protocol. Like any wrapper library, security depends on how you use it. You are responsible for configuring authentication, encryption algorithms, access controls, and limits appropriately for your environment. Review the [Security Best Practices guide](docs/guide/07-security.md) before deploying to production.

**ApacheMinaSSHD.NET helps organizations — engineering firms, healthcare providers, and developers — build their own SFTP/SCP servers in .NET.** Use it alongside any SFTP/SCP client to enable secure file transfer and automation for your infrastructure.

## Installation

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

## Table of Contents

- [Installation](#installation)
- [Introduction](#introduction)
- [Why ApacheMinaSSHD.NET?](#why-apacheminasshdnet)
- [Authentication Modules](#authentication-modules)
- [Use Cases](#use-cases)
- [Version Compatibility](#version-compatibility)
- [Sample Projects](#sample-projects)
- [Developer Guide](#developer-guide)
- [Quick Start](#quick-start)
- [Directory Entry Filtering](#directory-entry-filtering)
- [Algorithm Configuration](#algorithm-configuration)
- [Powered by Apache MINA SSHD](#powered-by-apache-mina-sshd)
- [License and Third-Party Notices](#license-and-third-party-notices)
  - [Acknowledgments](#acknowledgments)

## Introduction

ApacheMinaSSHD.NET is the **best open source .NET SFTP server library** — a C# wrapper around Apache MINA SSHD
that lets you embed a full SFTP/SCP server directly into your .NET application. It is **actively maintained**,
**Apache 2.0 licensed**, and the leading **open source alternative to commercial SFTP libraries for .NET developers**.

Whether you are an **engineering firm** sharing CAD files with subcontractors, a **healthcare provider** exchanging
patient data under compliance requirements, or a **developer** building automated file transfer pipelines,
ApacheMinaSSHD.NET gives you the server side of the equation. Pair it with an SFTP/SCP client (e.g. SSH.NET,
WinSCP, Cyberduck, or any OpenSSH client) for a complete secure file transfer solution.

The library exposes the full power of Apache MINA SSHD through a clean .NET API with zero Java dependencies,
following the same architectural approach as **[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)**
— a portable SFTP server for Windows and Linux created by **[SERALYNX LLC](https://portasftpserver.com)**,
a Critical Infrastructure Engineering firm.

![SERALYNX LLC - Porta SFTP Server for Critical Infrastructure](https://raw.githubusercontent.com/PortaSFTPServer/ApacheMinaSSHD.NET/main/docs/images/seralynx-banner.png)

SERALYNX LLC was founded to solve a real problem in critical infrastructure engineering. Civil engineers, construction teams, and IT administrators needed a way to **securely collaborate** on CAD files, 3D models, BIM data, technical drawings, and project documents — without complex VPNs, insecure FTP, or expensive enterprise systems. **ApacheMinaSSHD.NET** brings that same enterprise-grade secure file transfer to .NET developers as an embeddable library, purpose-built for **secure collaboration**, **project management**, and **data exchange**. Build your server with this library; connect to it with any SFTP/SCP client.

## Why ApacheMinaSSHD.NET?

ApacheMinaSSHD.NET exists because building an SSH server from scratch is neither practical nor safe. The SSH protocol suite (RFC 4251–4254) involves dozens of cryptographic handshake variants, channel multiplexing, port forwarding, agent forwarding, and a dozen subsystem protocols — each a potential vulnerability surface. Even mature libraries have had CVEs; a from-scratch implementation would inevitably introduce more. Rather than reimplementing a decade of protocol engineering, ApacheMinaSSHD.NET wraps **Apache MINA SSHD** — the production-grade SSH library from the Apache Software Foundation. Every SSH handshake, cryptographic operation, and protocol message is handled by Apache MINA SSHD, the same engine used by enterprise Java applications worldwide. This means you get battle-tested SSH protocol handling without the burden of writing or maintaining SSH protocol code.

**ApacheMinaSSHD.NET is the only open source library that provides a full SSH/SFTP/SCP *server* for .NET.** The .NET ecosystem has several SSH *client* libraries (SSH.NET being the most popular), but none implement the server side. Other open source options like KeenSystemsNL's SFTPServer are minimal SFTP subsystem handlers that require an external SSH daemon.

For developers who need an **embeddable SFTP server** in their C# application, the alternatives are all commercial products. ApacheMinaSSHD.NET delivers the same enterprise-grade server capability — powered by Apache MINA SSHD — as **free, open source, Apache 2.0-licensed software**.

## Authentication Modules

The public authentication API remains .NET-only. Developers can implement the
interfaces directly or compose the built-in modules:

- `AMNetFixedPasswordAuthenticator` for a single username/password pair.
- `AMNetDelegatePasswordAuthenticator` for application callbacks or identity stores.
- `AMNetCompositePasswordAuthenticator` to try multiple password modules in order.
- `AMNetFingerprintPublickeyAuthenticator` for database or configuration-backed key fingerprints.
- `AMNetPublickeyAuthenticator` for the legacy `Authorized_Keys` directory pattern.
- `AMNetAuthorizedKeysAuthenticator` for OpenSSH-style `authorized_keys` files.
- `AMNetDelegatePublickeyAuthenticator` and `AMNetCompositePublickeyAuthenticator` for custom key policies.
- `AMNetDelegateKeyboardInteractiveAuthenticator` and `AMNetFixedKeyboardInteractiveAuthenticator` for keyboard-interactive prompts.
- `IAMNetHostBasedAuthenticator` / `AMNetDelegateHostBasedAuthenticator` for host-based (host key + hostname) authentication.
- `IAMNetGssapiAuthenticator` / `AMNetDelegateGssapiAuthenticator` for Kerberos/SSO authentication.

Keyboard-interactive authentication is also modular, but it is challenge-based
rather than a single credential check. For advanced routing, use one delegate
authenticator and dispatch to your own per-user or per-tenant modules inside
that callback.

`AMNetPasswordAuthenticator` and `AMNetKeyboardInteractiveAuthenticator` deny access by
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

ApacheMinaSSHD.NET provides the **server** side of secure file transfer.
Pair it with any SFTP/SCP **client** (SSH.NET, WinSCP, Cyberduck, OpenSSH)
to build complete workflows:

- **Engineering firm file exchange** — Build a server for subcontractors and partners to upload/download
  CAD files, 3D models, BIM data, and contract documents with root jail isolation.
  Connect using any SFTP client for secure collaboration.
- **Healthcare data transfer** — Deploy an embedded SFTP server in your .NET application for
  exchanging patient data under compliance requirements. Clients connect via their preferred
  SFTP tooling.
- **Automated file transfer pipelines** — Build a server that receives reports, logs, or data feeds
  from automated clients. Use SSH.NET or scripting to push/pull files on a schedule.
- **Portable SFTP server deployment** — For IT administrators who need a
  zero-install, USB-drive-friendly SFTP server that any SFTP client can connect to.
- **Secure collaboration** — Give your team members and external partners a dedicated
  SFTP server for project files, accessible from any OpenSSH-compatible client.
- **Custom authentication and access control** — Password, public key, `authorized_keys`,
  host-based, Kerberos/GSSAPI, keyboard-interactive, virtual file systems, root jail,
  hidden-file filtering, SFTP/SCP event hooks, audit, and SSH algorithm configuration.

## Version Compatibility

| Component | Version |
|-----------|---------|
| .NET | 9.0, 10.0 |
| Bundled Apache MINA SSHD | 2.18.0 |
| IKVM | 8.15.0+ |

## Sample Projects

Browse ready-to-run C# SFTP server examples that demonstrate real scenarios:

| Sample | What it shows |
|--------|--------------|
| [AuthenticationServer](Sample/AuthenticationServer) | Password, public key, fingerprint, authorized_keys, host-based, GSSAPI/Kerberos, keyboard-interactive, and MFA auth — pick via CLI arg |
| [AvaloniaSftpServer](Sample/AvaloniaSftpServer) | Cross-platform Avalonia UI SFTP server manager — start/stop, sessions, live log |
| [BlazorSftpServer](Sample/BlazorSftpServer) | Blazor Server web app — manage SFTP server from any browser, sessions, live log |
| [ConsoleSftpServer](Sample/ConsoleSftpServer) | Lightweight console-based SFTP server |
| [DareSftpServer](Sample/DareSftpServer) | Data-at-rest encryption with AES-256-GCM chunked encryption |
| [MinimalServer](Sample/MinimalServer) | Minimal C# SFTP server — simplest way to embed an SFTP server in .NET (~15 lines) |
| [PhotinoSftpServer](Sample/PhotinoSftpServer) | Cross-platform Photino Blazor desktop app — Blazor UI, start/stop, sessions, live log |
| [PortForwardingServer](Sample/PortForwardingServer) | SSH port forwarding / tunneling — TCP forwarding policy, remote and local port forwarding, data flow through tunnels via SSH.NET |
| [ProductionServer](Sample/ProductionServer) | Production-ready: external JSON config, algorithm selection, combined listeners |
| [ScpServer](Sample/ScpServer) | SCP server with custom file opener, transfer audit, and directory filtering |
| [SessionMonitorServer](Sample/SessionMonitorServer) | Session lifecycle, connection monitoring, proxy metadata inspection |
| [SFTPServerWithNuget](Sample/SFTPServerWithNuget) | Minimal SFTP server consuming the published NuGet package — validates end-to-end package reference |
| [SftpEventServer](Sample/SftpEventServer) | SFTP event listeners — track open/close/read/write/create/remove/move operations |
| [SimpleSSHDServer](Sample/SimpleSSHDServer) | Windows Forms GUI server with FIPS mode and integration test harness |
| [VirtualFileSystemServer](Sample/VirtualFileSystemServer) | Root jail isolation, hidden-file filtering, path access control for SFTP and SCP |
| [SftpClientServer](Sample/SftpClientServer) | SFTP/SCP client example — connects to the server via SSH.NET to upload, download, and verify files over SFTP and SCP |

All samples target .NET 10 and reference the wrapper NuGet package directly.

## Developer Guide

For complete, organized documentation on building SFTP/SCP servers with ApacheMinaSSHD.NET, see the [Developer Guide](docs/guide/README.md):

- [Quick Start](docs/guide/01-quickstart.md) — Build your first server in 5 minutes
- [Server Configuration](docs/guide/02-configuration.md) — Timeouts, limits, algorithms
- [Authentication](docs/guide/03-authentication.md) — Password, public key, host-based, GSSAPI/Kerberos, MFA
- [Virtual Filesystem](docs/guide/04-virtual-filesystem.md) — Root jail, path containment
- [SFTP Subsystem](docs/guide/05-sftp-subsystem.md) — File operations, event hooks
- [SCP Subsystem](docs/guide/06-scp-subsystem.md) — Secure copy, transfer events
- [Security Best Practices](docs/guide/07-security.md) — Hardening, symlink containment
- [Logging & Monitoring](docs/guide/08-logging.md) — SLF4J bridge, audit events
- [Production Deployment](docs/guide/09-production-deployment.md) — Windows Service, Docker, CI/CD
- [PROXY Protocol](docs/guide/10-proxy-protocol.md) — Load balancers, real client IP, PROXY v1/v2

## Quick Start

The sample classes `MyFileAccessor` and `MyScpOpener` used below are defined in the
[Directory Entry Filtering](#directory-entry-filtering) section.

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
    new MyScpOpener(rootPath)));

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
        var name = Path.GetFileName(context.RemotePath);
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
    public MyScpOpener(string rootPath) : base(rootPath) { }

    public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
    {
        var name = Path.GetFileName(access.LocalPath);
        if (name != null && name.StartsWith(".")) return false;
        return base.ShouldIncludeDirectoryEntry(access);
    }
}
```

## Algorithm Configuration

Developers can inspect and configure allowed algorithms
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

## Powered by Apache MINA SSHD

[![Apache MINA SSHD](https://raw.githubusercontent.com/PortaSFTPServer/ApacheMinaSSHD.NET/main/docs/images/apache-mina-sshd.png)](https://mina.apache.org/sshd-project/)

ApacheMinaSSHD.NET is a **.NET wrapper** around **[Apache MINA SSHD](https://mina.apache.org/sshd-project/)**, the
production-grade SSH/SFTP/SCP server library maintained by the **[Apache Software Foundation](https://www.apache.org/)**.
Apache MINA SSHD is the backbone of this project — all SSH protocol handling, cryptographic operations, session
management, and file transfer logic come from the Apache MINA SSHD project.

We are grateful to the Apache Software Foundation and the Apache MINA SSHD community for creating and maintaining
this excellent library. This project would not exist without their work.

Apache MINA SSHD is a trademark of the Apache Software Foundation. ApacheMinaSSHD.NET is **not** affiliated with
or endorsed by the Apache Software Foundation.

## License and Third-Party Notices

Project code is Apache 2.0 licensed. See `LICENSE`, `THIRD-PARTY-NOTICES.md`, and the
`licenses/` folder before publishing source or NuGet packages.

### Acknowledgments

ApacheMinaSSHD.NET is made possible by these exceptional open source projects:

| [![Bouncy Castle](https://raw.githubusercontent.com/PortaSFTPServer/ApacheMinaSSHD.NET/main/docs/images/bouncycastle-logo.svg)](https://www.bouncycastle.org/) | [![SLF4J](https://raw.githubusercontent.com/PortaSFTPServer/ApacheMinaSSHD.NET/main/docs/images/slf4j-logo.png)](https://www.slf4j.org/) |
|---|---|
| Cryptography library (MIT / Bouncy Castle) | Logging facade (MIT) |

- **[Apache MINA SSHD](https://mina.apache.org/sshd-project/)** — SSH/SFTP/SCP protocol engine (Apache License 2.0)
- **[IKVM](https://ikvm.org/)** — Java-to-.NET bridge

ApacheMinaSSHD.NET is maintained by SERALYNX LLC. For a ready-to-use portable SFTP server, see [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).

> **Disclaimer:** Porta SFTP Server Business Edition / Enterprise Edition do not use this library or the Apache Mina SSHD Server Java library. Only the Free and Pro Edition uses the Apache Mina SSHD Server Library. SERALYNX LLC developed its own SFTP/SCP Server Protocols in addition to their HACLEX™ (High-Acceleration & Cryptographic Layer Exchange) protocol.
