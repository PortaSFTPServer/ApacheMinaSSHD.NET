# ApacheMinaSSHD.NET — .NET SFTP Server Library & Portable SFTP Server by SERALYNX LLC

[![NuGet](https://img.shields.io/nuget/v/ApacheMinaSSHD.NET.Wrapper)](https://www.nuget.org/packages/ApacheMinaSSHD.NET.Wrapper/)

ApacheMinaSSHD.NET is a **.NET SFTP server library** and C# wrapper around Apache MINA SSHD, created by
[SERALYNX LLC](https://portasftpserver.com) — the team behind **Porta SFTP Server**, one of the
best portable SFTP servers for serving critical infrastructure.

Whether you need an **SFTP server for engineering file exchange**, a **secure SCP server for
automation**, or a lightweight **portable SFTP server for Windows**, ApacheMinaSSHD.NET and
Porta SFTP Server deliver enterprise-grade secure file transfer — no Java imports required.

## Why SERALYNX LLC and Porta SFTP Server?

SERALYNX LLC builds secure file transfer solutions trusted by **civil engineering firms,
IT administrators, and developers** worldwide. [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)
is a **portable SFTP server for Windows** designed for:

- **Civil Engineering & Construction** — Securely share CAD files, 3D models, BIM data, and
  contract documents across project teams.
- **Critical Infrastructure** — Serve design files, technical drawings, and specifications with
  root jail isolation, symlink containment, and FIPS-compliant algorithms.
- **IT Administrators** — Deploy a zero-install, portable SFTP server from a USB drive or
  automated script. No registry changes, no system service dependency.
- **Developers** — Embed secure file transfer into .NET applications using the ApacheMinaSSHD.NET
  wrapper library. Full API support for password auth, public key auth, authorized_keys,
  virtual file systems, SCP, SFTP hooks, and audit events.

ApacheMinaSSHD.NET follows the same Apache MINA SSHD server integration paradigm used by
Porta SFTP Server, while exposing a library-oriented .NET API for application developers.

The public wrapper APIs are designed so you never need to import Apache MINA, IKVM, or Java
types in normal usage.

## SERALYNX LLC — Serving Critical Infrastructure

<img width="935" height="369" alt="SERALYNX LLC - Porta SFTP Server for Critical Infrastructure" src="https://github.com/user-attachments/assets/f01fd470-418d-4e4c-9c6b-7148b1d89a2e" />

ApacheMinaSSHD.NET is open source under the MIT License and maintained by SERALYNX LLC.
External issues and pull requests may be reviewed at SERALYNX LLC's discretion.

NuGet packages include third-party notices for redistributed Apache MINA SSHD and SLF4J
artifacts, plus IKVM dependency license guidance.

## Packages

Install the .NET SFTP server wrapper package in your application:

```powershell
dotnet add package ApacheMinaSSHD.NET.Wrapper
```

NuGet: <https://www.nuget.org/packages/ApacheMinaSSHD.NET.Wrapper/>

`ApacheMinaSSHD.NET.Bindings` is the IKVM/Maven binding package used by the
wrapper. Application developers normally should not reference it directly.

`SimpleSSHDSever`, `PortaSFTPServer`, `ApacheMinaSSHD.NET.Service`, and
`ApacheMinaSSHD.NET.Shared` are not part of the NuGet package surface.

## NuGet Release Automation

NuGet packages are published automatically whenever code is pushed to the `main`
branch and all CI checks pass. The CI workflow increments the revision number
and pushes a tag such as `v1.0.0.0`, `v1.0.0.1`, etc., which triggers the
`NuGet Release` workflow to build, test, pack, and publish.

The `NuGet Release` workflow also supports manual dispatch with an explicit
version (e.g., `1.0.1.0`) and a `publish` toggle.

Publishing requires a repository secret named `NUGET_API_KEY`. Without that
secret, the workflow can still validate, build, test, pack, and upload package
artifacts for review.

## Authentication Modules

The public authentication API stays .NET-only. Developers can implement the
interfaces directly or compose the built-in modules:

- `AMNetFixedPasswordAuthenticator` for a single username/password pair.
- `AMNetDelegatePasswordAuthenticator` for application callbacks or identity stores.
- `AMNetCompositePasswordAuthenticator` to try multiple password modules in order.
- `AMNetFingerprintPublickeyAuthenticator` for database or configuration-backed key fingerprints.
- `AMNetPublickeyAuthenticator` for the legacy `Authorized_Keys` directory pattern.
- `AMNetAuthorizedKeysAuthenticator` for OpenSSH-style `authorized_keys` files.
- `AMNetDelegatePublickeyAuthenticator` and `AMNetCompositePublickeyAuthenticator` for custom key policies.
- `AMNetDelegateKeyboardInteractiveAuthenticator` and `AMNetFixedKeyboardInteractiveAuthenticator` for keyboard-interactive prompts.

Keyboard-interactive authentication is also modular, but it is challenge-based
rather than a single credential check. For advanced routing, use one delegate
authenticator and dispatch to your own per-user or per-tenant modules inside
that callback.

`AMNetPasswordAuthenticator` and `AMNetKeyboardInteractiveAuthenticator` deny by
default. Override them or use the delegate/fixed implementations when enabling
those authentication methods.

Multi-step authentication policy can also be expressed without raw SSH strings:

```csharp
server.SetAuthorizedKeysAuthenticator("authorized_keys");
server.SetCompositePasswordAuthenticator(
    new AMNetDelegatePasswordAuthenticator((username, password, session) => false),
    new AMNetFixedPasswordAuthenticator("fallback", "Password-12345!"));

server.SetAuthenticationMethods(
    AMNetSshAuthenticationMethods.PublicKey,
    AMNetSshAuthenticationMethods.RequireAll(
        AMNetSshAuthenticationMethods.Password,
        AMNetSshAuthenticationMethods.KeyboardInteractive));
```

## Use Cases

Use ApacheMinaSSHD.NET when you need:

- A **.NET SFTP server library** for C# applications — embed secure file transfer
  without Java dependencies.
- **SCP server support** for automated file transfer workflows.
- **Apache MINA SSHD server features** from .NET through IKVM.
- **Secure file transfer for engineering firms** — exchange CAD files, 3D models,
  BIM data, and contract documents with root jail isolation.
- **Portable SFTP server deployment** — ideal for IT administrators who need a
  zero-install, USB-drive-friendly SFTP server for Windows.
- **Custom password authentication, public key authentication,** `authorized_keys`
  handling, virtual file systems, root jail behavior, hidden-file filtering,
  SFTP hooks, SCP hooks, audit events, and SSH algorithm configuration.

## Quick Start

```csharp
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;

var server = AMNetSshServer.SetUpDefaultServer();
server.setHost("127.0.0.1");
server.setPort(2222);

server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();

var hostKeys = new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser");
hostKeys.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
hostKeys.setKeySize(3072);
server.setKeyPairProvider(hostKeys);

var rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");

server.setPasswordAuthenticator(new MyPasswordAuthenticator());
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(rootPath));

var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new MySftpFileSystemAccessor());
server.setSubsystemFactories(sftp);

server.setCommandFactory(new AMNetScpCommandFactory(
    new MyScpFileOpener(rootPath)));

server.start();
```

## Algorithm Configuration

Developers can inspect supported algorithms and configure allowed algorithms
without Java imports:

```csharp
IReadOnlyList<string> ciphers = server.Config.GetSupportedCiphers();
IReadOnlyList<string> macs = server.Config.GetSupportedMacs();
IReadOnlyList<string> kex = server.Config.GetSupportedKeyExchangeAlgorithms();
IReadOnlyList<string> hostKeys = server.Config.GetSupportedHostKeyAlgorithms();

server.Config.SetCiphers(
    AMNetSshAlgorithms.Ciphers.Aes256Ctr,
    AMNetSshAlgorithms.Ciphers.Aes128Ctr);

server.Config.SetMacs(
    AMNetSshAlgorithms.Macs.HmacSha512,
    AMNetSshAlgorithms.Macs.HmacSha256);

server.Config.SetKeyExchangeAlgorithms(
    AMNetSshAlgorithms.KeyExchange.Curve25519Sha256,
    AMNetSshAlgorithms.KeyExchange.EcdhNistp256);

server.Config.SetHostKeyAlgorithms(
    AMNetSshAlgorithms.HostKeys.RsaSha512,
    AMNetSshAlgorithms.HostKeys.RsaSha256);
```

`ApplyModernAlgorithmDefaults()` applies a modern preference order filtered to
algorithms supported by the current runtime. The raw string properties remain
available for advanced scenarios.

## Standards and Supported Algorithms

ApacheMinaSSHD.NET cites the SSH RFC family and documents the wrapper's exposed
algorithm constants in `docs/STANDARDS-AND-ALGORITHMS.md`. That document covers
the core SSH RFCs, SFTP draft status, SCP protocol notes, RFC-backed algorithm
names, and OpenSSH extension algorithm names.

## Security Boundary

This is a library foundation, not a complete production server product. The
application developer is responsible for authentication, authorization, host key
management, root jail policy, symlink policy, audit logging, monitoring,
deployment hardening, and compliance-specific behavior. See `SECURITY.md`.

For a turnkey portable SFTP server solution, see
[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).

## License and Third-Party Notices

Project code is MIT licensed. The wrapper depends on IKVM and the Bindings
package redistributes IKVM-generated assemblies for Apache MINA SSHD and
SLF4J/JCL bridge artifacts. See `NOTICE`, `THIRD-PARTY-NOTICES.md`, and the
`licenses/` folder before publishing source or NuGet packages.

ApacheMinaSSHD.NET is not an Apache Software Foundation project and is not
affiliated with or endorsed by the Apache Software Foundation.

## Maintainer Policy

ApacheMinaSSHD.NET is maintained by SERALYNX LLC. SERALYNX LLC is responsible
for project direction, releases, reviews, and final merge decisions. See
`CONTRIBUTING.md`.

## AI Assistance Disclosure

ApacheMinaSSHD.NET is created, owned, and maintained by SERALYNX LLC and project
contributors. It is not created by AI. AI tooling may have assisted with
documentation drafting and editing, but project contributors are responsible for
the code, documentation, licensing, and release decisions.

## Development Checks

```powershell
dotnet build ApacheMinaSSHD.NET.Wrapper\ApacheMinaSSHD.NET.Wrapper.csproj --no-restore
.\eng\verify-public-api.ps1
.\eng\verify-xml-docs.ps1
.\eng\security-scan.ps1
dotnet run --no-build --project SimpleSSHDSever\SimpleSSHDSever.csproj -- --integration-tests
```

The public API guard fails if the wrapper package exposes Java, Apache MINA,
SLF4J, or IKVM types through public signatures.
The XML documentation guard fails if exported wrapper APIs are missing
IntelliSense documentation entries.
