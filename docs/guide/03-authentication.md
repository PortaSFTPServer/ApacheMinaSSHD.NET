# Authentication

ApacheMinaSSHD.NET supports multiple authentication methods, including multi-factor authentication (MFA) via authentication method groups. All authenticators are deny-by-default — no user is allowed unless explicitly configured.

## Password Authentication

### Fixed Credentials (Testing Only)

```csharp
server.SetFixedPasswordAuthenticator("username", "password");
```

Uses constant-time comparison (`CryptographicOperations.FixedTimeEquals`) to prevent timing attacks.

### Delegate (Callback)

```csharp
server.SetDelegatePasswordAuthenticator(
    (username, password, session) =>
    {
        // Validate against your identity store
        return ValidateUser(username, password);
    });
```

The callback receives `ISshSession` with `RemoteAddress` and `SessionId` for audit context.

### Custom Implementation

```csharp
class MyPasswordAuth : IAMNetPasswordAuthenticator
{
    public bool Authenticate(string username, string password, ISshSession session)
    {
        // Your identity store logic here
        return Database.ValidateCredentials(username, password);
    }

    // Optional: support password change
    public bool HandlePasswordChangeRequest(string username, string oldPassword, string newPassword, ISshSession session)
    {
        return Database.ChangePassword(username, oldPassword, newPassword);
    }
}

server.SetPasswordAuthenticator(new MyPasswordAuth());
```

### Composite (Multiple Providers)

```csharp
server.SetCompositePasswordAuthenticator(
    new LdapPasswordAuthenticator(),
    new LocalPasswordAuthenticator(),
    new AMNetFixedPasswordAuthenticator("admin", "emergency-pass"));
```

Each authenticator is tried in order until one accepts the credentials.

## Public Key Authentication

### Fingerprint-Based

```csharp
server.SetFingerprintPublickeyAuthenticator("username",
    "SHA256:abc123...",
    "SHA256:def456...");
```

### Authorized Keys File

OpenSSH-compatible `authorized_keys` file:

```csharp
server.SetAuthorizedKeysAuthenticator("/path/to/authorized_keys");
```

### Delegate (Callback)

```csharp
server.SetDelegatePublickeyAuthenticator(
    (username, fingerprint, session) =>
    {
        return Database.KeyIsAllowed(username, fingerprint);
    });
```

### Custom Implementation

```csharp
class MyPublicKeyAuth : IAMNetPublickeyAuthenticator
{
    public bool Authenticate(string username, string fingerprint, ISshSession session)
    {
        return ValidatePublicKey(username, fingerprint);
    }
}

server.SetPublickeyAuthenticator(new MyPublicKeyAuth());
```

### Composite

```csharp
server.SetCompositePublickeyAuthenticator(
    new DatabaseKeyAuth(),
    new FileBasedKeyAuth());
```

### Directory-Backed (Legacy)

The `AMNetPublickeyAuthenticator` scans an `Authorized_Keys` directory for files matching the username. Supports PEM, OpenSSH public key, OpenSSH private key, and SSH2 public key formats.

```csharp
server.SetPublickeyAuthenticator(new AMNetPublickeyAuthenticator(basePath));
```

## Keyboard-Interactive Authentication

### Fixed Response

```csharp
server.SetFixedKeyboardInteractiveAuthenticator(
    expectedResponse: "123456",
    username: "user",
    prompt: "Verification code",
    interactionName: "2FA",
    instruction: "Enter the code from your authenticator app.");
```

### Delegate (Callback)

```csharp
server.SetDelegateKeyboardInteractiveAuthenticator(
    generateChallenge: (username, challenge) =>
    {
        challenge.AddPrompt("Current PIN:", false);
        challenge.AddPrompt("New PIN:", true);
    },
    authenticate: (session, username, responses) =>
    {
        return responses[0] == "1234" && responses[1] == "5678";
    });
```

## Multi-Factor Authentication

Require multiple authentication methods to succeed sequentially:

```csharp
// Require public key first, then password
server.SetAuthenticationMethods(
    AMNetSshAuthenticationMethods.PublicKey,
    AMNetSshAuthenticationMethods.Password);
```

Or require multiple methods in one step:

```csharp
// Require both password AND keyboard-interactive (in sequence)
server.SetAuthenticationMethods(
    AMNetSshAuthenticationMethods.RequireAll(
        AMNetSshAuthenticationMethods.Password,
        AMNetSshAuthenticationMethods.KeyboardInteractive));
```

Or offer alternative combinations:

```csharp
// Either public key alone, OR password+keyboard-interactive
server.SetAuthenticationMethodGroups(
    new[] { AMNetSshAuthenticationMethods.PublicKey },
    new[] {
        AMNetSshAuthenticationMethods.Password,
        AMNetSshAuthenticationMethods.KeyboardInteractive
    });
```

## Session Event Callbacks

Monitor authentication and session lifecycle:

```csharp
server.addSessionListener(new AMNetSessionListener());
```

Low-level connection monitoring:

```csharp
server.setIoServiceEventListener(new AMNetIoServiceEventListener());
```

PROXY protocol support for load balancers:

```csharp
server.setServerProxyAcceptor(new AMNetServerProxyAcceptor());
```

## Security Scanner Integration

ApacheMinaSSHD.NET includes a security scanning script that checks both NuGet and Maven dependencies for known CVEs. Run it as part of your CI pipeline:

```powershell
./eng/security-scan.ps1
```

This scans all project files for vulnerable package references and queries the OSV database for Maven dependency vulnerabilities.

---

**Next:** [Virtual Filesystem](https://github.com/PortaSFTPServer/ApacheMinaSSHD.NET/blob/main/docs/guide/04-virtual-filesystem.md) — root jail isolation, user home directories, and path containment.

---

*ApacheMinaSSHD.NET is maintained by **SERALYNX LLC**. For a turnkey portable SFTP server with enterprise authentication, monitoring, and GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
