using System.Text.Json;
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

var config = LoadConfig("appsettings.json");

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = config.host;
server.Port = config.port;
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();

server.Config.MAX_CONCURRENT_SESSIONS = config.limits.maxSessions;
server.Config.MAX_CONCURRENT_CHANNELS = config.limits.maxChannels;
server.Config.IDLE_TIMEOUT = TimeSpan.FromSeconds(config.limits.idleTimeoutSeconds);
server.Config.AUTH_TIMEOUT = TimeSpan.FromSeconds(config.limits.authTimeoutSeconds);
server.Config.MAX_AUTH_REQUESTS = config.limits.maxAuthRequests;
server.Config.WELCOME_BANNER = config.banner;

ApplyAlgorithmConfig(server, config);

server.setKeyPairProvider(GenerateHostKey(config.hostKeyPath));
server.SetFixedPasswordAuthenticator(config.auth.username, config.auth.password);

EnsureDirectory(config.sftpRoot);
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(config.sftpRoot));

var sftp = new AMNetSftpSubsystemFactory();
sftp.setFileSystemAccessor(new ProductionFileAccessor());
sftp.addSftpEventListener(new ProductionSftpEventListener());
server.setSubsystemFactories(sftp);

var scp = new AMNetScpCommandFactory(new ProductionScpFileOpener(config.sftpRoot));
scp.addEventListener(new ProductionScpTransferListener());
server.setCommandFactory(scp);

server.addSessionListener(new ProductionSessionListener());
server.setIoServiceEventListener(new ProductionIoServiceEventListener());

server.Start();
Console.WriteLine($"Production SFTP server running on {config.host}:{config.port}");
Console.WriteLine($"  Root: {Path.GetFullPath(config.sftpRoot)}");
Console.WriteLine("Press Enter to stop.");
Console.ReadLine();
server.Stop();

static ServerConfig LoadConfig(string path)
{
    var text = File.ReadAllText(path);
    return JsonSerializer.Deserialize<ServerConfig>(text)
        ?? throw new InvalidOperationException("Failed to load config");
}

static AMNetSimpleGeneratorHostKeyProvider GenerateHostKey(string path)
{
    var provider = new AMNetSimpleGeneratorHostKeyProvider(path);
    provider.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
    provider.setKeySize(4096);
    return provider;
}

static void ApplyAlgorithmConfig(AMNetSshServer server, ServerConfig config)
{
    if (config.algorithms.ciphers?.Count > 0)
        server.Config.SetCiphers(config.algorithms.ciphers);
    if (config.algorithms.macs?.Count > 0)
        server.Config.SetMacs(config.algorithms.macs);
    if (config.algorithms.keyExchange?.Count > 0)
        server.Config.SetKeyExchangeAlgorithms(config.algorithms.keyExchange);
    if (config.algorithms.hostKeys?.Count > 0)
        server.Config.SetHostKeyAlgorithms(config.algorithms.hostKeys);
}

static void EnsureDirectory(string path)
{
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
}

record ServerConfig(
    string host,
    int port,
    string sftpRoot,
    string hostKeyPath,
    AuthConfig auth,
    LimitsConfig limits,
    AlgorithmConfig algorithms,
    string banner);

record AuthConfig(string username, string password);
record LimitsConfig(
    int maxSessions,
    int maxChannels,
    int idleTimeoutSeconds,
    int authTimeoutSeconds,
    int maxAuthRequests);
record AlgorithmConfig(
    List<string>? ciphers,
    List<string>? macs,
    List<string>? keyExchange,
    List<string>? hostKeys);

class ProductionFileAccessor : AMNetSftpFileSystemAccessor
{
    public override bool ShouldIncludeDirectoryEntry(ISshFileSystemAccess context)
    {
        var name = Path.GetFileName(context.RemotePath);
        if (string.IsNullOrWhiteSpace(name)) return true;
        if (name.StartsWith('.')) return false;
        if (name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)) return false;
        return base.ShouldIncludeDirectoryEntry(context);
    }

    public override bool IsPathAllowed(ISshFileSystemAccess context)
    {
        if (context.RemotePath?.Contains("..") == true) return false;
        return base.IsPathAllowed(context);
    }
}

class ProductionSftpEventListener : AMNetSftpEventListener
{
    public override void OnInitialized(ISshSession sshSession, int version)
    {
        Log($"[SFTP] Session v{version} from {sshSession.RemoteAddress}");
    }

    public override void OnOpen(ISshEvent ctx)
    {
        Log($"[SFTP] Open {ctx.SshHandle.PhysicalPath}");
    }

    public override void OnClosed(ISshEvent ctx)
    {
        Log($"[SFTP] Close {ctx.SshHandle.PhysicalPath}");
    }

    public override void OnWrite(ISshReadWrite ctx)
    {
        Log($"[SFTP] Write {ctx.Length} bytes at offset {ctx.Offset}");
    }

    public override void OnOpenFailed(ISshIOFailure ctx)
    {
        Log($"[SFTP] Open FAILED: {ctx.LocalPath} - {ctx.Exception?.Message}");
    }

    static void Log(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} {message}");
}

class ProductionScpFileOpener : AMNetScpFileOpener
{
    public ProductionScpFileOpener(string rootPath) : base(rootPath) { }

    public override bool IsPathAllowed(ISshScpFileAccess access)
    {
        if (access.RequestedPath?.Contains("..") == true) return false;
        return base.IsPathAllowed(access);
    }

    public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
    {
        var name = Path.GetFileName(access.LocalPath);
        if (name != null && name.StartsWith('.')) return false;
        return base.ShouldIncludeDirectoryEntry(access);
    }
}

class ProductionScpTransferListener : AMNetScpTransferEventListener
{
    public override void OnStartFile(ISshScpTransferEvent context)
    {
        Console.WriteLine($"[SCP] {(context.Operation == "0" ? "Download" : "Upload")}: {context.Path}");
    }

    public override void OnEndFile(ISshScpTransferEvent context)
    {
        Console.WriteLine($"[SCP] Complete: {context.Path}");
    }
}

class ProductionSessionListener : AMNetSessionListener
{
    public override void OnSessionCreated(ISshSession session)
    {
        Console.WriteLine($"[Session] New: {session.RemoteAddress}");
    }

    public override void OnSessionClosed(ISshSession session)
    {
        Console.WriteLine($"[Session] Gone: {session.RemoteAddress}");
    }
}

class ProductionIoServiceEventListener : AMNetIoServiceEventListener
{
    public override bool OnConnectionAccepted(ISshServiceConnection context)
    {
        Console.WriteLine($"[Net] Accept: {context.RemoteEndPoint}");
        return base.OnConnectionAccepted(context);
    }

    public override void OnConnectionAborted(ISshServiceConnection context)
    {
        Console.WriteLine($"[Net] Abort: {context.RemoteEndPoint}");
    }
}
