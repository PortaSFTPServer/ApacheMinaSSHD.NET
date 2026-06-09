using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Photino.NET;
using PhotinoSftpServer;
using System.Net;

namespace PhotinoSftpServer;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var logFile = Path.Combine(Path.GetTempPath(), "PhotinoSftpServer.log");
        File.WriteAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] Starting...\n");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot"),
        });
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, 0);
        });
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddSingleton<ServerState>();

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<PhotinoSftpServer.Components.App>().AddInteractiveServerRenderMode();

        app.StartAsync().GetAwaiter().GetResult();

        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] Kestrel bound: {address}\n");

        var window = new PhotinoWindow()
            .SetTitle("ApacheMinaSSHD.NET — SFTP Server Manager")
            .SetSize(900, 650)
            .SetMinSize(640, 400)
            .SetUseOsDefaultSize(false)
            .Load(new Uri(address));

        window.WaitForClose();
        File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] Window closed\n");
        app.StopAsync().GetAwaiter().GetResult();
    }
}

class PhotinoSessionListener(ServerState state) : AMNetSessionListener
{
    public override void OnSessionCreated(ISshSession session)
    {
        state.LogMessage($"Session opened: {session.RemoteAddress}");
        state.AddSession(session);
    }

    public override void OnSessionClosed(ISshSession session)
    {
        state.LogMessage($"Session closed: {session.RemoteAddress}");
        state.RemoveSession(session);
    }
}
