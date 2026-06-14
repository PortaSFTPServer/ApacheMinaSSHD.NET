// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;

string rootPath = Path.Combine(AppContext.BaseDirectory, "sftp-root");
string hostKeyPath = Path.Combine(AppContext.BaseDirectory, "hostkey.ser");

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;

server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();

var hostKeys = new AMNetSimpleGeneratorHostKeyProvider(hostKeyPath);
hostKeys.setAlgorithm(AMNetSshAlgorithms.HostKeyAlgorithms.Rsa);
hostKeys.setKeySize(3072);
server.setKeyPairProvider(hostKeys);

string? password = "Test1234";
if (!string.IsNullOrWhiteSpace(password))
{
    string username = Environment.GetEnvironmentVariable("SFTP_USERNAME") ?? "demo";
    server.SetFixedPasswordAuthenticator(username, password);
    Console.WriteLine($"Password auth enabled for user '{username}'");
}
else
{
    Console.WriteLine("WARNING: No SFTP_PASSWORD set � authentication disabled");
}

server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(rootPath));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    server.Stop();
    Console.WriteLine("\nServer stopped.");
};

server.Start();
Console.WriteLine($"SFTP server listening on {server.Host}:{server.Port}");
Console.WriteLine("Press Ctrl+C to stop.");
await Task.Delay(Timeout.Infinite);
