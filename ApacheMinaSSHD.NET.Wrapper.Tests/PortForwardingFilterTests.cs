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

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public sealed class PortForwardingFilterTests
{
    private static readonly ISshSession FakeSession = new FakeSshSession();

    [Fact]
    public void TcpForwardingFilter_AllPolicy_allows_all()
    {
        var filter = new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.All);
        Assert.True(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void TcpForwardingFilter_NonePolicy_rejects_all()
    {
        var filter = new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.None);
        Assert.False(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void TcpForwardingFilter_LocalPolicy_allows_direct_only()
    {
        var filter = new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.Local);
        Assert.True(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void TcpForwardingFilter_RemotePolicy_allows_forwarded_only()
    {
        var filter = new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.Remote);
        Assert.True(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void TcpForwardingFilter_AcceptAll_static_is_all()
    {
        var filter = AMNetTcpForwardingFilter.AcceptAll;
        Assert.True(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void TcpForwardingFilter_RejectAll_static_is_none()
    {
        var filter = AMNetTcpForwardingFilter.RejectAll;
        Assert.False(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void ForwardingFilter_AcceptAll_allows_tcp_rejects_agent_and_x11()
    {
        var filter = AMNetForwardingFilter.AcceptAll;
        Assert.True(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(filter.CanForwardAgent(FakeSession, "auth-agent@openssh.com"));
        Assert.False(filter.CanForwardX11(FakeSession, "x11"));
    }

    [Fact]
    public void ForwardingFilter_RejectAll_rejects_everything()
    {
        var filter = AMNetForwardingFilter.RejectAll;
        Assert.False(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
        Assert.False(filter.CanForwardAgent(FakeSession, "auth-agent@openssh.com"));
        Assert.False(filter.CanForwardX11(FakeSession, "x11"));
    }

    [Fact]
    public void ForwardingFilter_FromPolicy_All_allows_all()
    {
        var filter = AMNetForwardingFilter.FromPolicy(AMNetTcpForwardingPolicy.All);
        Assert.True(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.True(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void ForwardingFilter_FromPolicy_None_rejects_all()
    {
        var filter = AMNetForwardingFilter.FromPolicy(AMNetTcpForwardingPolicy.None);
        Assert.False(filter.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(filter.CanConnect(AMNetForwardingType.Forwarded, "10.0.0.1", 22, FakeSession));
    }

    [Fact]
    public void ForwardingFilter_composite_with_individual_filters()
    {
        var tcp = new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.All);
        var agent = new AMNetAcceptAllAgentForwardingFilter();
        var x11 = new AMNetAcceptAllX11ForwardingFilter();
        var composite = new AMNetForwardingFilter(tcp, agent, x11);

        Assert.True(composite.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(composite.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.True(composite.CanForwardAgent(FakeSession, "auth-agent@openssh.com"));
        Assert.True(composite.CanForwardX11(FakeSession, "x11"));
    }

    [Fact]
    public void ForwardingFilter_composite_with_null_subfilters_uses_safe_defaults()
    {
        var composite = new AMNetForwardingFilter(null, null, null);
        Assert.True(composite.CanListen("0.0.0.0", 8080, FakeSession));
        Assert.True(composite.CanConnect(AMNetForwardingType.Direct, "example.com", 80, FakeSession));
        Assert.False(composite.CanForwardAgent(FakeSession, "auth-agent@openssh.com"));
        Assert.False(composite.CanForwardX11(FakeSession, "x11"));
    }

    [Fact]
    public void PortForwardingEventListener_default_methods_do_not_throw()
    {
        var listener = new AMNetPortForwardingEventListener();
        listener.OnEstablishingTunnel("host", 80, true, FakeSession);
        listener.OnEstablishedTunnel("host", 80, true, "0.0.0.0:8080", FakeSession);
        listener.OnTearingDownTunnel("host", 80, true, FakeSession);
        listener.OnTornDownTunnel("host", 80, true, FakeSession);
    }

    [Fact]
    public void DelegatingTcpForwardingFilter_works()
    {
        bool canListenCalled = false;
        bool canConnectCalled = false;
        var filter = new AMNetDelegateTcpForwardingFilter(
            (host, port, session) =>
            {
                canListenCalled = true;
                Assert.Equal("test", host);
                Assert.Equal(1234, port);
                Assert.Same(FakeSession, session);
                return true;
            },
            (type, host, port, session) =>
            {
                canConnectCalled = true;
                Assert.Equal(AMNetForwardingType.Direct, type);
                Assert.Equal("target", host);
                Assert.Equal(99, port);
                Assert.Same(FakeSession, session);
                return false;
            });

        Assert.True(filter.CanListen("test", 1234, FakeSession));
        Assert.True(canListenCalled);
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "target", 99, FakeSession));
        Assert.True(canConnectCalled);
    }

    private sealed class FakeSshSession : ISshSession
    {
        public string RemoteAddress => "127.0.0.1";
        public Guid SessionId { get; } = Guid.NewGuid();
    }

    private sealed class AMNetAcceptAllAgentForwardingFilter : IAMNetAgentForwardingFilter
    {
        public bool CanForwardAgent(ISshSession session, string requestType) => true;
    }

    private sealed class AMNetAcceptAllX11ForwardingFilter : IAMNetX11ForwardingFilter
    {
        public bool CanForwardX11(ISshSession session, string requestType) => true;
    }
}
