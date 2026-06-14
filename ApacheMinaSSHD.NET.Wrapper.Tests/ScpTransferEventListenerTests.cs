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
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class ScpTransferEventListenerTests
{
    private sealed class CollectingLogger : IAMNetLogger
    {
        public List<string> Debugs { get; } = [];
        public void Info(string message) { }
        public void Warn(string message, Exception? ex = null) { }
        public void Debug(string message, Exception? ex = null) => Debugs.Add(message);
        public void Error(string message, Exception? ex = null) { }
        public void Trace(string message, Exception? ex = null) { }
    }

    private sealed class MockTransferEvent : ISshScpTransferEvent
    {
        public ISshSession Session { get; set; } = new MockSession();
        public string Operation { get; set; } = "upload";
        public string Path { get; set; } = "/remote/file.txt";
        public long Length { get; set; }
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
        public int? AckStatusCode { get; set; }
        public string? AckLine { get; set; }
        public string? Command { get; set; } = "scp -t";
        public Exception? Exception { get; set; }
    }

    private sealed class MockSession : ISshSession
    {
        public string RemoteAddress => "10.0.0.5";
        public Guid SessionId => Guid.NewGuid();
    }

    [Fact] public void Constructor_default_logger() => new AMNetScpTransferEventListener();
    [Fact] public void Constructor_with_logger() => new AMNetScpTransferEventListener(new CollectingLogger());

    [Fact]
    public void OnStartFile_logs()
    {
        var logger = new CollectingLogger();
        new AMNetScpTransferEventListener(logger).OnStartFile(new MockTransferEvent { Operation = "upload", Path = "/f" });
        Assert.Contains(logger.Debugs, m => m.Contains("upload") && m.Contains("/f"));
    }

    [Fact]
    public void OnEndFile_logs()
    {
        var logger = new CollectingLogger();
        new AMNetScpTransferEventListener(logger).OnEndFile(new MockTransferEvent { Operation = "upload", Path = "/f" });
        Assert.Contains(logger.Debugs, m => m.Contains("ended"));
    }

    [Fact]
    public void OnFileAck_logs()
    {
        var logger = new CollectingLogger();
        new AMNetScpTransferEventListener(logger).OnFileAck(new MockTransferEvent { Operation = "upload", Path = "/f", AckStatusCode = 0 });
        Assert.Contains(logger.Debugs, m => m.Contains("acknowledgement"));
    }

    [Fact]
    public void OnReceiveCommandAck_logs()
    {
        var logger = new CollectingLogger();
        new AMNetScpTransferEventListener(logger).OnReceiveCommandAck(new MockTransferEvent { Command = "scp -t", AckStatusCode = 1 });
        Assert.Contains(logger.Debugs, m => m.Contains("scp -t"));
    }

    [Fact]
    public void OnStartFolder_logs()
    {
        var logger = new CollectingLogger();
        new AMNetScpTransferEventListener(logger).OnStartFolder(new MockTransferEvent { Operation = "download", Path = "/dir" });
        Assert.Contains(logger.Debugs, m => m.Contains("download") && m.Contains("/dir"));
    }

    [Fact]
    public void OnEndFolder_logs()
    {
        var logger = new CollectingLogger();
        new AMNetScpTransferEventListener(logger).OnEndFolder(new MockTransferEvent { Operation = "download", Path = "/dir" });
        Assert.Contains(logger.Debugs, m => m.Contains("ended for folder"));
    }
}
