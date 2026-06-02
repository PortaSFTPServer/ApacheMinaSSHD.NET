using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class SessionListenerTests
{
    private sealed class MockSession : ISshSession
    {
        public string RemoteAddress => "10.0.0.5";
        public Guid SessionId => Guid.NewGuid();
    }

    private sealed class MockSessionEvent : ISshSessionEvent
    {
        public ISshSession Session { get; set; } = new MockSession();
        public string? EventName { get; set; }
        public int? Reason { get; set; }
        public string? Message { get; set; }
        public string? Language { get; set; }
        public bool? Initiator { get; set; }
        public string? Version { get; set; }
        public IReadOnlyList<string> ExtraLines { get; set; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, string> ClientProposal { get; set; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> ServerProposal { get; set; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> NegotiatedOptions { get; set; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> Proposal { get; set; } = new Dictionary<string, string>();
        public Exception? Exception { get; set; }
    }

    private sealed class CollectingLogger : IAMNetLogger
    {
        public List<string> Messages { get; } = [];
        public List<string> Errors { get; } = [];

        public void Info(string message) => Messages.Add(message);
        public void Warn(string message, Exception? ex = null) => Messages.Add(message);
        public void Debug(string message, Exception? ex = null) => Messages.Add(message);
        public void Error(string message, Exception? ex = null)
        {
            Errors.Add(message);
            if (ex != null) Errors.Add(ex.Message);
        }
        public void Trace(string message, Exception? ex = null) => Messages.Add(message);
    }

    [Fact] public void Constructor_default_logger() => new AMNetSessionListener();
    [Fact] public void Constructor_with_logger() => new AMNetSessionListener(new CollectingLogger());

    [Fact]
    public void OnSessionCreated_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionCreated(new MockSession());
        Assert.Contains(logger.Messages, m => m.Contains("created"));
    }

    [Fact]
    public void OnSessionEstablished_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionEstablished(new MockSession());
        Assert.Contains(logger.Messages, m => m.Contains("established"));
    }

    [Fact]
    public void OnSessionClosed_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionClosed(new MockSession());
        Assert.Contains(logger.Messages, m => m.Contains("closed"));
    }

    [Fact]
    public void OnSessionDisconnect_logs_message()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionDisconnect(new MockSessionEvent { Message = "Connection closed" });
        Assert.Contains(logger.Messages, m => m.Contains("Connection closed"));
    }

    [Fact]
    public void OnSessionEvent_logs_event_name()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionEvent(new MockSessionEvent { EventName = "auth-success" });
        Assert.Contains(logger.Messages, m => m.Contains("auth-success"));
    }

    [Fact]
    public void OnSessionException_logs_error()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionException(new MockSessionEvent { Exception = new InvalidOperationException("test error") });
        Assert.Contains(logger.Errors, m => m.Contains("test error"));
    }

    [Fact]
    public void OnSessionNegotiationStart_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionNegotiationStart(new MockSessionEvent());
        Assert.Contains(logger.Messages, m => m.Contains("negotiation started"));
    }

    [Fact]
    public void OnSessionNegotiationEnd_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionNegotiationEnd(new MockSessionEvent());
        Assert.Contains(logger.Messages, m => m.Contains("negotiation ended"));
    }

    [Fact]
    public void OnSessionPeerIdentificationLine_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionPeerIdentificationLine(new MockSessionEvent { Version = "SSH-2.0-OpenSSH" });
        Assert.Contains(logger.Messages, m => m.Contains("SSH-2.0-OpenSSH"));
    }

    [Fact]
    public void OnSessionPeerIdentificationReceived_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionPeerIdentificationReceived(new MockSessionEvent { Version = "v2.0" });
        Assert.Contains(logger.Messages, m => m.Contains("received") && m.Contains("v2.0"));
    }

    [Fact]
    public void OnSessionPeerIdentificationSend_logs()
    {
        var logger = new CollectingLogger();
        new AMNetSessionListener(logger).OnSessionPeerIdentificationSend(new MockSessionEvent { Version = "v2.0" });
        Assert.Contains(logger.Messages, m => m.Contains("sent") && m.Contains("v2.0"));
    }
}
