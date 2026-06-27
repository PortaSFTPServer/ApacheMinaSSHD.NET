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
        return responses.GetResponses()[0] == "1234" && responses.GetResponses()[1] == "5678";
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

## Host-Based Authentication

Host-based authentication verifies a client by the host key and hostname from which the connection originates, in addition to the username.

### Delegate (Callback)

```csharp
server.SetDelegateHostBasedAuthenticator(
    (username, fingerprint, clientHostname, clientUsername, session) =>
    {
        // Validate the host key fingerprint and client hostname
        return AllowedHosts.ContainsKey(clientHostname) &&
               AllowedHosts[clientHostname] == fingerprint;
    });
```

The callback receives:
- `username` — The SSH username being authenticated
- `fingerprint` — SHA-256 fingerprint of the client's host key
- `clientHostname` — The hostname claimed by the client
- `clientUsername` — The username on the client machine
- `session` — Session metadata for audit context

### Custom Implementation

```csharp
class MyHostBasedAuth : IAMNetHostBasedAuthenticator
{
    public bool Authenticate(string username, string fingerprint,
        string clientHostname, string clientUsername, ISshSession session)
    {
        return HostDatabase.IsTrustedHost(clientHostname, fingerprint);
    }
}

server.SetHostBasedAuthenticator(new MyHostBasedAuth());
```

Host-based authentication is typically combined with public key or password as a multi-factor method. Add it to an authentication chain:

```csharp
server.SetAuthenticationMethods(
    AMNetSshAuthenticationMethods.PublicKey,
    AMNetSshAuthenticationMethods.HostBased);
```

## GSSAPI/Kerberos Authentication

GSSAPI (Generic Security Services API) enables Kerberos-based single sign-on authentication for SSH connections.

### Delegate (Callback)

```csharp
server.SetDelegateGssapiAuthenticator(
    (session, identity) =>
    {
        // identity is the Kerberos principal (e.g., user@REALM)
        return KerberosService.ValidatePrincipal(identity);
    });
```

### Custom Implementation

```csharp
class MyGssapiAuth : IAMNetGssapiAuthenticator
{
    public bool ValidateIdentity(ISshSession session, string identity)
    {
        // Validate the Kerberos principal
        return identity.EndsWith("@YOUR-REALM.COM");
    }
}

server.SetGssapiAuthenticator(new MyGssapiAuth());
```

GSSAPI is typically used alongside other methods:

```csharp
server.SetAuthenticationMethods(
    AMNetSshAuthenticationMethods.Gssapi);
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

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| All logins rejected | Authenticator deny-by-default | Configure at least one authenticator with `SetPasswordAuthenticator()` or equivalent |
| Public key auth fails | Wrong key format | Use OpenSSH format (`ssh-rsa AAA...`), not `-----BEGIN RSA PRIVATE KEY-----` |
| MFA stops working mid-session | Auth method order matters | Configure methods **before** starting the server |
| Delegate authenticator not called | Wrong method used | Use `SetDelegatePasswordAuthenticator()`, not `SetPasswordAuthenticator()` with a custom class |

See [AuthenticationServer](../../Sample/AuthenticationServer/) for runnable examples of every auth mode.

---

**Next:** [Virtual Filesystem](04-virtual-filesystem.md) — root jail isolation, user home directories, and path containment.

---

*ApacheMinaSSHD.NET is maintained by **SERALYNX LLC**. For a turnkey portable SFTP server with enterprise authentication, monitoring, and GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
