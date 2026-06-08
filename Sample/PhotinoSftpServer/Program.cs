using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using Photino.Blazor;

var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);
appBuilder.RootComponents.Add<PhotinoSftpServer.App>("#app");

var app = appBuilder.Build();

app.MainWindow
    .SetTitle("ApacheMinaSSHD.NET — SFTP Server Manager")
    .SetSize(900, 650)
    .SetMinSize(640, 400)
    .SetUseOsDefaultSize(false);

app.Run();

class PhotinoSessionListener(PhotinoSftpServer.App app) : AMNetSessionListener
{
    public override void OnSessionCreated(ISshSession session)
    {
        app.Log($"Session opened: {session.RemoteAddress}");
        app.AddSession(session);
    }

    public override void OnSessionClosed(ISshSession session)
    {
        app.Log($"Session closed: {session.RemoteAddress}");
        app.RemoveSession(session);
    }
}
