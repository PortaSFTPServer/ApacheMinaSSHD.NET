# Logging & Monitoring

ApacheMinaSSHD.NET provides configurable logging and event-driven monitoring for production operations.

## Quick Start

The simplest way to add logging:

```csharp
using ApacheMinaSSHD.NET.Wrapper.Logging;

var logger = new AMNetLogger(typeof(MyClass), AMNetLogger.LogLevel.Debug);
logger.Info("SFTP server starting...");
```

## Log Levels

| Level | Usage |
|-------|-------|
| `Trace` | Detailed diagnostic information |
| `Debug` | Debug-level messages |
| `Info` | General operational information |
| `Warn` | Potentially harmful situations |
| `Error` | Error events that might still allow the server to continue |

## SLF4J Integration

The `AMNetLogger` bridges .NET logging to the SLF4J backend used by Apache MINA SSHD. Configuration is thread-safe and runs once:

```csharp
var logger = new AMNetLogger(typeof(MyService), AMNetLogger.LogLevel.Info);
logger.Info("Service initialized");
logger.Warn("Configuration missing, using defaults", new FileNotFoundException());
logger.Error("Connection failed", exception);
```

## Custom Logger Implementation

Implement `IAMNetLogger` for integration with your existing logging framework:

```csharp
using ApacheMinaSSHD.NET.Wrapper.Logging;
using Microsoft.Extensions.Logging;

class MsftLogger : IAMNetLogger
{
    private readonly ILogger _logger;

    public MsftLogger(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void Info(string message) => _logger.LogInformation(message);
    public void Warn(string message, Exception? ex = null)
        => _logger.LogWarning(ex, message);
    public void Error(string message, Exception? ex = null)
        => _logger.LogError(ex, message);
    public void Debug(string message, Exception? ex = null)
        => _logger.LogDebug(ex, message);
    public void Trace(string message, Exception? ex = null)
        => _logger.LogTrace(ex, message);
}
```

## SFTP Audit Events

For detailed operational audit, attach an SFTP event listener:

```csharp
class OperationalAudit : AMNetSftpEventListener
{
    public void OnOpen(ISshEvent e)
    {
        Logger.Info($"OPEN {e.Session.RemoteAddress} {e.SshHandle?.File}");
    }

    public void OnWrite(ISshReadWrite e)
    {
        Logger.Info($"WRITE {e.Session.RemoteAddress} " +
                    $"{e.SshHandle?.File} offset={e.Offset} len={e.Length}");
    }

    public void OnReadEntries(ISshEntries e)
    {
        Logger.Debug($"LIST {e.Session.RemoteAddress} " +
                     $"{e.localHandle?.File} entries={e.Entries?.Count}");
    }
}
```

## Session Monitoring

Track session lifecycle and connection events:

```csharp
class SessionMonitor : AMNetSessionListener
{
    public override void OnSessionCreated(ISshSessionEvent e)
    {
        Logger.Info($"SESSION_CREATED {e.Session.RemoteAddress}");
    }

    public override void OnSessionClosed(ISshSessionEvent e)
    {
        Logger.Info($"SESSION_CLOSED {e.Session.RemoteAddress}");
    }
}

server.addSessionListener(new SessionMonitor());
```

## Connection-Level Events

Monitor I/O service events at the transport layer:

```csharp
server.setIoServiceEventListener(new AMNetIoServiceEventListener());
```

## Java Logging Redirection

For debugging Apache MINA SSHD internals, Java log output can be redirected:

```csharp
class JavaLogRedirect
{
    public JavaLogRedirect(TextBox output)
    {
        var stream = new SshdLoggerStream(output);
        // Java stdout/stderr is redirected to the text box
    }
}
```

## Production Monitoring Checklist

- [ ] Configure `AMNetLogger` with appropriate log level for each environment
- [ ] Attach SFTP event listeners for audit trail on file operations
- [ ] Monitor session lifecycle events for anomaly detection
- [ ] Forward logs to your centralized logging system (ELK, Splunk, etc.)
- [ ] Set up alerts for repeated authentication failures
- [ ] Review `Debug`-level logs during initial deployment, switch to `Info` or `Warn` in production

---

**Next:** [Production Deployment](https://github.com/PortaSFTPServer/ApacheMinaSSHD.NET/blob/main/docs/guide/09-production-deployment.md) — Windows Service, Docker, monitoring, and CI/CD.

---

*ApacheMinaSSHD.NET is maintained by **SERALYNX LLC**. For a turnkey portable SFTP server with built-in logging, monitoring, and GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
