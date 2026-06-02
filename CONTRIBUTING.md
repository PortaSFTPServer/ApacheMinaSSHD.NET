# Contributing

ApacheMinaSSHD.NET — a .NET SFTP server library and C# wrapper created by
[SERALYNX LLC](https://portasftpserver.com), the team behind
**[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)** —
is open source under the MIT License and maintained by SERALYNX LLC.

External issues and pull requests are welcome, but SERALYNX LLC decides what is
accepted, merged, released, or included in the project roadmap. This keeps the
library aligned with its goal: a .NET-facing Apache MINA SSHD wrapper for SFTP
and SCP server features that hides Java, IKVM, and Apache MINA types from normal
application code.

## Contribution Terms

By submitting a pull request or other contribution, you agree that your
contribution may be distributed under the project's MIT License unless SERALYNX
LLC explicitly agrees to different terms in writing.

## Review Priorities

SERALYNX LLC gives priority to contributions that:

- Preserve the public .NET-only API surface.
- Keep naming consistent with existing `AMNet`, `IAMNet`, `ISsh`, `Ssh`, and
  `Internal` patterns.
- Improve SFTP/SCP server behavior, reliability, documentation, or security
  posture.
- Include focused tests or sample coverage when behavior changes.
- Avoid unrelated refactors and generated-file churn.

## Security Reports

Do not open public issues for suspected vulnerabilities. Follow `SECURITY.md`
for responsible disclosure guidance.
