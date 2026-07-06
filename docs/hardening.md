# Hardening Mechanisms

This document describes the hardening mechanisms used in ApacheMinaSSHD.NET to reduce the likelihood that software defects result in security vulnerabilities.

## Compiler-Level Hardening

The following settings are applied in `Directory.Build.props`:

| Setting | Value | Purpose |
|---------|-------|---------|
| `<Deterministic>` | `true` | Ensures identical binary output for identical source — enables reproducible builds |
| `<AnalysisLevel>` | `latest` | Enables the latest available code analysis rules |
| `<AnalysisMode>` | `Recommended` | Enables Microsoft's recommended set of analyzer rules |
| `<EnforceCodeStyleInBuild>` | `true` | Code style violations are treated as build warnings |
| `<ContinuousIntegrationBuild>` | `true` (CI) | Optimizes build for CI, enables source link |

## Static Analysis

- **Roslyn analyzers** — Build-time analysis via .NET SDK's built-in analyzers (`AnalysisMode>Recommended`)
- **CodeQL** — GitHub CodeQL workflow scans for security vulnerabilities on every push and pull request
- **EditorConfig** — Coding conventions enforced at build time via `.editorconfig`

## Code Review

See [CONTRIBUTING.md](../CONTRIBUTING.md) for code review standards. All pull requests are reviewed for security, correctness, and adherence to coding standards.

## Automated Security Scanning

| Tool | When | What It Checks |
|------|------|---------------|
| `dotnet list package --vulnerable` | Every CI run | NuGet packages for known CVEs |
| OSV API scan | Every CI run | Maven/Java dependencies (Apache MINA SSHD, SLF4J) for known vulnerabilities |
| Dependabot | Weekly | NuGet and GitHub Actions dependency updates with CVE information |
| CodeQL | Every push + weekly | C# code for security vulnerabilities |

## Runtime Hardening

- **FIPS mode** — `AMNSecurityUtils.SetFipsMode(true)` restricts cryptographic operations to NIST-approved algorithms
- **Modern algorithm defaults** — `ApplyModernAlgorithmDefaults()` enables AEAD ciphers, Curve25519 KEX, Ed25519/ECDSA host keys; disables legacy algorithms
- **Deny by default** — Password authentication is disabled until explicitly configured
- **Terrapin mitigation** — `kex-strict-*` strict key exchange (Apache MINA SSHD 2.12.0+, bundled 2.18.0)

## Dependency Management

- Dependencies are declared via NuGet references and Maven references (IKVM)
- Software Bill of Materials (SBOM) generated during release using Microsoft SBOM tool
- THIRD-PARTY-NOTICES.md documents all dependencies and their licenses
