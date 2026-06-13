// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
