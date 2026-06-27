# PROXY Protocol Support

PROXY protocol preserves the real client IP address when your SFTP server sits behind a load balancer or reverse proxy (HAProxy, Nginx, AWS NLB, Azure LB). ApacheMinaSSHD.NET supports PROXY protocol v1 (text) and v2 (binary) out of the box.

## When to Use PROXY Protocol

Without PROXY protocol, every connection appears to come from the load balancer's IP. Enable it when:

- Your server runs behind HAProxy, Nginx, or a cloud load balancer
- You need real client IPs for authentication, audit logging, or rate limiting
- Your load balancer supports `send-proxy` or `send-proxy-v2`

Direct SSH connections (no load balancer) are handled automatically — no configuration change needed.

## Basic Setup

```csharp
server.setServerProxyAcceptor(new AMNetServerProxyAcceptor());
```

Place this before `server.Start()`. The default acceptor handles all supported connection types.

## Supported Connection Types

The `AMNetServerProxyAcceptor` automatically detects three scenarios:

| Type | Detection | Handling |
|------|-----------|----------|
| Direct SSH | Starts with `SSH-` | Passes through untouched |
| PROXY v1 | Text: `PROXY TCP4 ...` or `PROXY TCP6 ...` | Parses client IP and port, advances past header |
| PROXY v2 | Binary signature `\r\n\r\n\0\r\nQUIT\n` | Parses IPv4/IPv6 address family, handles LOCAL health probes |
| LOCAL (health check) | v2 command byte `0x00` | Binds to `127.0.0.1:0`, returns `true` |
| Malformed | Anything unrecognized | Calls `ForceDisconnect` and throws |

## Custom Proxy Acceptor

Extend `AMNetServerProxyAcceptor` to inspect or log metadata before delegating to the default handler:

```csharp
class AuditProxyAcceptor : AMNetServerProxyAcceptor
{
    public override bool acceptServerProxyMetadata(IProxyMetadata proxyMetadata)
    {
        Console.WriteLine($"[Proxy] Connection from {proxyMetadata.RemoteAddress}");

        // Access pre-handshake data
        var header = proxyMetadata.GetHeader();
        if (header != null)
            Console.WriteLine($"[Proxy] Header: {header}");

        return base.acceptServerProxyMetadata(proxyMetadata);
    }
}

server.setServerProxyAcceptor(new AuditProxyAcceptor());
```

## Inspecting Proxy Metadata

The `IProxyMetadata` interface provides access to connection details at the proxy layer:

| Member | Description |
|--------|-------------|
| `RemoteAddress` | The raw remote endpoint (load balancer IP before parsing) |
| `LocalAddress` | The local server endpoint |
| `AvailableBytes` | Bytes available in the pre-handshake buffer |
| `GetHeader()` | Text header snapshot when available |
| `GetRawBytes()` | Raw pre-handshake bytes (non-destructive) |
| `GetHostname()` | Remote hostname when resolvable |
| `ForceDisconnect(reason)` | Reject the connection with a reason |

The real client address (after PROXY header parsing) is automatically set on the underlying SSH session — not on `IProxyMetadata.RemoteAddress` (which always reflects the raw load balancer endpoint).

## Load Balancer Configuration

### HAProxy

```haproxy
frontend sftp
    bind *:22
    mode tcp
    default_backend sftp_servers

backend sftp_servers
    mode tcp
    server sftp1 10.0.1.10:22 send-proxy-v2
    server sftp2 10.0.1.11:22 send-proxy-v2
```

### Nginx Stream Module

```nginx
stream {
    server {
        listen 22;
        proxy_pass 10.0.1.10:22;
        proxy_protocol on;
    }
}
```

### AWS NLB

Enable PROXY protocol v2 on the target group. The default acceptor handles AWS NLB health check probes (LOCAL command) automatically.

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| All connections rejected with "Protocol violation" | Load balancer not sending PROXY header but acceptor is enabled | Remove `setServerProxyAcceptor()` for direct connections, or configure load balancer to send PROXY protocol |
| Intermittent "fragmented frame" errors | TCP packet fragmentation during TLS/NAT | Acceptor automatically retries on next packet — usually self-resolving |
| Health checks failing | Load balancer health probes rejected | v2 LOCAL probes are handled automatically; ensure your LB sends v2, not v1 |
| `InvalidOperationException: Malformed transport` | Non-SSH traffic hitting the port | Verify the load balancer and clients speak SSH or PROXY protocol |
| Client IP always shows load balancer IP | PROXY protocol not enabled on load balancer | Add `send-proxy-v2` (HAProxy) or `proxy_protocol on` (Nginx) to the backend config |

## Sample

See [SessionMonitorServer](../../Sample/SessionMonitorServer/) for a complete example with custom proxy acceptor, session lifecycle monitoring, and connection auditing.

---

**Next:** See [Production Deployment](09-production-deployment.md) for high-availability configurations with PROXY protocol.

---

*ApacheMinaSSHD.NET is developed by **SERALYNX LLC**. For a production-ready portable SFTP server with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*

