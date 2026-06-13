// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();
server.SetFixedPasswordAuthenticator("admin", "changeme");
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser"));
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(@"C:\sftp-root"));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());

// Session lifecycle monitoring
server.addSessionListener(new AuditSessionListener());

// Network-level connection monitoring
server.setIoServiceEventListener(new AuditIoServiceEventListener());

// Proxy metadata inspection
server.setServerProxyAcceptor(new AuditProxyAcceptor());

server.Start();
Console.WriteLine("SessionMonitorServer on port 2222. Press Enter to stop.");
Console.ReadLine();
server.Stop();

class AuditSessionListener : AMNetSessionListener
{
    public AuditSessionListener() : base(new AMNetLogger(typeof(AuditSessionListener))) { }

    public override void OnSessionCreated(ISshSession session)
    {
        Console.WriteLine($"[Session] Created: {session.SessionId} from {session.RemoteAddress}");
    }

    public override void OnSessionEstablished(ISshSession session)
    {
        Console.WriteLine($"[Session] Established: {session.SessionId}");
    }

    public override void OnSessionClosed(ISshSession session)
    {
        Console.WriteLine($"[Session] Closed: {session.SessionId}");
    }

    public override void OnSessionDisconnect(ISshSessionEvent context)
    {
        Console.WriteLine($"[Session] Disconnect: {context.Session.SessionId} (reason={context.Reason}, msg={context.Message})");
    }

    public override void OnSessionException(ISshSessionEvent context)
    {
        Console.WriteLine($"[Session] Exception: {context.Session.SessionId} - {context.Exception?.Message}");
    }
}

class AuditIoServiceEventListener : AMNetIoServiceEventListener
{
    public override bool OnConnectionAccepted(ISshServiceConnection context)
    {
        Console.WriteLine($"[Network] Accept: {context.RemoteEndPoint} -> {context.LocalEndPoint}");
        return base.OnConnectionAccepted(context);
    }

    public override void OnConnectionAborted(ISshServiceConnection context)
    {
        Console.WriteLine($"[Network] Abort: {context.RemoteEndPoint}");
    }
}

class AuditProxyAcceptor : AMNetServerProxyAcceptor
{
    public override bool acceptServerProxyMetadata(IProxyMetadata proxyMetadata)
    {
        Console.WriteLine($"[Proxy] From: {proxyMetadata.RemoteAddress}");
        return base.acceptServerProxyMetadata(proxyMetadata);
    }
}
