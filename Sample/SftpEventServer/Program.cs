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
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(@"C:\sftp-root"));

var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new AuditFileAccessor());
sftp.addSftpEventListener(new AuditSftpEventListener());
server.setSubsystemFactories(sftp);

server.Start();
Console.WriteLine("SFTP event server on port 2222. Press Enter to stop.");
Console.ReadLine();
server.Stop();

class AuditFileAccessor : AMNetSftpFileSystemAccessor
{
    public override string ResolveLocalFilePath(ISshFileSystemAccess context, string resolvedLocalPath)
    {
        Console.WriteLine($"[FS] Resolve: {context.RemotePath} -> {resolvedLocalPath}");
        return base.ResolveLocalFilePath(context, resolvedLocalPath);
    }

    public override bool IsPathAllowed(ISshFileSystemAccess context)
    {
        if (context.RemotePath?.Contains("..") == true) return false;
        return base.IsPathAllowed(context);
    }
}

class AuditSftpEventListener : AMNetSftpEventListener
{
    public override void OnInitialized(ISshSession sshSession, int version)
    {
        Console.WriteLine($"[SFTP] Session initialized (v{version}) from {sshSession.RemoteAddress}");
    }

    public override void OnOpen(ISshEvent ctx)
    {
        Console.WriteLine($"[SFTP] Open: {ctx.SshHandle.PhysicalPath}");
    }

    public override void OnClosed(ISshEvent ctx)
    {
        Console.WriteLine($"[SFTP] Close: {ctx.SshHandle.PhysicalPath}");
    }

    public override void OnRead(ISshReadWrite ctx)
    {
        Console.WriteLine($"[SFTP] Read: offset={ctx.Offset}, length={ctx.Length}");
    }

    public override void OnWrite(ISshReadWrite ctx)
    {
        Console.WriteLine($"[SFTP] Write: offset={ctx.Offset}, length={ctx.Length}");
    }

    public override void OnCreating(ISshPath ctx)
    {
        Console.WriteLine($"[SFTP] Creating: {ctx.Path}");
    }

    public override void OnCreated(ISshPath ctx)
    {
        Console.WriteLine($"[SFTP] Created: {ctx.Path}");
    }

    public override void OnRemoving(ISshPath ctx)
    {
        Console.WriteLine($"[SFTP] Removing: {ctx.Path}");
    }

    public override void OnRemoved(ISshPath ctx)
    {
        Console.WriteLine($"[SFTP] Removed: {ctx.Path}");
    }

    public override void OnMoving(ISshMove ctx)
    {
        Console.WriteLine($"[SFTP] Moving: {ctx.SourcePath} -> {ctx.DestPath}");
    }

    public override void OnMoved(ISshMove ctx)
    {
        Console.WriteLine($"[SFTP] Moved: {ctx.SourcePath} -> {ctx.DestPath}");
    }

    public override void OnDestroying(ISshSession sshSession)
    {
        Console.WriteLine($"[SFTP] Session destroyed");
    }
}
