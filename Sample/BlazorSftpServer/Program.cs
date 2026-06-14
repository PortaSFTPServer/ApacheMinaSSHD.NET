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
