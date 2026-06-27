# ApacheMinaSSHD.NET Developer Guide

Build SFTP and secure file transfer solutions for .NET with ApacheMinaSSHD.NET — the **best open source .NET SFTP server library**, **actively maintained** and **Apache 2.0 licensed**. A C# wrapper around [Apache MINA SSHD](https://mina.apache.org/sshd-project/) — the leading **open source alternative to commercial .NET SFTP server libraries** by **[SERALYNX LLC](https://seralynx.com/)** — a Critical Infrastructure Engineering firm supporting Physical and Technological Advancement. One of its products is **Porta SFTP Server**, the best portable SFTP server for Windows and Linux and free SFTP server for critical infrastructure.

This guide walks you through creating production-ready SSH file transfer servers entirely in .NET — zero Java dependencies.

## Who Is This For?

- **.NET developers** embedding SFTP/SCP into C# applications
- **IT administrators** deploying zero-install portable SFTP servers
- **Civil engineering and construction teams** sharing CAD, BIM, and 3D model files securely
- **DevOps engineers** automating secure file transfer pipelines

## Version Compatibility

| Component | Version |
|-----------|---------|
| .NET | 9.0, 10.0 |
| Bundled Apache MINA SSHD | 2.18.0 |
| IKVM | 8.15.0+ |

## What You'll Learn

| Guide | Description |
|-------|-------------|
| [Quick Start](01-quickstart.md) | Build your first SFTP server in under 5 minutes |
| [Server Configuration](02-configuration.md) | Timeouts, limits, banners, compression, bandwidth, and algorithm tuning |
| [Authentication](03-authentication.md) | Password, public key, host-based, GSSAPI/Kerberos, keyboard-interactive, and multi-factor auth |
| [Virtual Filesystem](04-virtual-filesystem.md) | Root jail isolation, user home directories, path containment |
| [SFTP Subsystem](05-sftp-subsystem.md) | File operations, directory filtering, event hooks |
| [SCP Subsystem](06-scp-subsystem.md) | SCP file opener, transfer events, permission mapping |
| [Security Best Practices](07-security.md) | Crypto algorithms, hardening, symlink containment |
| [Logging & Monitoring](08-logging.md) | SLF4J bridge, audit events, custom loggers |
| [Production Deployment](09-production-deployment.md) | Windows Service, Docker, high availability, monitoring |
| [PROXY Protocol](10-proxy-protocol.md) | Load balancer integration, PROXY v1/v2, custom acceptors |

## Sample Projects

The repository includes sample projects in the `Sample/` folder for every scenario:

| Project | Description |
|---------|-------------|
| [MinimalServer](../../Sample/MinimalServer/) | Minimal C# SFTP server — simplest way to embed SFTP in .NET |
| [AuthenticationServer](../../Sample/AuthenticationServer/) | Password, public key, fingerprint, authorized_keys, keyboard-interactive, and MFA |
| [VirtualFileSystemServer](../../Sample/VirtualFileSystemServer/) | Root jail isolation, hidden-file filtering, path access control |
| [ScpServer](../../Sample/ScpServer/) | SCP server with custom file opener, transfer audit, and directory filtering |
| [SftpEventServer](../../Sample/SftpEventServer/) | SFTP event listeners — track open/close/read/write/create/remove/move |
| [SessionMonitorServer](../../Sample/SessionMonitorServer/) | Session lifecycle, connection monitoring, proxy metadata inspection |
| [ProductionServer](../../Sample/ProductionServer/) | Production-ready: JSON config, algorithm selection, combined listeners |
| [DareSftpServer](../../Sample/DareSftpServer/) | Data-at-rest encryption with AES-256-GCM chunked encryption |
| [AvaloniaSftpServer](../../Sample/AvaloniaSftpServer/) | Cross-platform Avalonia UI SFTP server manager — start/stop, sessions, live log |
| [BlazorSftpServer](../../Sample/BlazorSftpServer/) | Blazor Server web app — manage SFTP server from any browser, sessions, live log |
| [PhotinoSftpServer](../../Sample/PhotinoSftpServer/) | Cross-platform Photino Blazor desktop app — Blazor UI, start/stop, sessions, live log |
| [PortForwardingServer](../../Sample/PortForwardingServer/) | SSH port forwarding / tunneling — TCP forwarding policy, remote and local port forwarding, data flow through tunnels via SSH.NET |
| [ConsoleSftpServer](../../Sample/ConsoleSftpServer/) | Lightweight console-based SFTP server |
| [SFTPServerWithNuget](../../Sample/SFTPServerWithNuget/) | Minimal SFTP server consuming the published NuGet package  |
| [SimpleSSHDServer](../../Sample/SimpleSSHDServer/) | Windows Forms GUI server with integration test harness |
| [SftpClientServer](../../Sample/SftpClientServer/) | SFTP/SCP client example — connects to the server via SSH.NET to upload, download, and verify files over SFTP and SCP |

## Installation

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

One package. No Java dependencies. No manual IKVM setup.

---

**ApacheMinaSSHD.NET** is maintained by **[SERALYNX LLC](https://seralynx.com/)** — building secure file transfer solutions for critical infrastructure since 2015. For a turnkey portable SFTP server with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).


