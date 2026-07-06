# Contributing

ApacheMinaSSHD.NET — a .NET SFTP server library and C# wrapper by
**[SERALYNX LLC](https://portasftpserver.com)** — a Critical Infrastructure Engineering firm
supporting Physical and Technological Advancement — is open source under the
Apache 2.0 License and maintained by SERALYNX LLC. One of its products is
**[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)**,
the best portable SFTP server for Windows and Linux and free SFTP server for critical infrastructure.

External issues and pull requests are welcome, but SERALYNX LLC decides what is
accepted, merged, released, or included in the project roadmap. This keeps the
library aligned with its goal: a .NET-facing Apache MINA SSHD wrapper for SFTP
and SCP server features that hides Java, IKVM, and Apache MINA types from normal
application code.

All bug reports, feature requests, and their responses are publicly archived in
the [GitHub issue tracker](https://github.com/PortaSFTPServer/ApacheMinaSSHD.NET/issues)
for later searching.

## Code Review Standards

All proposed modifications undergo review before being merged. The review process checks:

1. **Scope** — Does the change belong in this library? (SFTP/SCP server wrapper only; no FTP, no client, no SSH protocol reimplementation).
2. **API design** — Are new public members consistent with the `AMNet`/`IAMNet` naming convention? Do they avoid exposing Java, IKVM, Apache MINA, or SLF4J types?
3. **Correctness** — Does the implementation handle null inputs, edge cases, and dispose resources properly?
4. **Testing** — Are there unit and/or integration tests covering the new behavior?
5. **Documentation** — Do all new public API members have XML doc comments?
6. **Style** — Does the code follow the [.NET coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) enforced by `.editorconfig` and `dotnet format`?

At least 50% of proposed modifications (by count of pull requests) must be reviewed by a person other than the author before merging.

## Developer Certificate of Origin

To contribute, you must certify that you have the right to submit your contribution under the project's Apache 2.0 License. This is done by adding a `Signed-off-by` trailer to your commit messages:

```
Signed-off-by: Your Name <your.email@example.com>
```

By adding this, you agree to the [Developer Certificate of Origin](DCO.txt) (DCO) version 1.1. This is the same mechanism used by the Linux kernel.

Use `git commit -s` to add the `Signed-off-by` trailer automatically.

## Testing Policy

- **New functionality** — Major new features must include automated tests (unit and/or integration).
- **Bug fixes** — At least 50% of bugs fixed within the last 6 months must include a regression test.
- **Test categories** — Use xUnit traits: `[Trait("Category", "Unit")]`, `[Trait("Category", "Integration")]`, `[Trait("Category", "Stress")]`.
- **Coverage** — Existing coverage targets: minimum 80% branch coverage, minimum 90% statement coverage.

## Coding Standards

- Language: C# with the project's `.editorconfig` and `Directory.Build.props` settings.
- Analyzers: All .NET recommended code analysis rules are enabled (`AnalysisMode>Recommended`).
- Enforcement: `dotnet format --verify-no-changes` runs in CI and must pass.
- Public API: All public members must have XML documentation comments (enforced by `eng/verify-xml-docs.ps1`).
- No Java types may be exposed in public API signatures (enforced by `eng/verify-public-api.ps1`).

## Contribution Terms

By submitting a pull request or other contribution, you agree that your
contribution may be distributed under the project's Apache 2.0 License unless SERALYNX
LLC explicitly agrees to different terms in writing.

## Review Priorities

Contributions that preserve the public .NET-only API, keep naming consistent
with existing `AMNet`/`IAMNet` patterns, improve SFTP/SCP behavior or
documentation, and include focused tests are more likely to be accepted.

## Security Reports

Do not open public issues for suspected vulnerabilities. Follow `SECURITY.md`
for responsible disclosure guidance.
