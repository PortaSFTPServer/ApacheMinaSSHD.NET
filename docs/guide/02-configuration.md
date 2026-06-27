# Server Configuration

ApacheMinaSSHD.NET exposes all SSH server properties through the `AMNetSshServerConfig` class, accessible via `server.Config`.

## Production Baseline

Apply conservative production limits with a single call:

```csharp
server.Config.ApplyProductionDefaults();
```

This sets:

| Setting | Default | Description |
|---------|---------|-------------|
| `MAX_AUTH_REQUESTS` | 5 | Authentication attempts per session |
| `AUTH_TIMEOUT` | 60s | Max time to complete authentication |
| `MAX_CONCURRENT_SESSIONS` | 10 | Active sessions per username |
| `MAX_CONCURRENT_CHANNELS` | 10 | Channels within one session |
| `IDLE_TIMEOUT` | 10m | Close inactive sessions |
| `HEARTBEAT_INTERVAL` | 45s | Server keep-alive interval |
| `REKEY_BYTES_LIMIT` | 1 GB | Data before key renegotiation |
| `REKEY_TIME_LIMIT` | 1h | Time before key renegotiation |

## Authentication Limits

```csharp
server.Config.MAX_AUTH_REQUESTS = 5;      // Max login attempts
server.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(60); // Login window
```

## Session & Channel Limits

```csharp
server.Config.MAX_CONCURRENT_SESSIONS = 10;  // Per user
server.Config.MAX_CONCURRENT_CHANNELS = 10;  // Per session
```

## Timeouts & Keep-Alives

```csharp
server.Config.IDLE_TIMEOUT = TimeSpan.FromMinutes(10);
server.Config.HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(45);
```

## Cryptographic Algorithms

Apply modern algorithm preferences:

```csharp
server.Config.ApplyModernAlgorithmDefaults();
```

Or configure individually:

```csharp
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

Inspect supported algorithms at runtime:

```csharp
IReadOnlyList<string> ciphers = server.Config.GetSupportedCiphers();
IReadOnlyList<string> macs = server.Config.GetSupportedMacs();
IReadOnlyList<string> kex = server.Config.GetSupportedKeyExchangeAlgorithms();
IReadOnlyList<string> hostKeys = server.Config.GetSupportedHostKeyAlgorithms();
```

## Compression

Configure SSH transport compression:

```csharp
server.Config.SetCompressionAlgorithms(
    AMNetSshAlgorithms.Compression.Zlib,
    AMNetSshAlgorithms.Compression.None);
```

Inspect available compression algorithms:

```csharp
IReadOnlyList<string> compressions = server.Config.GetSupportedCompressionAlgorithms();
```

## Bandwidth & Advanced Properties

Set any server property directly for features not exposed by a dedicated wrapper property (channel window sizes, max packet size, etc.):

```csharp
// Channel window and packet sizes
server.Config.SetProperty("window-size", 2097152);     // 2 MB receive window
server.Config.SetProperty("max-packet-size", 65536);    // 64 KB max packet

// Read them back
long windowSize = server.Config.GetLongProperty("window-size", 1048576);
int packetSize = server.Config.GetIntProperty("max-packet-size", 32768);
```

Available property accessors:

```csharp
server.Config.SetProperty("key", value);
string str = server.Config.GetProperty("key", "default");
int    i   = server.Config.GetIntProperty("key", 0);
long   l   = server.Config.GetLongProperty("key", 0L);
bool   b   = server.Config.GetBoolProperty("key", false);
```

## Server Identification

```csharp
server.Config.WELCOME_BANNER = "Welcome to Porta SFTP Server";
server.Config.SERVER_IDENTIFICATION = "SSH-2.0-PortaSFTP";
```

## Full Configuration Example

```csharp
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();

server.Config.MAX_AUTH_REQUESTS = 3;
server.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(30);
server.Config.IDLE_TIMEOUT = TimeSpan.FromMinutes(5);
server.Config.WELCOME_BANNER = "Authorized access only";
```

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| Algorithms not taking effect | Set after server start | Configure all algorithms **before** calling `Start()` |
| `ApplyModernAlgorithmDefaults()` unavailable | Outdated wrapper version | Update to the latest NuGet package |
| Timeouts triggering during large transfers | `IDLE_TIMEOUT` too low | Set `IDLE_TIMEOUT` high enough for your largest file transfers |

See [ProductionServer](../../Sample/ProductionServer/) for a real-world configuration example.

---

**Next:** [Authentication](03-authentication.md) — password, public key, keyboard-interactive, and multi-factor authentication.

---

*ApacheMinaSSHD.NET is developed by **SERALYNX LLC** — building secure file transfer for critical infrastructure. Deploy [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/) for a turnkey portable solution.*
