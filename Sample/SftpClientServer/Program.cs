using System.Security.Cryptography;
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Renci.SshNet;
using Renci.SshNet.Sftp;

var rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
var hostKeyPath = Path.Combine(AppContext.BaseDirectory, "hostkey.ser");
Directory.CreateDirectory(rootPath);

// ── Start server ──────────────────────────────────────────
var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 0; // OS-assigned port
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();
server.SetFixedPasswordAuthenticator("demo", "demo");
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(hostKeyPath));
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(rootPath));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());
server.setCommandFactory(new AMNetScpCommandFactory());
server.Start();

var sshPort = server.Port;
Console.WriteLine($"SFTP/SCP server listening on 127.0.0.1:{sshPort}");
Console.WriteLine();

// ── Create a test file for uploads ────────────────────────
var uploadDir = Directory.CreateDirectory(Path.Combine(rootPath, "upload-test"));
var localTemp = Path.Combine(Path.GetTempPath(), $"sftp-test-{Guid.NewGuid()}.bin");
var original = RandomNumberGenerator.GetBytes(16 * 1024);
File.WriteAllBytes(localTemp, original);
Console.WriteLine($"Test file: {localTemp} ({original.Length} bytes, SHA256={Convert.ToHexString(SHA256.HashData(original))})");
Console.WriteLine();

// ══════════════════════════════════════════════════════════
// SFTP CLIENT DEMO
// ══════════════════════════════════════════════════════════
Console.WriteLine("═══ SFTP Client ═══");
using (var sftp = new SftpClient("127.0.0.1", sshPort, "demo", "demo"))
{
    sftp.Connect();
    Console.WriteLine($"Connected via SFTP (protocol v{sftp.ProtocolVersion})");

    // List root directory
    var entries = sftp.ListDirectory("/").ToList();
    Console.WriteLine($"Root contains {entries.Count} entries:");
    foreach (var e in entries.OrderBy(e => e.Name))
    {
        Console.WriteLine($"  {(e.IsDirectory ? "DIR" : "FILE")} {e.Name}  {e.Length} B");
    }

    // Upload file
    var remotePath = $"/upload-test/sample-{Guid.NewGuid()}.bin";
    await using (var fs = File.OpenRead(localTemp))
    {
        sftp.UploadFile(fs, remotePath);
    }
    var stat = sftp.Get(remotePath);
    Console.WriteLine($"Uploaded via SFTP: {remotePath} ({stat.Length} bytes)");

    // Download and verify
    var downloadTarget = Path.Combine(Path.GetTempPath(), $"sftp-download-{Guid.NewGuid()}.bin");
    await using (var fs = File.Create(downloadTarget))
    {
        sftp.DownloadFile(remotePath, fs);
    }
    var downloaded = File.ReadAllBytes(downloadTarget);
    var match = CryptographicOperations.FixedTimeEquals(original, downloaded);
    Console.WriteLine($"Downloaded and verified: {match}");

    // Clean up downloaded copy
    File.Delete(downloadTarget);

    sftp.Disconnect();
}
Console.WriteLine();

// ══════════════════════════════════════════════════════════
// SCP CLIENT DEMO
// ══════════════════════════════════════════════════════════
Console.WriteLine("═══ SCP Client ═══");
using (var scp = new ScpClient("127.0.0.1", sshPort, "demo", "demo"))
{
    scp.Connect();
    Console.WriteLine("Connected via SCP");

    // Upload file via SCP
    var scpRemotePath = $"/upload-test/scp-sample-{Guid.NewGuid()}.bin";
    using (var fs = File.OpenRead(localTemp))
    {
        scp.Upload(fs, scpRemotePath);
    }
    Console.WriteLine($"Uploaded via SCP: {scpRemotePath}");

    // Download and verify
    var scpDownload = Path.Combine(Path.GetTempPath(), $"scp-download-{Guid.NewGuid()}.bin");
    using (var fs = File.Create(scpDownload))
    {
        scp.Download(scpRemotePath, fs);
    }
    var scpData = File.ReadAllBytes(scpDownload);
    var scpMatch = CryptographicOperations.FixedTimeEquals(original, scpData);
    Console.WriteLine($"Downloaded via SCP and verified: {scpMatch}");

    File.Delete(scpDownload);
    scp.Disconnect();
}
Console.WriteLine();

// ── Cleanup ───────────────────────────────────────────────
File.Delete(localTemp);
Directory.Delete(uploadDir.FullName, recursive: true);
server.Stop();
Console.WriteLine("Done.");
