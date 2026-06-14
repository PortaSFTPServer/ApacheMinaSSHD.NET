# Virtual Filesystem & Root Jail Isolation

Secure file transfer starts with filesystem isolation. ApacheMinaSSHD.NET provides a virtual filesystem layer where each user is confined to their own home directory — a root jail they cannot escape.

## Basic Setup

```csharp
string basePath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
var fsFactory = new AMNetVirtualFileSystemFactory(basePath);
server.setFileSystemFactory(fsFactory);
```

Each authenticated user gets access to `sftp-root/{username}`. If the directory does not exist and `createUserDirectory` is `true` (the default), it is created automatically.

## Custom User Home Resolution

Override how usernames map to directories:

```csharp
class TenantFileSystemFactory : AMNetVirtualFileSystemFactory
{
    public TenantFileSystemFactory(string basePath)
        : base(basePath) { }

    public override string ResolveUserHomeDirectory(string username)
    {
        // Group users by first letter for directory distribution
        string prefix = username.Length > 0
            ? username[0].ToString().ToLower()
            : "_";
        return Path.Combine(BasePath, prefix, username);
    }
}

server.setFileSystemFactory(new TenantFileSystemFactory("/sftp/data"));
```

Usernames are automatically sanitized to prevent path traversal.

## Path Traversal Protection

The virtual filesystem factory sanitizes usernames by stripping `..`, `/`, `\`, and `:` characters, ensuring users cannot escape their jail via crafted usernames.

At the SFTP level, every file path is validated against the root directory using layered containment checks:

1. **Java VirtualFileSystemFactory** — Apache MINA SSHD's built-in jail
2. **IsPathAllowed** — .NET policy hook verifies the resolved path is within root
3. **Symlink containment** — follows reparse points and junction targets

## Symlink Containment

ApacheMinaSSHD.NET includes comprehensive symlink attack prevention:

```csharp
// Automatic — no configuration needed
// The server validates that symlink targets stay within the user's root
```

The containment system uses three layers:

1. **Java NIO** — `Path.toRealPath()` resolves and checks containment
2. **Windows Native** — `FindFirstFile`/`DeviceIoControl` detects reparse points
3. **.NET** — `File.ResolveLinkTarget` provides final verification

## Directory Entry Filtering

Control which files and folders are visible to users:

```csharp
class SecureFileAccessor : AMNetSftpFileSystemAccessor
{
    public override bool ShouldIncludeDirectoryEntry(ISshFileSystemAccess context)
    {
        string name = Path.GetFileName(context.LocalPath);
        if (string.IsNullOrWhiteSpace(name)) return true;

        // Hide system files
        if (name.StartsWith(".")) return false;

        // Hide specific extensions
        string ext = Path.GetExtension(name);
        if (string.Equals(ext, ".log", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ext, ".tmp", StringComparison.OrdinalIgnoreCase))
            return false;

        return base.ShouldIncludeDirectoryEntry(context);
    }
}
```

Apply it:

```csharp
var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new SecureFileAccessor());
server.setSubsystemFactories(sftp);
```

The same filter applies to SCP:

```csharp
class SecureScpOpener : AMNetScpFileOpener
{
    public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
    {
        string name = Path.GetFileName(access.LocalPath);
        if (name != null && name.StartsWith(".")) return false;
        return base.ShouldIncludeDirectoryEntry(access);
    }
}
```

## Hidden Files

By default, the following are hidden from directory listings:

- Files and directories starting with `.` (dot-files)
- Files and directories named `secret_data` (configurable via `HiddenNames`)
- Files with the `Hidden` attribute set on Windows

Override the default hidden names:

```csharp
class CustomAccessor : AMNetSftpFileSystemAccessor
{
    protected override IReadOnlySet<string> HiddenNames { get; }
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "secret_data",
            "backup_config",
            ".git"
        };
}
```

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| Users see each other's files | Root jail not configured | Always set `AMNetVirtualFileSystemFactory` via `setFileSystemFactory()` |
| Path traversal works | `createUserDirectory` with unsanitized username | Username sanitization is automatic — but verify `ResolveUserHomeDirectory` does not introduce traversal in custom overrides |
| Dot-files visible | Hidden names not configured | Override `HiddenNames` or `ShouldIncludeDirectoryEntry` to hide `.git`, `.env`, etc. |

See [VirtualFileSystemServer](../../Sample/VirtualFileSystemServer/) for a complete root jail implementation.

---

**Next:** [SFTP Subsystem](05-sftp-subsystem.md) — file operations, event hooks, and access control.

---

*ApacheMinaSSHD.NET is developed by **SERALYNX LLC** — securing file transfer for critical infrastructure. Try [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/) for a complete portable solution with GUI management.*
