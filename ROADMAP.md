# Roadmap

This document outlines the intended direction of ApacheMinaSSHD.NET for the next 12 months. It is a living document and may change based on community feedback, security considerations, and maintainer priorities.

## Current State (v1.x)

- Apache MINA SSHD 2.18.0 wrapped via IKVM
- Full SFTP v3 and SCP subsystem support
- Password, public key, host-based, GSSAPI/Kerberos, and keyboard-interactive authentication
- MFA (multi-factor authentication) support
- Virtual file system with root jail isolation
- SFTP/SCP event hooks and session monitoring
- SSH port forwarding (local, remote, X11, dynamic)
- Terrapin attack mitigation (kex-strict)
- Post-quantum key exchange (`sntrup761x25519-sha512@openssh.com`)
- FIPS 140-2/140-3 compliance

## Short-Term (Next 6 Months)

- Upgrade to Apache MINA SSHD 2.19+ (when released) for upstream security fixes
- Enhanced benchmark suite for connection throughput and transfer performance
- Additional sample projects demonstrating advanced scenarios (cluster deployment, HA)

## Medium-Term (6-12 Months)

- API stabilization for v2.0 (breaking changes accepted)
- Reduced IKVM overhead / native AOT compatibility investigation
- Extended audit logging with structured output (JSON/CEF)
- Certificate-based authentication (X.509) support
- Expanded post-quantum algorithm support as NIST standards solidify

## Out of Scope

The following are intentionally **not** planned:

- FTP, FTPS, or TFTP protocol support (use a dedicated server)
- SSH client functionality (use SSH.NET)
- GUI management tools (use the sample Avalonia/Photino apps as a starting point)
- Cloud-managed hosting services
- Non-SSH protocol support of any kind

## Version Compatibility

| Component | Current Minimum | Target |
|-----------|----------------|--------|
| .NET | 9.0 | 10.0 LTS |
| Bundled Apache MINA SSHD | 2.18.0 | 2.19+ |
| IKVM | 8.15.0+ | 8.15.0+ |
