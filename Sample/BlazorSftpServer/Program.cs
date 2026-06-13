// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using BlazorSftpServer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<ServerState>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<BlazorSftpServer.Components.App>().AddInteractiveServerRenderMode();
app.Run();

class BlazorSessionListener(ServerState state) : AMNetSessionListener
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
