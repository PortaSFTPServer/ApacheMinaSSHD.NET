# Changelog

## [Unreleased]

### Added
- TCP/Agent/X11 forwarding filter API with `AMNetTcpForwardingPolicy`, `ForwardedPortLocal`/`ForwardedPortRemote` support
- `InternalForwardingFilter` Java bridge implementing `ForwardingFilter`
- `FORWARDER_BUFFER_SIZE` and `FORWARD_REQUEST_TIMEOUT` config properties
- 27 new tests (14 unit + 13 integration with SSH.NET)
- `PortForwardingServer` sample project
- `[Trait("Category", ...)]` on all test classes (Unit/Integration/Stress)
- `.editorconfig` with C# code-style rules
- Issue templates (bug report, feature request) and PR template
- `CHANGELOG.md`
- License headers on all source files
- Dependabot coverage expanded to all projects

### Changed
- Heartbeat default from 0 to 45 seconds
- Path traversal guard in `AMNetSimpleGeneratorHostKeyProvider` rejects `..` before `Path.GetFullPath`
- NIO worker pool, socket backlog, keepalive, and TCP_NODELAY exposed with get/set + validation
- Connection rate limiter (`IAmNetConnectionRateLimiter`, `AMNetConnectionRateLimiter`, `RateLimitingIoServiceEventListener`)
- CodeQL workflow excludes `Bindings/` from analysis
- NuGet packages attach to GitHub Releases instead of NuGet.org publish

### Security
- Heartbeat default 45s to prevent idle session resource exhaustion
- Host key path traversal validated at constructor time
- Per-IP sliding-window connection rate limiter

## [1.0.0.8-beta] - 2026-05

### Added
- CodeQL workflow with `Bindings/` path exclusion
- `Sample/PortForwardingServer` project

### Changed
- Bumped Apache MINA SSHD from 2.17.1 to 2.18.0
- Multi-targeted to `net9.0;net10.0`
- NuGet packaging: bundle IKVM interop DLLs per TFM via `_PackageFiles`
- Various documentation and SEO improvements

### Fixed
- PhotinoSftpServer: `[STAThread]`, `InvokeAsync`, safe `SessionId`, blank window fix
- NuGet package URL (trailing slash → 404)
- `Tmds.DBus.Protocol` vulnerability (CVE-2026-39959, CVSS 7.1)
- `AMNSecurityUtils.SetFipsMode` one-shot per JVM lifetime
- Shields.io badges blocked by CSP

## [1.0.0.0-beta] - 2026-04

### Added
- Initial public release
- Full SSH/SFTP/SCP server via Apache MINA SSHD 2.17.1 IKVM interop
- Wrapper API: `AMNetSshServer`, `AMNetSshServerConfig`, authenticators, host key providers, file system factories
- `Sample/` projects: MinimalServer, ConsoleSftpServer, AuthenticationServer, ProductionServer, ScpServer, SftpEventServer, SessionMonitorServer, VirtualFileSystemServer, SimpleSSHDServer, BlazorSftpServer, PhotinoSftpServer, AvaloniaSftpServer, DareSftpServer, SFTPServerWithNuget
- CI/CD: build, test, pack, release workflows
- Documentation: guides, API reference site, SEO

### Security
- Path traversal and symlink containment hardening
- Credential zeroing after use
- Host key size enforcement (min 2048-bit)
- Port validation
- Glob injection prevention
