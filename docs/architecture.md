# Architecture

ApacheMinaSSHD.NET is a .NET wrapper around Apache MINA SSHD that translates Java SSH/SFTP/SCP server APIs into .NET idioms. This document describes its high-level structure.

## Component Overview

```
┌──────────────────────────────────────────────────┐
│                  Application Code                  │
│  (.NET — no Java, IKVM, or Apache MINA imports)   │
├──────────────────────────────────────────────────┤
│                  .NET Wrapper Layer                │
│  ApacheMinaSSHD.NET.Wrapper.dll                    │
│  - AMNetSshServer (fluent server config)           │
│  - AMNetSshAlgorithms (algorithm constants)        │
│  - Authenticators, Factories, Event Listeners      │
│  - Abstract base classes (AMNet* pattern)          │
├──────────────────────────────────────────────────┤
│                  IKVM Bridge                       │
│  ApacheMinaSSHD.NET.Bindings.dll                   │
│  (ikvmc-compiled Java→.NET assembly)               │
├──────────────────────────────────────────────────┤
│              Apache MINA SSHD (Java)               │
│  - SSH transport, key exchange, authentication     │
│  - SFTP subsystem, SCP command                     │
│  - Port forwarding, session management             │
│  - Cryptographic providers (Bouncy Castle)         │
└──────────────────────────────────────────────────┘
```

## Layers

### 1. Application Code

.NET code that references only `ApacheMinaSSHD.NET.Wrapper`. No Java, Apache MINA, IKVM, or SLF4J types appear in public API signatures. All SSH concepts are exposed through .NET interfaces (`ISshSession`, `ISshEvent`, `ISshFileSystemAccess`, etc.).

### 2. Wrapper Layer (`ApacheMinaSSHD.NET.Wrapper`)

The wrapper provides:

- **`AMNetSshServer`** — fluent server configuration with .NET properties and methods
- **`AMNetSshAlgorithms`** — static string constants for SSH algorithm names
- **Authenticators** — password, public key, keyboard-interactive, host-based, GSSAPI/Kerberos
- **Factories** — SFTP subsystem factory, SCP command factory, virtual file system factory
- **Event listeners** — SFTP lifecycle events, session events
- **Helpers** — FIPS mode, security utilities
- **Abstract base classes** — `AMNetSftpFileSystemAccessor`, `AMNetScpFileOpener`, etc.

All wrapper classes use the `AMNet` or `IAMNet` prefix to avoid namespace collisions.

### 3. IKVM Bridge (`ApacheMinaSSHD.NET.Bindings`)

IKVM translates Apache MINA SSHD's Java bytecode into a .NET assembly. This is a build-time artifact — no Java runtime is needed. The bindings assembly is bundled inside the NuGet package.

### 4. Apache MINA SSHD (via IKVM)

The underlying SSH protocol engine handles:

- SSH transport layer (RFC 4253): key exchange, encryption, MAC, compression
- User authentication (RFC 4252): password, public key, host-based, keyboard-interactive, GSSAPI
- Connection protocol (RFC 4254): channels, port forwarding, subsystems
- SFTP (SSH File Transfer Protocol): file operations, directory listing, attributes
- SCP: file copy over SSH command channel

## Key Design Decisions

- **No Java runtime dependency** — IKVM compiles Java to .NET at build time
- **No SSH protocol code in the wrapper** — all SSH/SFTP/SCP protocol logic is in Apache MINA SSHD
- **Deny by default** — password authentication is denied unless explicitly configured
- **Algorithm negotiation** — modern presets prefer AEAD ciphers, Curve25519 KEX, Ed25519/ECDSA host keys
- **Extensibility** — all authenticators, listeners, and file accessors support delegate-based customization
