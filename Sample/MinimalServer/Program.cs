using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser"));
server.SetFixedPasswordAuthenticator("admin", "changeme");
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(@"C:\sftp-root"));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());
server.Start();
Console.WriteLine("SFTP server running on port 2222. Press Enter to stop.");
Console.ReadLine();
server.Stop();
