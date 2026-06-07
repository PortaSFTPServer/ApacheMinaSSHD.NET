# SCP Subsystem

ApacheMinaSSHD.NET supports the SCP protocol for secure file copy, providing hooks for path resolution, permission mapping, and transfer events.

## Basic Setup

```csharp
string rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
server.setCommandFactory(new AMNetScpCommandFactory(
    new AMNetScpFileOpener(rootPath)));
```

## Custom SCP File Opener

The `AMNetScpFileOpener` base class provides hooks for every SCP operation:

```csharp
class EnterpriseScpOpener : AMNetScpFileOpener
{
    public EnterpriseScpOpener(string rootPath) : base(rootPath) { }

    // Resolve incoming file paths (uploads)
    public override string ResolveIncomingFilePath(
        ISshScpFileAccess access, string resolvedPath)
    {
        // Route uploads to an "incoming" subdirectory
        return Path.Combine(access.RootPath, "incoming",
            Path.GetFileName(resolvedPath));
    }

    // Resolve outgoing file paths (downloads)
    public override string ResolveOutgoingFilePath(
        ISshScpFileAccess access, string resolvedPath)
    {
        return resolvedPath;
    }

    // Allow or deny paths
    public override bool IsPathAllowed(ISshScpFileAccess access)
    {
        if (access.LocalPath?.Contains("..") == true)
            return false;

        return base.IsPathAllowed(access);
    }
}
```

## Transfer Events

```csharp
class AuditScpListener : AMNetScpTransferEventListener
{
    public override void OnOpenRead(ISshScpTransferEvent e)
    {
        Log($"SCP download started: {e.Path} by {e.Session.RemoteAddress}");
    }

    public override void OnCloseRead(ISshScpTransferEvent e)
    {
        Log($"SCP download completed: {e.Path}");
    }

    public override void OnOpenWrite(ISshScpTransferEvent e)
    {
        Log($"SCP upload started: {e.Path} by {e.Session.RemoteAddress}");
    }

    public override void OnCloseWrite(ISshScpTransferEvent e)
    {
        Log($"SCP upload completed: {e.Path}");
    }
}
```

Register the listener:

```csharp
var scpFactory = new AMNetScpCommandFactory(
    new EnterpriseScpOpener(rootPath));
scpFactory.addScpTransferEventListener(new AuditScpListener());
server.setCommandFactory(scpFactory);
```

## File Permissions

SCP sends file permission information during transfers. The opener can map permissions:

```csharp
public override IReadOnlyList<string> GetLocalFilePermissions(
    ISshScpFileAccess access, IReadOnlyList<string> permissions)
{
    // Ensure uploaded files are not executable by default
    return permissions
        .Where(p => !p.Contains("EXECUTE"))
        .ToList();
}
```

## Directory Filtering

Control directory contents visible over SCP:

```csharp
public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
{
    string name = Path.GetFileName(access.LocalPath);
    if (name != null && name.StartsWith(".")) return false;
    return base.ShouldIncludeDirectoryEntry(access);
}
```

## Combined SFTP + SCP Setup

Most deployments enable both protocols:

```csharp
string rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");

// SFTP
var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new SecureFileAccessor());
sftp.addSftpEventListener(new AuditSftpListener());
server.setSubsystemFactories(sftp);

// SCP
server.setCommandFactory(new AMNetScpCommandFactory(
    new EnterpriseScpOpener(rootPath)));
```

---

**Next:** [Security Best Practices](https://github.com/PortaSFTPServer/ApacheMinaSSHD.NET/blob/main/docs/guide/07-security.md) — hardening your server for production.

---

*ApacheMinaSSHD.NET is maintained by **SERALYNX LLC**. For a turnkey portable SFTP server with built-in SCP support and GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
