# Security Review

This document describes the security review process for ApacheMinaSSHD.NET.

## Review Cadence

A security review of the project is performed at least once every 12 months, or more frequently if there are significant changes to the SSH protocol, cryptographic landscape, or upstream dependencies.

## Review Scope

Each security review examines:

1. **Cryptographic algorithm configuration** — Cipher suites, MAC algorithms, key exchange methods, and host key algorithms are reviewed against current SSH recommendations (RFC 9142, RFC 8731, RFC 8709).
2. **Authentication boundary** — Review of authentication hooks to ensure the "deny by default" principle is maintained.
3. **File system access** — Review of virtual file system isolation, symlink handling, and path traversal protections.
4. **Session management** — Session lifecycle, timeout enforcement, concurrent connection limits.
5. **Dependency vulnerabilities** — CVE scan results for NuGet packages and Maven references (Apache MINA SSHD, SLF4J, Bouncy Castle through IKVM).
6. **Upstream security patches** — Review of Apache MINA SSHD release notes for relevant security fixes.

## Review Methods

The review combines:

- **Automated static analysis** — CodeQL scan (GitHub Code Scanning)
- **Dependency scanning** — NuGet vulnerability audit, OSV API scan for Maven references
- **Manual code review** — Human review of critical security paths (authentication, file system access, crypto configuration)
- **Integration testing** — Automated tests against live SSH clients (SSH.NET, Windows OpenSSH)
- **Fuzz testing** — Stress testing with varied connection patterns and payloads

## Last Review

| Date | Scope | Findings | Status |
|------|-------|----------|--------|
| 2026-06 | Initial project security review | See [Security Best Practices Guide](guide/07-security.md) | Complete |
