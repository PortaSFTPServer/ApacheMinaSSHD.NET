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
public class ConnectionRateLimiterTests
{
    [Fact]
    public void Constructor_default_values()
    {
        var limiter = new AMNetConnectionRateLimiter();
        Assert.Equal(10, limiter.MaxConnections);
        Assert.Equal(TimeSpan.FromSeconds(1), limiter.Window);
    }

    [Fact]
    public void Constructor_custom_values()
    {
        var limiter = new AMNetConnectionRateLimiter(5, TimeSpan.FromSeconds(3));
        Assert.Equal(5, limiter.MaxConnections);
        Assert.Equal(TimeSpan.FromSeconds(3), limiter.Window);
    }

    [Fact]
    public void Constructor_zero_maxConnections_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AMNetConnectionRateLimiter(0));
    }

    [Fact]
    public void Constructor_negative_maxConnections_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AMNetConnectionRateLimiter(-1));
    }

    [Fact]
    public void IsConnectionAllowed_returns_true_for_first_connection()
    {
        var limiter = new AMNetConnectionRateLimiter(5);
        Assert.True(limiter.IsConnectionAllowed("192.168.1.1"));
    }

    [Fact]
    public void IsConnectionAllowed_allows_up_to_limit()
    {
        var limiter = new AMNetConnectionRateLimiter(3);
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
    }

    [Fact]
    public void IsConnectionAllowed_rejects_after_limit()
    {
        var limiter = new AMNetConnectionRateLimiter(2);
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.False(limiter.IsConnectionAllowed("10.0.0.1"));
    }

    [Fact]
    public void Different_IPs_have_independent_limits()
    {
        var limiter = new AMNetConnectionRateLimiter(2);
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.False(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.2"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.2"));
    }

    [Fact]
    public void IsConnectionAllowed_empty_string_is_tracked_separately()
    {
        var limiter = new AMNetConnectionRateLimiter(1);
        Assert.True(limiter.IsConnectionAllowed(""));
        Assert.False(limiter.IsConnectionAllowed(""));
    }

    [Fact]
    public void Reset_single_address_clears_that_address_only()
    {
        var limiter = new AMNetConnectionRateLimiter(1);
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.False(limiter.IsConnectionAllowed("10.0.0.1"));

        limiter.Reset("10.0.0.1");

        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
    }

    [Fact]
    public void Reset_all_addresses_clears_everything()
    {
        var limiter = new AMNetConnectionRateLimiter(1);
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.2"));
        Assert.False(limiter.IsConnectionAllowed("10.0.0.1"));

        limiter.Reset();

        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.2"));
    }

    [Fact]
    public void Sliding_window_expires_old_entries()
    {
        var limiter = new AMNetConnectionRateLimiter(2, TimeSpan.FromMilliseconds(50));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
        Assert.False(limiter.IsConnectionAllowed("10.0.0.1"));

        Thread.Sleep(100);

        Assert.True(limiter.IsConnectionAllowed("10.0.0.1"));
    }

    [Fact]
    public async Task Thread_safety_concurrent_access()
    {
        var limiter = new AMNetConnectionRateLimiter(100);
        var tasks = new Task<bool>[50];

        for (int i = 0; i < 50; i++)
        {
            tasks[i] = Task.Run(() => limiter.IsConnectionAllowed("10.0.0.1"));
        }

        var results = await Task.WhenAll(tasks);

        int allowed = results.Count(r => r);
        Assert.InRange(allowed, 1, 100);
    }
}

[Trait("Category", "Unit")]
public class RateLimitingIoServiceEventListenerTests
{
    private class AllowAllListener : IAMNetIoServiceEventListener
    {
        public int AcceptedCount;
        public int AbortedCount;

        public bool OnConnectionAccepted(ISshServiceConnection context)
        {
            AcceptedCount++;
            return true;
        }

        public void OnConnectionAborted(ISshServiceConnection context) => AbortedCount++;
        public void OnOutboundConnectionAborted(ISshServiceConnection context) { }
        public void OnOutboundConnectionEstablished(ISshServiceConnection context) { }
    }

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

    private static ISshServiceConnection MakeConnection(string ip)
    {
        return new MockConnection
        {
            RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), 12345)
        };
    }

    [Fact]
    public void Constructor_null_inner_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new RateLimitingIoServiceEventListener(null!, new AMNetConnectionRateLimiter()));
    }

    [Fact]
    public void Constructor_null_limiter_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new RateLimitingIoServiceEventListener(new AllowAllListener(), null!));
    }

    [Fact]
    public void OnConnectionAccepted_delegates_to_inner_when_allowed()
    {
        var inner = new AllowAllListener();
        var listener = new RateLimitingIoServiceEventListener(inner, new AMNetConnectionRateLimiter(5));

        var result = listener.OnConnectionAccepted(MakeConnection("10.0.0.1"));

        Assert.True(result);
        Assert.Equal(1, inner.AcceptedCount);
    }

    [Fact]
    public void OnConnectionAccepted_blocks_when_rate_limited()
    {
        var inner = new AllowAllListener();
        var limiter = new AMNetConnectionRateLimiter(2);
        var listener = new RateLimitingIoServiceEventListener(inner, limiter);

        listener.OnConnectionAccepted(MakeConnection("10.0.0.1"));
        listener.OnConnectionAccepted(MakeConnection("10.0.0.1"));
        var result = listener.OnConnectionAccepted(MakeConnection("10.0.0.1"));

        Assert.False(result);
        Assert.Equal(2, inner.AcceptedCount);
    }

    [Fact]
    public void OnConnectionAccepted_blocks_by_address()
    {
        var inner = new AllowAllListener();
        var limiter = new AMNetConnectionRateLimiter(1);
        var listener = new RateLimitingIoServiceEventListener(inner, limiter);

        Assert.True(listener.OnConnectionAccepted(MakeConnection("10.0.0.1")));
        Assert.True(listener.OnConnectionAccepted(MakeConnection("10.0.0.2")));
        Assert.False(listener.OnConnectionAccepted(MakeConnection("10.0.0.1")));

        Assert.Equal(2, inner.AcceptedCount);
    }

    [Fact]
    public void OnConnectionAccepted_empty_remote_address_is_allowed()
    {
        var inner = new AllowAllListener();
        var listener = new RateLimitingIoServiceEventListener(inner, new AMNetConnectionRateLimiter(1));

        var conn = new MockConnection
        {
            RemoteEndPoint = null!,
        };

        var result = listener.OnConnectionAccepted(conn);

        Assert.True(result);
        Assert.Equal(1, inner.AcceptedCount);
    }

    [Fact]
    public void OnConnectionAborted_delegates_to_inner()
    {
        var inner = new AllowAllListener();
        var listener = new RateLimitingIoServiceEventListener(inner, new AMNetConnectionRateLimiter());

        listener.OnConnectionAborted(MakeConnection("10.0.0.1"));

        Assert.Equal(1, inner.AbortedCount);
    }

    [Fact]
    public void Outbound_events_delegate_to_inner()
    {
        var inner = new AllowAllListener();
        var listener = new RateLimitingIoServiceEventListener(inner, new AMNetConnectionRateLimiter());

        listener.OnOutboundConnectionEstablished(MakeConnection("10.0.0.1"));
        listener.OnOutboundConnectionAborted(MakeConnection("10.0.0.1"));

        Assert.Equal(0, inner.AcceptedCount);
        Assert.Equal(0, inner.AbortedCount);
    }
}
