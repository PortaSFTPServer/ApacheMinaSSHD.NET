using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class SftpEventListenerTests
{
    private readonly AMNetSftpEventListener _listener = new();
    private readonly MockSession _session = new();
    private readonly MockHandle _handle = new();

    [Fact] public void Constructor_does_not_throw() => _ = new AMNetSftpEventListener();

    [Fact] public void OnModifyingAttributes() => _listener.OnModifyingAttributes(MkPath("/f"));
    [Fact] public void OnModifiedAttributes() => _listener.OnModifiedAttributes(MkPath("/f"));
    [Fact] public void OnClosing() => _listener.OnClosing(MkEvent());
    [Fact] public void OnClosed() => _listener.OnClosed(MkEvent());
    [Fact] public void OnOpening() => _listener.OnOpening(MkEvent());
    [Fact] public void OnOpen() => _listener.OnOpen(MkEvent());
    [Fact] public void OnReading() => _listener.OnReading(MkReadWrite());
    [Fact] public void OnRead() => _listener.OnRead(MkReadWrite());
    [Fact] public void OnOpenFailed() => _listener.OnOpenFailed(MkIOFailure());
    [Fact] public void OnWriting() => _listener.OnWriting(MkReadWrite());
    [Fact] public void OnWrite() => _listener.OnWrite(MkReadWrite());
    [Fact] public void OnCreating() => _listener.OnCreating(MkPath("/d"));
    [Fact] public void OnCreated() => _listener.OnCreated(MkPath("/d"));
    [Fact] public void OnMoving() => _listener.OnMoving(MkMove());
    [Fact] public void OnMoved() => _listener.OnMoved(MkMove());
    [Fact] public void OnRemoving() => _listener.OnRemoving(MkPath("/r"));
    [Fact] public void OnRemoved() => _listener.OnRemoved(MkPath("/r"));
    [Fact] public void OnLinking() => _listener.OnLinking(MkLink());
    [Fact] public void OnLink() => _listener.OnLink(MkLink());
    [Fact] public void OnInitialized() => _listener.OnInitialized(_session, 4);
    [Fact] public void OnDestroying() => _listener.OnDestroying(_session);
    [Fact] public void OnReadingEntries() => _listener.OnReadingEntries(MkEntries());
    [Fact] public void OnReadEntries() => _listener.OnReadEntries(MkEntries());
    [Fact] public void OnExiting() => _listener.OnExiting(_session, _handle);
    [Fact] public void OnReceivedExtension() => _listener.OnReceivedExtension(MkReceived());
    [Fact] public void OnReceived() => _listener.OnReceived(MkReceived());

    private ISshPath MkPath(string path) => new MockPath { Path = path, Session = _session };
    private ISshEvent MkEvent() => new MockEvent { SshHandle = _handle, Session = _session };
    private ISshReadWrite MkReadWrite() => new MockReadWrite { SshHandle = _handle, Session = _session };
    private ISshIOFailure MkIOFailure() => new MockIOFailure { LocalPath = "/fail.txt", Session = _session };
    private ISshMove MkMove() => new MockMove { SourcePath = "/a", DestPath = "/b", Session = _session };
    private ISshSysLink MkLink() => new MockSysLink { SourcePath = "/t", DestPath = "/l", Session = _session, SymLink = true };
    private ISshEntries MkEntries() => new MockEntries { RemoteHandle = "h", SshSession = _session, localHandle = new MockDirHandle() };
    private ISshReceived MkReceived() => new MockReceived { Extension = "ext", Id = 1, Type = 200, SshSession = _session };

    private sealed class MockSession : ISshSession
    {
        public string RemoteAddress => "10.0.0.5";
        public Guid SessionId => Guid.NewGuid();
    }

    private sealed class MockHandle : ISshHandle
    {
        public string Id => "h1";
        public string PhysicalPath => "/test.txt";
        public bool IsOpen => true;
        public void Dispose() { }
    }

    private sealed class MockDirHandle : ISshDirectoryHandle
    {
        public bool HasNext => false;
        public bool IsDone => true;
        public bool ShouldSendDot => false;
        public bool ShouldSendDotDot => false;
        public bool IsWithDots => true;
        public string PhysicalPath => "/dir";
        public string Next() => "";
        public void MarkDone() { }
        public void MarkDotSent() { }
        public void MarkDotDotSent() { }
        public void Remove() { }
        public void Close() { }
        public void Dispose() { }
    }

    private sealed class MockPath : ISshPath
    {
        public ISshSession Session { get; set; } = null!;
        public string Path { get; set; } = "";
        public bool IsDirectory { get; set; }
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        public Exception Exception { get; set; } = null!;
    }

    private sealed class MockEvent : ISshEvent
    {
        public ISshSession Session { get; set; } = null!;
        public string RemoteHandle { get; set; } = "rh1";
        public ISshHandle SshHandle { get; set; } = null!;
        public Exception Exception { get; set; } = null!;
    }

    private sealed class MockReadWrite : ISshReadWrite
    {
        public ISshSession Session { get; set; } = null!;
        public string RemoteHandle { get; set; } = "rh1";
        public ISshHandle SshHandle { get; set; } = null!;
        public Exception Exception { get; set; } = null!;
        public long Offset => 0;
        public int Length => 1024;
        public byte[] Data { get; set; } = [];
    }

    private sealed class MockIOFailure : ISshIOFailure
    {
        public ISshSession Session { get; set; } = null!;
        public string RemoteHandle { get; set; } = "rh1";
        public string LocalPath { get; set; } = "";
        public Exception Exception { get; set; } = null!;
    }

    private sealed class MockMove : ISshMove
    {
        public ISshSession Session { get; set; } = null!;
        public string SourcePath { get; set; } = "";
        public string DestPath { get; set; } = "";
        public IEnumerable<string> Options { get; set; } = [];
        public Exception Exception { get; set; } = null!;
    }

    private sealed class MockSysLink : ISshSysLink
    {
        public ISshSession Session { get; set; } = null!;
        public string SourcePath { get; set; } = "";
        public string DestPath { get; set; } = "";
        public bool SymLink { get; set; }
        public Exception Exception { get; set; } = null!;
    }

    private sealed class MockEntries : ISshEntries
    {
        public ISshSession SshSession { get; set; } = null!;
        public string RemoteHandle { get; set; } = "";
        public ISshDirectoryHandle localHandle { get; set; } = null!;
        public IReadOnlyDictionary<string, object> Entries { get; set; } = new Dictionary<string, object>();
    }

    private sealed class MockReceived : ISshReceived
    {
        public ISshSession SshSession { get; set; } = null!;
        public int Type { get; set; }
        public string Extension { get; set; } = "";
        public int Id { get; set; }
    }
}
