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

// SCP with custom file opener and transfer event listener
var scp = new AMNetScpCommandFactory(new AuditScpFileOpener(@"C:\sftp-root"));
scp.addEventListener(new AuditScpTransferListener());
server.setCommandFactory(scp);

// Also set up SFTP subsystem for hybrid usage
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());

server.Start();
Console.WriteLine("SCP server on port 2222. Press Enter to stop.");
Console.ReadLine();
server.Stop();

class AuditScpFileOpener : AMNetScpFileOpener
{
    public AuditScpFileOpener(string rootPath) : base(rootPath) { }

    public override string ResolveLocalPath(ISshScpFileAccess access, string resolvedPath)
    {
        Console.WriteLine($"[SCP] Resolve: {access.RequestedPath} -> {resolvedPath}");
        return base.ResolveLocalPath(access, resolvedPath);
    }

    public override bool IsPathAllowed(ISshScpFileAccess access)
    {
        var allowed = base.IsPathAllowed(access);
        Console.WriteLine($"[SCP] {access.Operation} on {access.LocalPath}: {(allowed ? "allowed" : "DENIED")}");
        return allowed;
    }

    public override void OpenRead(ISshScpFileAccess access)
    {
        Console.WriteLine($"[SCP] Download started: {access.LocalPath} ({access.Length} bytes)");
    }

    public override void CloseRead(ISshScpFileAccess access)
    {
        Console.WriteLine($"[SCP] Download complete: {access.LocalPath}");
    }

    public override void OpenWrite(ISshScpFileAccess access)
    {
        Console.WriteLine($"[SCP] Upload started: {access.LocalPath}");
    }

    public override void CloseWrite(ISshScpFileAccess access)
    {
        Console.WriteLine($"[SCP] Upload complete: {access.LocalPath}");
    }

    public override bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
    {
        var name = Path.GetFileName(access.LocalPath);
        if (string.Equals(name, "backup", StringComparison.OrdinalIgnoreCase)) return false;
        return base.ShouldIncludeDirectoryEntry(access);
    }
}

class AuditScpTransferListener : AMNetScpTransferEventListener
{
    public override void OnStartFile(ISshScpTransferEvent context)
    {
        Console.WriteLine($"[SCP Event] Start: {context.Path} ({context.Length} bytes, op={context.Operation})");
    }

    public override void OnEndFile(ISshScpTransferEvent context)
    {
        Console.WriteLine($"[SCP Event] End: {context.Path}");
    }

    public override void OnStartFolder(ISshScpTransferEvent context)
    {
        Console.WriteLine($"[SCP Event] Start folder: {context.Path}");
    }

    public override void OnEndFolder(ISshScpTransferEvent context)
    {
        Console.WriteLine($"[SCP Event] End folder: {context.Path}");
    }
}
