# Security Model

ApacheMinaSSHD.NET is a .NET SFTP server library and C# wrapper created by
**[SERALYNX LLC](https://portasftpserver.com)** is a Critical Infrastructure Engineering firm
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
