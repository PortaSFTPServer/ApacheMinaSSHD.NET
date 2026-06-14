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
using System.Net;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class IoServiceEventListenerTests
{
    private class MockConnection : ISshServiceConnection
    {
        public IPEndPoint LocalEndPoint { get; set; } = new(IPAddress.Any, 22);
        public IPEndPoint RemoteEndPoint { get; set; } = new(IPAddress.Loopback, 12345);
        public IPEndPoint ServiceEndPoint { get; set; } = new(IPAddress.Any, 22);
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        public ISshIoService IoService { get; set; } = new MockIoService();
        public Exception Exception { get; set; } = null!;
    }

    private class MockIoService : ISshIoService
    {
        public bool IsAcceptor { get; set; } = true;
        public bool IsClosed => false;
        public bool IsClosing => false;
        public IEnumerable<IPEndPoint> BoundAddresses { get; set; } = [new IPEndPoint(IPAddress.Any, 22)];
    }

    [Fact]
    public void Constructor_does_not_throw() => _ = new AMNetIoServiceEventListener();

    [Fact]
    public void OnConnectionAborted_does_not_throw()
    {
        new AMNetIoServiceEventListener().OnConnectionAborted(
            new MockConnection { Exception = new InvalidOperationException("test") });
    }

    [Fact]
    public void OnConnectionAccepted_returns_true()
    {
        Assert.True(new AMNetIoServiceEventListener().OnConnectionAccepted(new MockConnection()));
    }

    [Fact]
    public void OnConnectionAccepted_with_non_acceptor()
    {
        Assert.True(new AMNetIoServiceEventListener().OnConnectionAccepted(
            new MockConnection
            {
                IoService = new MockIoService { IsAcceptor = false, BoundAddresses = [] }
            }));
    }

    [Fact]
    public void OnOutboundConnectionAborted_does_not_throw()
    {
        new AMNetIoServiceEventListener().OnOutboundConnectionAborted(
            new MockConnection { Exception = new TimeoutException("timeout") });
    }

    [Fact]
    public void OnOutboundConnectionEstablished_does_not_throw()
    {
        new AMNetIoServiceEventListener().OnOutboundConnectionEstablished(new MockConnection());
    }
}
