# Changelog

## [Unreleased]

### Added
- **Host-based authentication** — `IAMNetHostBasedAuthenticator`, `AMNetDelegateHostBasedAuthenticator`, `setHostBasedAuthenticator()`, `SetHostBasedAuthenticator()`, `setDelegateHostBasedAuthenticator()`, `SetDelegateHostBasedAuthenticator()`
- **GSSAPI/Kerberos authentication** — `IAMNetGssapiAuthenticator`, `AMNetDelegateGssapiAuthenticator`, `InternalGssapiAuthenticator` (extends `GSSAuthenticator`), `setGssapiAuthenticator()`, `SetGssapiAuthenticator()`, `setDelegateGssapiAuthenticator()`, `SetDelegateGssapiAuthenticator()`
- **Shell/exec command handler** — `IAMNetCommandHandler`, `AMNetDelegateCommandHandler`, `AMNetCommandHandler`, `InternalCommandFactory`, `InternalCommand`, `setCommandHandler()`, `SetCommandHandler()`, `setDelegateCommandHandler()`, `SetDelegateCommandHandler()` for handling shell and exec requests from SSH clients
- **SFTP version control** — `AMNetSftpSubsystemFactory.MaximumVersion` property (range 3–6) to negotiate max SFTP protocol version
- **Compression configuration** — `AMNetSshAlgorithms.Compression` constants (`None`, `Zlib`, `ZlibDelayed`), `COMPRESSION` config property, `SetCompressionAlgorithms()`, `GetSupportedCompressionAlgorithms()`, `GetConfiguredCompressionAlgorithms()`
- **Dynamic (SOCKS) forwarding** — `AMNetForwardingType.Dynamic`, `CanForwardDynamic()` on `IAMNetTcpForwardingFilter`, `IAMNetForwardingFilter`, `AMNetTcpForwardingFilter`, `AMNetDelegateTcpForwardingFilter`, `AMNetForwardingFilter`, `InternalForwardingFilter.canForwardDynamic()`
- **Generic property configuration** — `SetProperty()`, `GetProperty()`, `GetIntProperty()`, `GetLongProperty()`, `GetBoolProperty()` on `AMNetSshServerConfig` for bandwidth and advanced settings
- Authentication method constants: `AMNetSshAuthenticationMethods.Gssapi`, `AMNetSshAuthenticationMethods.HostBased`
- TCP/Agent/X11 forwarding filter API with `AMNetTcpForwardingPolicy`, `ForwardedPortLocal`/`ForwardedPortRemote` support
- `InternalForwardingFilter` Java bridge implementing `ForwardingFilter`
- `FORWARDER_BUFFER_SIZE` and `FORWARD_REQUEST_TIMEOUT` config properties
- 27 new tests (14 unit + 13 integration with SSH.NET)
- `[Trait("Category", ...)]` on all test classes (Unit/Integration/Stress)
- `.editorconfig` with C# code-style rules
- Issue templates (bug report, feature request) and PR template
- `CHANGELOG.md`
- License headers on all source files
- Dependabot coverage expanded to all projects

### Changed
- `setSubsystemFactories()` now accepts `params AMNetSftpSubsystemFactory[]` for multiple subsystem factories
- `AMNetDelegateTcpForwardingFilter` accepts optional `canForwardDynamic` parameter
- `IAMNetTcpForwardingFilter` adds `CanForwardDynamic()` method
- `IAMNetForwardingFilter` adds `CanForwardDynamic()` method
- `SshServerTests` mock filters updated with `CanForwardDynamic()` implementation
- Heartbeat default from 0 to 45 seconds
- Path traversal guard in `AMNetSimpleGeneratorHostKeyProvider` rejects `..` before `Path.GetFullPath`
- NIO worker pool, socket backlog, keepalive, and TCP_NODELAY exposed with get/set + validation
- Connection rate limiter (`IAmNetConnectionRateLimiter`, `AMNetConnectionRateLimiter`, `RateLimitingIoServiceEventListener`)
- CodeQL workflow excludes `Bindings/` from analysis
- NuGet packages attach to GitHub Releases instead of NuGet.org publish

### Security
- **Upgraded Apache MINA SSHD from 2.18.0 to 2.19.0** to resolve CVE-2026-56624 (improper SSH certificate validation) and CVE-2026-48827 (path traversal in sshd-git)
- `setUpDefaultServer()` now calls `ApplyModernAlgorithmDefaults()` automatically, restricting to strong ciphers/MACs/key exchanges by default
- Host key temp files use `FileOptions.DeleteOnClose` to prevent unencrypted key material persisting on disk after crash
- PuTTY key decrypt failure now throws `CryptographicException` instead of silently generating a new host key (which would enable MITM)
- Command handler rejects exec/shell strings exceeding 32 KB to limit resource abuse
- Command execution moved from raw `Thread` to `ThreadPool` to prevent thread exhaustion under load
- Acceptor shutdown uses bounded wait (2s) instead of fixed 300ms sleep
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
