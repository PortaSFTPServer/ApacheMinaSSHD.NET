using System.Text;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Integration")]
public class SftpScpIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _homeDir;
    private readonly AMNetSshServer _server;
    private readonly int _port;

    public SftpScpIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SftpScpIntegration_" + Guid.NewGuid());
        _homeDir = Path.Combine(_tempDir, "home");
        Directory.CreateDirectory(_homeDir);

        _server = AMNetSshServer.SetUpDefaultServer();
        _server.Host = "127.0.0.1";
        _server.Port = 0;
        _server.Config.ApplyProductionDefaults();
        _server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
            Path.Combine(_tempDir, "hostkey.ser")));
        _server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(_homeDir));
        _server.SetFixedPasswordAuthenticator("testuser", "testpass");

        var sftp = new AMNetSftpSubsystemFactory();
        _server.setSubsystemFactories(sftp);
        _server.setCommandFactory(new AMNetScpCommandFactory());
        _server.Start();

        _port = _server.Port;
    }

    public void Dispose()
    {
        try
        {
            _server.Stop(true);
        }
        catch { }
        _server.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void Sftp_upload_file()
    {
        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            using var ms = new MemoryStream("Hello SFTP from test"u8.ToArray());
            client.UploadFile(ms, "/uploaded-test.txt");
            var destFile = Path.Combine(_homeDir, "testuser", "uploaded-test.txt");
            Assert.True(File.Exists(destFile));
            Assert.Equal("Hello SFTP from test", File.ReadAllText(destFile));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_download_file()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "download-me.txt"), "Content for download");

        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            using var ms = new MemoryStream();
            client.DownloadFile("/download-me.txt", ms);
            ms.Position = 0;
            Assert.Equal("Content for download", new StreamReader(ms).ReadToEnd());
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_list_directory()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "file1.txt"), "one");
        File.WriteAllText(Path.Combine(userDir, "file2.txt"), "two");
        Directory.CreateDirectory(Path.Combine(userDir, "subdir"));

        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            var files = client.ListDirectory("/");
            var names = files.Select(f => f.Name).OrderBy(n => n).ToList();
            Assert.Contains("file1.txt", names);
            Assert.Contains("file2.txt", names);
            Assert.Contains("subdir", names);
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_create_and_remove_directory()
    {
        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            client.CreateDirectory("/newdir");
            var dirPath = Path.Combine(_homeDir, "testuser", "newdir");
            Assert.True(Directory.Exists(dirPath));

            client.DeleteDirectory("/newdir");
            Assert.False(Directory.Exists(dirPath));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_rename_file()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "old.txt"), "rename me");

        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            client.RenameFile("/old.txt", "/new.txt");
            Assert.False(File.Exists(Path.Combine(userDir, "old.txt")));
            Assert.True(File.Exists(Path.Combine(userDir, "new.txt")));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_delete_file()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "delete-me.txt"), "delete me");

        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            client.DeleteFile("/delete-me.txt");
            Assert.False(File.Exists(Path.Combine(userDir, "delete-me.txt")));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_file_exists()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "exists.txt"), "content");

        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            Assert.True(client.Exists("/exists.txt"));
            Assert.False(client.Exists("/does-not-exist.txt"));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Sftp_get_attributes()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "attrs.txt"), "check attrs");

        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            var attrs = client.GetAttributes("/attrs.txt");
            Assert.NotNull(attrs);
            Assert.False(attrs.IsDirectory);
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Scp_upload_file()
    {
        var srcFile = Path.Combine(_tempDir, "scp-upload-src.txt");
        File.WriteAllText(srcFile, "SCP upload test content");

        using var client = new ScpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            using var fs = File.OpenRead(srcFile);
            client.Upload(fs, "/uploaded-via-scp.txt");
            var destFile = Path.Combine(_homeDir, "testuser", "uploaded-via-scp.txt");
            Assert.True(File.Exists(destFile));
            Assert.Equal("SCP upload test content", File.ReadAllText(destFile));
        }
        finally
        {
            client.Disconnect();
        }
    }

    [Fact]
    public void Scp_download_file()
    {
        var userDir = Path.Combine(_homeDir, "testuser");
        Directory.CreateDirectory(userDir);
        File.WriteAllText(Path.Combine(userDir, "scp-download.txt"), "SCP download test");

        var destFile = Path.Combine(_tempDir, "scp-download-dest.txt");
        using var client = new ScpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            using var fs = File.OpenWrite(destFile);
            client.Download("/scp-download.txt", fs);
        }
        finally
        {
            client.Disconnect();
        }
        Assert.True(File.Exists(destFile));
        Assert.Equal("SCP download test", File.ReadAllText(destFile));
    }

    [Fact]
    public void Authentication_fails_with_wrong_password()
    {
        using var client = new SftpClient("127.0.0.1", _port, "testuser", "wrongpass");
        Assert.Throws<SshAuthenticationException>(() => client.Connect());
    }

    [Fact]
    public void Sftp_upload_large_file()
    {
        var content = new string('X', 1024 * 100);
        using var client = new SftpClient("127.0.0.1", _port, "testuser", "testpass");
        client.Connect();
        try
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            client.UploadFile(ms, "/large-file.txt");
            var destFile = Path.Combine(_homeDir, "testuser", "large-file.txt");
            Assert.True(File.Exists(destFile));
        }
        finally
        {
            client.Disconnect();
        }
    }
}
