// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();
server.SetFixedPasswordAuthenticator("admin", "changeme");
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser"));

// Virtual file system rooted at a specific directory
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(@"C:\sftp-root"));

// SFTP with custom file system accessor that filters files
var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new FilteredFileAccessor());
server.setSubsystemFactories(sftp);

// SCP with the same filtering
server.setCommandFactory(new AMNetScpCommandFactory(new FilteredScpFileOpener(@"C:\sftp-root")));

server.Start();
Console.WriteLine("VirtualFileSystemServer on port 2222. Press Enter to stop.");
Console.ReadLine();
server.Stop();

class FilteredFileAccessor : AMNetSftpFileSystemAccessor
{
    public override bool ShouldIncludeDirectoryEntry(ISshFileSystemAccess context)
    {
        var name = Path.GetFileName(context.RemotePath);
        if (string.IsNullOrWhiteSpace(name)) return true;
        if (name.StartsWith('.')) return false;
        if (name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)) return false;
        if (name.EndsWith(".log", StringComparison.OrdinalIgnoreCase)) return false;
        if (string.Equals(name, "secret", StringComparison.OrdinalIgnoreCase)) return false;
        return base.ShouldIncludeDirectoryEntry(context);
    }

    public override string ResolveLocalFilePath(ISshFileSystemAccess context, string resolvedLocalPath)
    {
        Console.WriteLine($"[SFTP] Access: {context.RemotePath} -> {resolvedLocalPath}");
        return base.ResolveLocalFilePath(context, resolvedLocalPath);
    }

    public override bool IsPathAllowed(ISshFileSystemAccess context)
    {
        var allowed = base.IsPathAllowed(context);
        if (!allowed) Console.WriteLine($"[SFTP] Blocked: {context.RemotePath}");
        return allowed;
    }
}

class FilteredScpFileOpener : AMNetScpFileOpener
{
    public FilteredScpFileOpener(string rootPath) : base(rootPath) { }

    public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
    {
        var name = Path.GetFileName(access.LocalPath);
        if (name != null && name.StartsWith('.')) return false;
        return base.ShouldIncludeDirectoryEntry(access);
    }

    public override void OpenRead(ISshScpFileAccess access)
    {
        Console.WriteLine($"[SCP] Reading: {access.LocalPath}");
    }

    public override void OpenWrite(ISshScpFileAccess access)
    {
        Console.WriteLine($"[SCP] Writing: {access.LocalPath}");
    }
}
