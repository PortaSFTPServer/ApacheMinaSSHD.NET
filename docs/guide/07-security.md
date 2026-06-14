# Security Best Practices

ApacheMinaSSHD.NET provides the building blocks for a secure SSH file transfer server. This guide covers hardening your deployment against common attack vectors.

## Cryptographic Algorithms

Start with modern algorithm preferences:

```csharp
server.Config.ApplyModernAlgorithmDefaults();
```

### Terrapin Attack (CVE-2023-48795) Mitigation

Apache MINA SSHD 2.12.0+ (bundled in this wrapper) implements the strict key
exchange extension (`kex-strict-*` version-header identifiers) that prevents the Terrapin
prefix-truncation attack. This wrapper uses Apache MINA SSHD 2.18.0, so the
mitigation is active by default — no configuration needed.

The modern cipher presets (`ApplyModernAlgorithmDefaults`) include ChaCha20-Poly1305
and AES-CTR modes, both safe under strict key exchange. Legacy CBC ciphers are
not included in the modern presets.

### Recommended Cipher Suite

```csharp
server.Config.SetCiphers(
    AMNetSshAlgorithms.Ciphers.Aes256Ctr,
    AMNetSshAlgorithms.Ciphers.Aes128Ctr,
    AMNetSshAlgorithms.Ciphers.Aes256Gcm,
    AMNetSshAlgorithms.Ciphers.Aes128Gcm);

server.Config.SetMacs(
    AMNetSshAlgorithms.Macs.HmacSha512,
    AMNetSshAlgorithms.Macs.HmacSha256);

server.Config.SetKeyExchangeAlgorithms(
    AMNetSshAlgorithms.KeyExchange.Curve25519Sha256,
    AMNetSshAlgorithms.KeyExchange.EcdhNistp521,
    AMNetSshAlgorithms.KeyExchange.EcdhNistp384,
    AMNetSshAlgorithms.KeyExchange.EcdhNistp256,
    AMNetSshAlgorithms.KeyExchange.DhGroup16Sha512);

server.Config.SetHostKeyAlgorithms(
    AMNetSshAlgorithms.HostKeys.RsaSha512,
    AMNetSshAlgorithms.HostKeys.RsaSha256,
    AMNetSshAlgorithms.HostKeys.EcdsaSha2Nistp521,
    AMNetSshAlgorithms.HostKeys.EcdsaSha2Nistp384,
    AMNetSshAlgorithms.HostKeys.Ed25519);
```

### FIPS Mode

Enable FIPS-compliant provider selection for regulated environments:

```csharp
using ApacheMinaSSHD.NET.Wrapper.Helpers;

AMNSecurityUtils.SetFipsMode(true);
```

## Host Key Management

```csharp
var hostKeys = new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser");
hostKeys.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
hostKeys.setKeySize(3072);        // At least 3072 bits
hostKeys.setStrictFilePermissions(true);  // Verify key file permissions
```

**Best practices:**
- Use at least RSA-3072 or Ed25519
- Store host keys in a secure, access-controlled location
- Rotate host keys periodically for high-security environments

## Authentication Hardening

```csharp
server.Config.MAX_AUTH_REQUESTS = 3;      // Limit brute force attempts
server.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(30); // Short login window
```

### Multi-Factor Authentication

Require multiple authentication factors:

```csharp
server.SetAuthenticationMethodGroups(
    new[] { AMNetSshAuthenticationMethods.PublicKey },
    new[] {
        AMNetSshAuthenticationMethods.Password,
        AMNetSshAuthenticationMethods.KeyboardInteractive
    });
```

## Session Limits

```csharp
server.Config.MAX_CONCURRENT_SESSIONS = 5;  // Per user
server.Config.MAX_CONCURRENT_CHANNELS = 5;  // Per session
server.Config.IDLE_TIMEOUT = TimeSpan.FromMinutes(5);
```

## Symlink Attack Prevention

