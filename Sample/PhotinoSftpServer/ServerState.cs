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
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace PhotinoSftpServer;

public class ServerState
{
    public AMNetSshServer? Server { get; set; }
    public List<LogEntry> Log { get; } = [];
    public List<SessionInfo> Sessions { get; } = [];

    public event Action? Changed;

    public void LogMessage(string text)
    {
        Log.Add(new LogEntry(text));
        if (Log.Count > 500) Log.RemoveAt(0);
        Notify();
    }

    public void AddSession(ISshSession session)
    {
        Sessions.Add(new SessionInfo(session));
        Notify();
    }

    public void RemoveSession(ISshSession session)
    {
        Sessions.RemoveAll(s => s.Id == session.SessionId);
        Notify();
    }

    public void ClearLog()
    {
        Log.Clear();
        Notify();
    }

    private void Notify() => Changed?.Invoke();

    public record LogEntry(string Text)
    {
        public string Timestamp { get; } = DateTime.Now.ToString("HH:mm:ss");
    }

    public record SessionInfo(ISshSession Session)
    {
        public Guid Id => Session.SessionId;
        public string RemoteAddress => Session.RemoteAddress ?? "unknown";
        public string StartedAt => DateTime.Now.ToString("HH:mm:ss");
    }
}
