# SFTP Subsystem

The SFTP subsystem handles file transfer operations over the SSH protocol. ApacheMinaSSHD.NET exposes every SFTP operation as a .NET hook for custom policy, audit, and monitoring.

## Basic Setup

```csharp
var sftp = new AMNetSftpSubsystemFactory();
server.setSubsystemFactories(sftp);
```

## Event Listeners

Monitor all SFTP activity:

```csharp
class AuditSftpListener : AMNetSftpEventListener
{
    public override void OnOpen(ISshEvent e)
    {
        Log($"File opened: {e.SshHandle?.File} by {e.Session.RemoteAddress}");
    }

    public override void OnRead(ISshReadWrite e)
    {
        Log($"Read {e.Length} bytes from {e.SshHandle?.File}");
    }

    public override void OnWrite(ISshReadWrite e)
    {
        Log($"Wrote {e.Length} bytes to {e.SshHandle?.File}");
    }

    public override void OnRemove(ISshEvent e)
    {
        Log($"File removed by {e.Session.RemoteAddress}");
    }

    public override void OnRename(ISshMove e)
    {
        Log($"File renamed: {e.SourcePath} -> {e.DestPath}");
    }

    public override void OnCreateDirectory(ISshEvent e)
    {
        Log($"Directory created: {e.SshHandle?.File}");
    }
}

sftp.addSftpEventListener(new AuditSftpListener());
```

Available events:

| Event | Description |
|-------|-------------|
| `OnInitialized` | SFTP session initialized |
| `OnOpen` / `OnOpening` | File open |
| `OnClose` / `OnClosing` | File close |
| `OnRead` / `OnReading` | File read |
| `OnWrite` / `OnWriting` | File write |
| `OnCreate` / `OnCreating` | File/directory creation |
| `OnRemove` / `OnRemoving` | File/directory removal |
| `OnRename` / `OnMoving` / `OnMoved` | File rename/move |
| `OnLink` / `OnLinking` | Symlink operations |
| `OnReadEntries` / `OnReadingEntries` | Directory listing |

## File System Accessor

The file system accessor is the central policy hook for all SFTP file operations.

### Path Resolution

```csharp
class PolicyAccessor : AMNetSftpFileSystemAccessor
{
    public override string ResolveLocalFilePath(
        ISshFileSystemAccess context, string resolvedLocalPath)
    {
        // Redirect certain paths
        if (resolvedLocalPath.Contains("incoming"))
            return resolvedLocalPath.Replace("incoming", "quarantine");

        return resolvedLocalPath;
    }
}
```

### Path Allow-Listing

```csharp
public override bool IsPathAllowed(ISshFileSystemAccess context)
{
    // Block write operations to system directories
    if (context.Operation == SshFileSystemOperation.WriteFile &&
        context.LocalPath?.Contains("system") == true)
        return false;

    return base.IsPathAllowed(context);
}
```

### File Attribute Control

```csharp
public override IReadOnlyDictionary<string, object> ResolveReportedFileAttributes(
    ISshFileSystemAccess context,
    IReadOnlyDictionary<string, object> resolvedAttributes)
{
    // Mask real file sizes (useful for sensitive files)
    if (context.LocalPath?.Contains("confidential") == true)
    {
        var masked = new Dictionary<string, object>(resolvedAttributes);
        masked["size"] = 0L;
        return masked;
    }

    return base.ResolveReportedFileAttributes(context, resolvedAttributes);
}
```

### Operation Hooks

Override any SFTP operation:

```csharp
public override void OpenFile(ISshFileSystemAccess context) { ... }
public override void CloseFile(ISshFileSystemAccess context) { ... }
public override void CreateDirectory(ISshFileSystemAccess context) { ... }
public override void RemoveFile(ISshFileSystemAccess context) { ... }
public override void RenameFile(ISshFileSystemAccess context) { ... }
public override void CopyFile(ISshFileSystemAccess context) { ... }
public override void CreateLink(ISshFileSystemAccess context) { ... }
public override void SetFilePermissions(ISshFileSystemAccess context) { ... }
public override void SetFileOwner(ISshFileSystemAccess context) { ... }
```

Each hook receives an `ISshFileSystemAccess` context with:

| Property | Description |
|----------|-------------|
| `Session` | Client session info (address, ID) |
| `Operation` | The SFTP operation being performed |
| `LocalPath` | Resolved local filesystem path |
| `RemotePath` | Original remote path from client |
| `RootPath` | User's home directory root |
| `SourcePath` | Source path (for rename/copy) |
| `DestinationPath` | Destination path (for rename/copy) |
| `IsDirectory` | Whether the operation targets a directory |
| `IsSymbolicLink` | Whether the operation targets a symlink |
| `Options` | SFTP option flags |
| `Attributes` | File attributes |

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| Events not firing | Listener not registered | Call `addSftpEventListener()` before `Start()` |
| File operations silently failing | `IsPathAllowed` rejecting paths | Override `IsPathAllowed` to log rejections for debugging |
| Custom accessor not used | Wrong factory instance | Ensure the same `AMNetSftpSubsystemFactory` instance is passed to `setSubsystemFactories()` and has the accessor set |

See [SftpEventServer](../../Sample/SftpEventServer/) for a complete event-driven implementation.

---

**Next:** [SCP Subsystem](https://github.com/PortaSFTPServer/ApacheMinaSSHD.NET/blob/main/docs/guide/06-scp-subsystem.md) — SCP file transfer setup and event handling.

---

*ApacheMinaSSHD.NET is built by **SERALYNX LLC**. For a complete portable SFTP server solution with GUI management, see [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
