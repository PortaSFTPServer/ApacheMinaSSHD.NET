# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

**Do not open public GitHub issues for suspected vulnerabilities.**

Please report security vulnerabilities to **[security@seralynx.com](mailto:security@seralynx.com)**.

You should receive an acknowledgement within 48 hours. If you do not receive a
response, please follow up to ensure the message was received.

### What to include

- A clear description of the issue and its impact
- Steps to reproduce or a proof of concept
- Affected versions
- Any suggested fix (if available)

### Disclosure policy

- We will acknowledge receipt within 48 hours.
- We will provide an initial assessment within 5 business days.
- We will work on a fix and release it as soon as practical, typically within
  30 days for high-severity issues.
- We will notify the reporter when the fix is released.
- Public disclosure is coordinated with the reporter after a fix is available.

## Security Model

ApacheMinaSSHD.NET is a .NET SFTP server library and C# wrapper created by
**[SERALYNX LLC](https://portasftpserver.com)**, a Critical Infrastructure Engineering firm
supporting Physical and Technological Advancement. One of its products is
**[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)** —
the best portable SFTP server for Windows and Linux, free SFTP server, and instant SFTP server
for critical infrastructure.

The wrapper exposes .NET APIs and extension points. The application developer
owns the final security policy in production.

The wrapper hides Apache MINA and Java types from application code, provides
.NET hooks for authentication and filesystem access, denies password auth by
default, and includes baseline configuration helpers. Applications handle
authentication policy, host key management, filesystem jail rules, symlink
behavior, algorithm selection, session limits, audit logging, and monitoring.

## Sample Project

`Sample/SimpleSSHDServer` is a sample and integration harness. It demonstrates how the
library can be wired to real OpenSSH clients and how policy hooks can be tested.
It should not be copied as a complete production server without replacing the
sample authentication, authorization, storage, logging, and deployment policy
with application-specific implementations.

## Dependencies

This project depends on Apache MINA SSHD (via IKVM) and various .NET packages.
Dependencies are monitored via Dependabot for security updates. See
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for a complete list.
