// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Security.Cryptography;
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using DareSftpServer;

var baseDir = Path.Combine(AppContext.BaseDirectory, "dare-data");
var encryptedDir = Path.Combine(baseDir, "encrypted-store");
var stagingDir = Path.Combine(baseDir, "staging");
var userDir = Path.Combine(stagingDir, "admin");

EnsureDirectory(encryptedDir);
EnsureDirectory(stagingDir);
EnsureDirectory(userDir);

Console.Error.WriteLine("""
WARNING: This sample uses a hardcoded master key derivation password for DEMO purposes only.
In production:
  1. Use a Key Management Service (KMS) or hardware security module (HSM).
  2. Store the master key in a secure vault, not in source code.
  3. Persist the PBKDF2 salt alongside the encrypted data.
  4. Rotate keys periodically.
Press Ctrl+C to abort, or wait 3 seconds to continue...
""");
Thread.Sleep(3000);

var masterKey = DeriveMasterKey("change-me-in-production-use-kms-instead");
using var crypto = new DareEncryptionService(masterKey);

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();
server.Config.WELCOME_BANNER = BuildBanner();
server.SetFixedPasswordAuthenticator("admin", "changeme");
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser"));

server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(stagingDir));

var sftp = new AMNetSftpSubsystemFactory();
var accessor = new DareSftpAccessor(encryptedDir, stagingDir);
sftp.setFileSystemAccessor(accessor);
sftp.addSftpEventListener(new DareEventListener(accessor, crypto, stagingDir, encryptedDir));
server.setSubsystemFactories(sftp);

server.setCommandFactory(new AMNetScpCommandFactory(new DareScpFileOpener(stagingDir, crypto)));

server.Start();

Console.WriteLine($"  Encrypted store : {Path.GetFullPath(encryptedDir)}");
Console.WriteLine($"  Staging area    : {Path.GetFullPath(stagingDir)}");
Console.WriteLine($"  Chunk size      : {crypto.ChunkSize:N0} bytes");
Console.WriteLine($"  Cipher          : AES-256-GCM");
Console.WriteLine($"  Mode            : Transparent encryption at rest");
Console.WriteLine($"  Notes           : Files served plaintext from staging;");
Console.WriteLine($"                    disk persistence is encrypted via DARE.");
Console.WriteLine();
Console.WriteLine("  Connect: sftp admin@localhost -P 2222");
Console.WriteLine("  Password: changeme");
Console.WriteLine();
Console.WriteLine("  After upload, encrypted .dare file is in encrypted-store/.");
Console.WriteLine("  Interrupt with Ctrl+C to stop.");
Console.WriteLine(new string('=', 60));

var stopEvent = new ManualResetEventSlim(false);
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nShutting down...");
    stopEvent.Set();
};

stopEvent.Wait();
server.Stop();
Console.WriteLine("Server stopped.");

static byte[] DeriveMasterKey(string password)
{
    var salt = new byte[16];
    RandomNumberGenerator.Fill(salt);
    return Rfc2898DeriveBytes.Pbkdf2(
        password, salt, 600_000, HashAlgorithmName.SHA256, 32);
}

static void EnsureDirectory(string path)
{
    if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
}

static string BuildBanner()
{
    return """
______  _____________________________  ______  _   ________________
___  / / /___  _/__  __/__  __ \__   |_  / / / /  / /_  __ \__  __/
__  /_/ / __  / __  /  _  / / /_  /|  / /_/ / /   __  /_/ /_  /
_  __  / __/ /  _  /   / /_/ // /_/  /  __  /  / /_  ____/_  /_
/_/ /_/  /___/  /_/    \____/ \____/   /_/ /_/   \__/_/    \___/

  Data At Rest Encryption SFTP Server
  SERALYNX LLC (Porta SFTP Server)
  ApacheMinaSSHD.NET
""";
}

sealed class DareEventListener : AMNetSftpEventListener
{
    private readonly DareSftpAccessor _accessor;
    private readonly DareEncryptionService _crypto;
    private readonly string _stagingDir;
    private readonly string _encryptedDir;

    public DareEventListener(
        DareSftpAccessor accessor,
        DareEncryptionService crypto,
        string stagingDir,
        string encryptedDir)
    {
        _accessor = accessor;
        _crypto = crypto;
        _stagingDir = stagingDir;
        _encryptedDir = encryptedDir;
    }

    public override void OnInitialized(ISshSession session, int version)
    {
        DareSftpAccessor.Log($"SFTP session v{version} from {session.RemoteAddress}");
    }

    public override void OnOpen(ISshEvent ctx)
    {
        var name = Path.GetFileName(ctx.SshHandle?.PhysicalPath) ?? "unknown";
        DareSftpAccessor.Log($"SFTP Open: {name}");
    }

    public override void OnWrite(ISshReadWrite ctx)
    {
        _accessor.TrackWrite(ctx.Length);
    }

    public override void OnRead(ISshReadWrite ctx)
    {
        _accessor.TrackRead(ctx.Length);
    }

    public override void OnClosed(ISshEvent ctx)
    {
        var name = Path.GetFileName(ctx.SshHandle?.PhysicalPath) ?? "unknown";
        DareSftpAccessor.Log($"SFTP Close: {name}");
    }

    public override void OnOpenFailed(ISshIOFailure ctx)
    {
        DareSftpAccessor.Log($"SFTP Open failed: {ctx.LocalPath} - {ctx.Exception?.Message}");
    }
}

sealed class DareScpFileOpener : AMNetScpFileOpener
{
    private readonly DareEncryptionService _crypto;

    public DareScpFileOpener(string rootPath, DareEncryptionService crypto)
        : base(rootPath)
    {
        _crypto = crypto;
    }

    public override void OpenRead(ISshScpFileAccess access)
    {
        DareSftpAccessor.Log($"SCP Read: {access.LocalPath}");
    }

    public override void OpenWrite(ISshScpFileAccess access)
    {
        DareSftpAccessor.Log($"SCP Write: {access.LocalPath}");
    }
}
