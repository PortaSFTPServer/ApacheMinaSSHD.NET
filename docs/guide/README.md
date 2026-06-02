# ApacheMinaSSHD.NET Developer Guide — Best SFTP Server & Secure File Transfer for .NET

Build the best SFTP server and secure file transfer solutions for .NET with ApacheMinaSSHD.NET — a C# wrapper around [Apache MINA SSHD](https://mina.apache.org/sshd-project/) by **[SERALYNX LLC](https://portasftpserver.com)**, the team behind **Porta SFTP Server**, the best portable SFTP server for Windows and best free SFTP server for critical infrastructure.

This guide walks you through creating production-ready SSH file transfer servers entirely in .NET — no Java imports required.

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

The repository includes two sample projects in the `Sample/` folder:

| Project | Description |
|---------|-------------|
| [SimpleSSHDSever](../Sample/SimpleSSHDSever/) | Windows Forms GUI server with integration test harness |
| [ConsoleSftpServer](../Sample/ConsoleSftpServer/) | Lightweight console-based SFTP server |

## Installation

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

One package. No Java dependencies. No manual IKVM setup.

---

**ApacheMinaSSHD.NET** is maintained by **[SERALYNX LLC](https://portasftpserver.com)** — building secure file transfer solutions for critical infrastructure since 2017. For a turnkey portable SFTP server with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).