ApacheMinaSSHD.NET includes automatic, multi-layered symlink containment:

| Layer | Technology |
|-------|------------|
| Java NIO | `Path.toRealPath()` normalization |
| Windows Native | `FindFirstFile` + `DeviceIoControl` reparse point resolution |
| .NET | `File.ResolveLinkTarget` fallback |
| Policy | `IsPathAllowed` callback on every file operation |

No configuration needed — containment is always active.

## Root Jail Isolation

```csharp
string jail = Path.Combine(AppContext.BaseDirectory, "sftp-root");
var fsFactory = new AMNetVirtualFileSystemFactory(jail, createUserDirectory: true);
server.setFileSystemFactory(fsFactory);
```

Each user is confined to `jail/{username}`. Path traversal via `../` in usernames is automatically blocked.

## Brute Force Protection

- `MAX_AUTH_REQUESTS` limits attempts per connection
- `AUTH_TIMEOUT` limits the authentication window
- Deny-by-default: all authenticators reject unless explicitly configured

## Data-at-Rest Encryption

For scenarios requiring encrypted storage, see [DareSftpServer](../../Sample/DareSftpServer/) — it demonstrates AES-256-GCM chunked encryption for data at rest.

## Dependency Vulnerability Scanning

Run the built-in security scanner in your CI pipeline:

```powershell
./eng/security-scan.ps1
```

This scans:
- **NuGet packages** for known vulnerabilities
- **Maven dependencies** via the OSV database

## Server Banner

Avoid leaking version information:

```csharp
server.Config.SERVER_IDENTIFICATION = "SSH-2.0-SFTP-Server";
```

## Network Security

- Bind to localhost or internal interfaces when possible
- Use a firewall to restrict access to the SSH port
- Consider a reverse proxy or load balancer for additional control
- Enable PROXY protocol for trusted front-end proxies:

```csharp
server.setServerProxyAcceptor(new AMNetServerProxyAcceptor());
```

## Audit Logging

```csharp
class SecurityAuditListener : AMNetSftpEventListener
{
    public override void OnOpen(ISshEvent e)
    {
        AuditLog("FILE_OPEN", e.Session.RemoteAddress, e.SshHandle?.PhysicalPath);
    }

    public override void OnWrite(ISshReadWrite e)
    {
        AuditLog("FILE_WRITE", e.Session.RemoteAddress,
            e.SshHandle?.PhysicalPath, bytes: e.Length);
    }

    public override void OnOpenFailed(ISshIOFailure e)
    {
        AuditLog("FILE_DENIED", e.Session.RemoteAddress, e.LocalPath);
    }
}
```

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| FIPS mode throws on second server instance | `SetFipsMode` is one-shot per JVM lifetime | The library handles this internally with a static sentinel — ensure you only call it once |
| "Weak algorithm" warning | Legacy cipher in default set | Call `ApplyModernAlgorithmDefaults()` to restrict to modern ciphers, MACs, and KEX |
| Host key file permissions too permissive | Default file permissions may be loose | Set `hostKeys.setStrictFilePermissions(true)` to enforce access control on the key file |

## Production Checklist

- [ ] Apply `ApplyProductionDefaults()` and `ApplyModernAlgorithmDefaults()`
- [ ] Use strong host keys (RSA-3072+ or Ed25519)
- [ ] Implement authentication against a real identity store
- [ ] Enable multi-factor authentication for sensitive environments
- [ ] Configure root jail with absolute paths
- [ ] Apply sensible session limits
- [ ] Add SFTP event listeners for audit logging
- [ ] Run `./eng/security-scan.ps1` in CI
- [ ] Restrict network access at the firewall level

---

**Next:** [Logging & Monitoring](08-logging.md) — SLF4J bridge, audit events, and custom loggers.

---

*ApacheMinaSSHD.NET is built by **SERALYNX LLC** — securing file transfer for critical infrastructure since 2015. For a production-ready portable SFTP server with built-in security features and GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
