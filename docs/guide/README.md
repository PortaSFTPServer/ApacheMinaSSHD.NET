# ApacheMinaSSHD.NET Developer Guide

Build SFTP and secure file transfer solutions for .NET with ApacheMinaSSHD.NET — a C# wrapper around [Apache MINA SSHD](https://mina.apache.org/sshd-project/) by **[SERALYNX LLC](https://portasftpserver.com)** — a Critical Infrastructure Engineering firm supporting Physical and Technological Advancement. One of its products is **Porta SFTP Server**, the best portable SFTP server for Windows and Linux and free SFTP server for critical infrastructure.

This guide walks you through creating production-ready SSH file transfer servers entirely in .NET — zero Java dependencies.

## Who Is This For?

- **.NET developers** embedding SFTP/SCP into C# applications
- **IT administrators** deploying zero-install portable SFTP servers
- **Civil engineering and construction teams** sharing CAD, BIM, and 3D model files securely
- **DevOps engineers** automating secure file transfer pipelines

## What You'll Learn

| Guide | Description |
|-------|-------------|
| [Quick Start](01-quickstart.md) | Build your first SFTP server in under 5 minutes |
| [Server Configuration](02-configuration.md) | Timeouts, limits, banners, and algorithm tuning |
| [Authentication](03-authentication.md) | Password, public key, keyboard-interactive, and multi-factor auth |
| [Virtual Filesystem](04-virtual-filesystem.md) | Root jail isolation, user home directories, path containment |
| [SFTP Subsystem](05-sftp-subsystem.md) | File operations, directory filtering, event hooks |
| [SCP Subsystem](06-scp-subsystem.md) | SCP file opener, transfer events, permission mapping |
| [Security Best Practices](07-security.md) | Crypto algorithms, hardening, symlink containment |
| [Logging & Monitoring](08-logging.md) | SLF4J bridge, audit events, custom loggers |
| [Production Deployment](09-production-deployment.md) | Windows Service, Docker, high availability, monitoring |

## Sample Projects

The repository includes sample projects in the `Sample/` folder for every scenario:

| Project | Description |
|---------|-------------|
| [MinimalServer](../Sample/MinimalServer/) | Minimal C# SFTP server — simplest way to embed SFTP in .NET |
| [AuthenticationServer](../Sample/AuthenticationServer/) | Password, public key, fingerprint, authorized_keys, keyboard-interactive, and MFA |
| [VirtualFileSystemServer](../Sample/VirtualFileSystemServer/) | Root jail isolation, hidden-file filtering, path access control |
| [ScpServer](../Sample/ScpServer/) | SCP server with custom file opener, transfer audit, and directory filtering |
| [SftpEventServer](../Sample/SftpEventServer/) | SFTP event listeners — track open/close/read/write/create/remove/move |
| [SessionMonitorServer](../Sample/SessionMonitorServer/) | Session lifecycle, connection monitoring, proxy metadata inspection |
| [ProductionServer](../Sample/ProductionServer/) | Production-ready: JSON config, algorithm selection, combined listeners |
| [DareSftpServer](../Sample/DareSftpServer/) | Data-at-rest encryption with AES-256-GCM chunked encryption |
| [ConsoleSftpServer](../Sample/ConsoleSftpServer/) | Lightweight console-based SFTP server |
| [SimpleSSHDSever](../Sample/SimpleSSHDSever/) | Windows Forms GUI server with integration test harness |

## Installation

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

One package. No Java dependencies. No manual IKVM setup.

---

**ApacheMinaSSHD.NET** is maintained by **[SERALYNX LLC](https://portasftpserver.com)** — building secure file transfer solutions for critical infrastructure since 2015. For a turnkey portable SFTP server with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).
