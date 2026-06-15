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
using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class SshServerTests : IDisposable
{
    private readonly AMNetSshServer _server;

    public SshServerTests()
    {
        _server = AMNetSshServer.SetUpDefaultServer();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    [Fact]
    public void SetUpDefaultServer_returns_non_null()
    {
        using var server = AMNetSshServer.SetUpDefaultServer();
        Assert.NotNull(server);
    }

    [Fact]
    public void Port_default_is_0()
    {
        Assert.Equal(0, _server.Port);
    }

    [Fact]
    public void Port_roundtrip()
    {
        _server.Port = 2222;
        Assert.Equal(2222, _server.Port);
        Assert.Equal(2222, _server.getPort());
    }

    [Fact]
    public void Port_negative_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _server.Port = -1);
    }

    [Fact]
    public void Port_too_high_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _server.Port = 65536);
    }

    [Fact]
    public void setPort_and_getPort()
    {
        _server.setPort(2222);
        Assert.Equal(2222, _server.getPort());
    }

    [Fact]
    public void Host_default_null()
    {
        Assert.Null(_server.Host);
    }

    [Fact]
    public void Host_roundtrip()
    {
        _server.Host = "127.0.0.1";
        Assert.Equal("127.0.0.1", _server.Host);
    }

    [Fact]
    public void Host_null_is_allowed()
    {
        _server.Host = null;
        Assert.Null(_server.Host);
    }

    [Fact]
    public void setHost_and_getHost()
    {
        _server.setHost("0.0.0.0");
        Assert.Equal("0.0.0.0", _server.getHost());
    }

    [Fact]
    public void IsStarted_returns_false_initially()
    {
        Assert.False(_server.IsStarted());
        Assert.False(_server.isStarted());
    }

    [Fact]
    public void IsClosed_returns_false_initially()
    {
        Assert.False(_server.IsClosed());
        Assert.False(_server.isClosed());
    }

    [Fact]
    public void Config_is_available()
    {
        Assert.NotNull(_server.Config);
    }

    [Fact]
    public void setKeyPairProvider_accepts_provider()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        _server.setKeyPairProvider(provider);
    }

    [Fact]
    public void setKeyPairProvider_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setKeyPairProvider(null!));
    }

    [Fact]
    public void setFileSystemFactory_accepts_factory()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        _server.setFileSystemFactory(factory);
    }

    [Fact]
    public void setFileSystemFactory_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setFileSystemFactory(null!));
    }

    [Fact]
    public void setPasswordAuthenticator_accepts_authenticator()
    {
        _server.setPasswordAuthenticator(new AMNetFixedPasswordAuthenticator("user", "pass"));
    }

    [Fact]
    public void SetPasswordAuthenticator_accepts_authenticator()
    {
        _server.SetPasswordAuthenticator(new AMNetFixedPasswordAuthenticator("user", "pass"));
    }

    [Fact]
    public void setPasswordAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setPasswordAuthenticator(null!));
    }

    [Fact]
    public void SetPasswordAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.SetPasswordAuthenticator(null!));
    }

    [Fact]
    public void setFixedPasswordAuthenticator()
    {
        _server.setFixedPasswordAuthenticator("admin", "secret");
    }

    [Fact]
    public void SetFixedPasswordAuthenticator()
    {
        _server.SetFixedPasswordAuthenticator("admin", "secret");
    }

    [Fact]
    public void setDelegatePasswordAuthenticator()
    {
        _server.setDelegatePasswordAuthenticator((_, _, _) => true);
    }

    [Fact]
    public void SetDelegatePasswordAuthenticator()
    {
        _server.SetDelegatePasswordAuthenticator((_, _, _) => true);
    }

    [Fact]
    public void setCompositePasswordAuthenticator()
    {
        _server.setCompositePasswordAuthenticator(
            new AMNetFixedPasswordAuthenticator("user", "a"),
            new AMNetFixedPasswordAuthenticator("user", "b"));
    }

    [Fact]
    public void SetCompositePasswordAuthenticator()
    {
        _server.SetCompositePasswordAuthenticator(
            new AMNetFixedPasswordAuthenticator("user", "a"),
            new AMNetFixedPasswordAuthenticator("user", "b"));
    }

    [Fact]
    public void setPublickeyAuthenticator_accepts_authenticator()
    {
        _server.setPublickeyAuthenticator(new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc"));
    }

    [Fact]
    public void SetPublickeyAuthenticator_accepts_authenticator()
    {
        _server.SetPublickeyAuthenticator(new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc"));
    }

    [Fact]
    public void SetPublicKeyAuthenticator_accepts_authenticator()
    {
        _server.SetPublicKeyAuthenticator(new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc"));
    }

    [Fact]
    public void setPublickeyAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setPublickeyAuthenticator(null!));
    }

    [Fact]
    public void SetPublickeyAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.SetPublickeyAuthenticator(null!));
    }

    [Fact]
    public void setDelegatePublickeyAuthenticator()
    {
        _server.setDelegatePublickeyAuthenticator((_, _, _) => true);
    }

    [Fact]
    public void SetDelegatePublickeyAuthenticator()
    {
        _server.SetDelegatePublickeyAuthenticator((_, _, _) => true);
    }

    [Fact]
    public void SetDelegatePublicKeyAuthenticator()
    {
        _server.SetDelegatePublicKeyAuthenticator((_, _, _) => true);
    }

    [Fact]
    public void setFingerprintPublickeyAuthenticator()
    {
        _server.setFingerprintPublickeyAuthenticator("user", "SHA256:abc", "SHA256:xyz");
    }

    [Fact]
    public void SetFingerprintPublickeyAuthenticator()
    {
        _server.SetFingerprintPublickeyAuthenticator("user", "SHA256:abc", "SHA256:xyz");
    }

    [Fact]
    public void setFingerprintPublickeyAuthenticator_empty_throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _server.setFingerprintPublickeyAuthenticator("user"));
    }

    [Fact]
    public void SetFingerprintPublickeyAuthenticator_empty_throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _server.SetFingerprintPublickeyAuthenticator("user"));
    }

    [Fact]
    public void setCompositePublickeyAuthenticator()
    {
        _server.setCompositePublickeyAuthenticator(
            new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:a"),
            new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:b"));
    }

    [Fact]
    public void SetCompositePublickeyAuthenticator()
    {
        _server.SetCompositePublickeyAuthenticator(
            new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:a"),
            new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:b"));
    }

    [Fact]
    public void setAuthorizedkeyAuthenticator_with_instance()
    {
        _server.setAuthorizedkeyAuthenticator(new AMNetAuthorizedKeysAuthenticator("/tmp/authorized_keys"));
    }

    [Fact]
    public void SetAuthorizedkeyAuthenticator_with_instance()
    {
        _server.SetAuthorizedkeyAuthenticator(new AMNetAuthorizedKeysAuthenticator("/tmp/authorized_keys"));
    }

    [Fact]
    public void SetAuthorizedKeysAuthenticator_with_instance()
    {
        _server.SetAuthorizedKeysAuthenticator(new AMNetAuthorizedKeysAuthenticator("/tmp/authorized_keys"));
    }

    [Fact]
    public void setAuthorizedkeyAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.setAuthorizedkeyAuthenticator((IAMNetAuthorizedKeysAuthenticator)null!));
    }

    [Fact]
    public void SetAuthorizedkeyAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.SetAuthorizedkeyAuthenticator((IAMNetAuthorizedKeysAuthenticator)null!));
    }

    [Fact]
    public void setAuthorizedkeyAuthenticator_with_path()
    {
        _server.setAuthorizedkeyAuthenticator("/etc/ssh/authorized_keys");
    }

    [Fact]
    public void SetAuthorizedkeyAuthenticator_with_path()
    {
        _server.SetAuthorizedkeyAuthenticator("/etc/ssh/authorized_keys");
    }

    [Fact]
    public void SetAuthorizedKeysAuthenticator_with_path()
    {
        _server.SetAuthorizedKeysAuthenticator("/etc/ssh/authorized_keys");
    }

    [Fact]
    public void setKeyboardInteractiveAuthenticator_accepts()
    {
        _server.setKeyboardInteractiveAuthenticator(new AMNetKeyboardInteractiveAuthenticator());
    }

    [Fact]
    public void SetKeyboardInteractiveAuthenticator_accepts()
    {
        _server.SetKeyboardInteractiveAuthenticator(new AMNetKeyboardInteractiveAuthenticator());
    }

    [Fact]
    public void setKeyboardInteractiveAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.setKeyboardInteractiveAuthenticator(null!));
    }

    [Fact]
    public void SetKeyboardInteractiveAuthenticator_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.SetKeyboardInteractiveAuthenticator(null!));
    }

    [Fact]
    public void setDelegateKeyboardInteractiveAuthenticator()
    {
        _server.setDelegateKeyboardInteractiveAuthenticator(
            (_, _) => { },
            (_, _, _) => true);
    }

    [Fact]
    public void SetDelegateKeyboardInteractiveAuthenticator()
    {
        _server.SetDelegateKeyboardInteractiveAuthenticator(
            (_, _) => { },
            (_, _, _) => true);
    }

    [Fact]
    public void setFixedKeyboardInteractiveAuthenticator()
    {
        _server.setFixedKeyboardInteractiveAuthenticator("response");
    }

    [Fact]
    public void SetFixedKeyboardInteractiveAuthenticator()
    {
        _server.SetFixedKeyboardInteractiveAuthenticator("response");
    }

    [Fact]
    public void setServerProxyAcceptor_accepts()
    {
        _server.setServerProxyAcceptor(new AMNetServerProxyAcceptor());
    }

    [Fact]
    public void setServerProxyAcceptor_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.setServerProxyAcceptor(null!));
    }

    [Fact]
    public void setIoServiceEventListener_accepts()
    {
        _server.setIoServiceEventListener(new AMNetIoServiceEventListener());
    }

    [Fact]
    public void setIoServiceEventListener_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.setIoServiceEventListener(null!));
    }

    [Fact]
    public void addSessionListener_accepts()
    {
        _server.addSessionListener(new AMNetSessionListener());
    }

    [Fact]
    public void addSessionListener_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.addSessionListener(null!));
    }

    [Fact]
    public void setSubsystemFactories_accepts()
    {
        var factory = new AMNetSftpSubsystemFactory();
        _server.setSubsystemFactories(factory);
    }

    [Fact]
    public void setSubsystemFactories_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.setSubsystemFactories(null!));
    }

    [Fact]
    public void setCommandFactory_accepts()
    {
        var factory = new AMNetScpCommandFactory();
        _server.setCommandFactory(factory);
    }

    [Fact]
    public void setCommandFactory_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _server.setCommandFactory(null!));
    }

    [Fact]
    public void setAuthenticationMethods()
    {
        _server.setAuthenticationMethods(
            AMNetSshAuthenticationMethods.PublicKey,
            AMNetSshAuthenticationMethods.Password);
    }

    [Fact]
    public void setAuthenticationMethodGroups()
    {
        _server.setAuthenticationMethodGroups(
            new[] { AMNetSshAuthenticationMethods.PublicKey });
    }

    [Fact]
    public void GetConfiguredAuthenticationMethods_returns_chains()
    {
        _server.SetAuthenticationMethods(
            AMNetSshAuthenticationMethods.PublicKey,
            AMNetSshAuthenticationMethods.Password);
        var methods = _server.GetConfiguredAuthenticationMethods();
        Assert.Equal(2, methods.Count);
    }

    private void ConfigureForStart(AMNetSshServer server)
    {
        server.Port = 0;
        server.Host = "127.0.0.1";
        server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(
            Path.Combine(Path.GetTempPath(), "sshdtestkey_" + Guid.NewGuid())));
        server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(
            Path.GetTempPath()));
    }

    [Fact]
    public void Start_stop_lifecycle()
    {
        ConfigureForStart(_server);
        _server.Start();
        Assert.True(_server.IsStarted());
        Assert.False(_server.IsClosed());
        _server.Stop();
        Assert.True(_server.IsClosed());
    }

    [Fact]
    public void Start_stop_using_java_methods()
    {
        ConfigureForStart(_server);
        _server.start();
        Assert.True(_server.isStarted());
        _server.stop();
        Assert.True(_server.isClosed());
    }

    [Fact]
    public void Start_stop_immediately()
    {
        ConfigureForStart(_server);
        _server.Start();
        _server.Stop(true);
        Assert.True(_server.IsClosed());
    }

    [Fact]
    public void Dispose_stops_server()
    {
        var server = AMNetSshServer.SetUpDefaultServer();
        ConfigureForStart(server);
        server.Start();
        Assert.True(server.IsStarted());
        server.Dispose();
    }

    private sealed class AcceptAllForwardingFilter : IAMNetForwardingFilter
    {
        public bool CanListen(string host, int port, ISshSession session) => true;
        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session) => true;
        public bool CanForwardAgent(ISshSession session, string requestType) => true;
        public bool CanForwardX11(ISshSession session, string requestType) => true;
    }

    private sealed class AcceptAllTcpFilter : IAMNetTcpForwardingFilter
    {
        public bool CanListen(string host, int port, ISshSession session) => true;
        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session) => true;
    }

    private sealed class AcceptAllAgentFilter : IAMNetAgentForwardingFilter
    {
        public bool CanForwardAgent(ISshSession session, string requestType) => true;
    }

    private sealed class AcceptAllX11Filter : IAMNetX11ForwardingFilter
    {
        public bool CanForwardX11(ISshSession session, string requestType) => true;
    }

    [Fact]
    public void setForwardingFilter_accepts()
    {
        _server.setForwardingFilter(new AcceptAllForwardingFilter());
    }

    [Fact]
    public void SetForwardingFilter_accepts()
    {
        _server.SetForwardingFilter(new AcceptAllForwardingFilter());
    }

    [Fact]
    public void setForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setForwardingFilter(null!));
    }

    [Fact]
    public void SetForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.SetForwardingFilter(null!));
    }

    [Fact]
    public void setTcpForwardingFilter_accepts()
    {
        _server.setTcpForwardingFilter(new AcceptAllTcpFilter());
    }

    [Fact]
    public void SetTcpForwardingFilter_accepts()
    {
        _server.SetTcpForwardingFilter(new AcceptAllTcpFilter());
    }

    [Fact]
    public void setTcpForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setTcpForwardingFilter(null!));
    }

    [Fact]
    public void setTcpForwardingPolicy_accepts()
    {
        _server.setTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);
    }

    [Fact]
    public void SetTcpForwardingPolicy_accepts()
    {
        _server.SetTcpForwardingPolicy(AMNetTcpForwardingPolicy.All);
    }

    [Fact]
    public void setAgentForwardingFilter_accepts()
    {
        _server.setAgentForwardingFilter(new AcceptAllAgentFilter());
    }

    [Fact]
    public void SetAgentForwardingFilter_accepts()
    {
        _server.SetAgentForwardingFilter(new AcceptAllAgentFilter());
    }

    [Fact]
    public void setAgentForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setAgentForwardingFilter(null!));
    }

    [Fact]
    public void SetAgentForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.SetAgentForwardingFilter(null!));
    }

    [Fact]
    public void setX11ForwardingFilter_accepts()
    {
        _server.setX11ForwardingFilter(new AcceptAllX11Filter());
    }

    [Fact]
    public void SetX11ForwardingFilter_accepts()
    {
        _server.SetX11ForwardingFilter(new AcceptAllX11Filter());
    }

    [Fact]
    public void setX11ForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.setX11ForwardingFilter(null!));
    }

    [Fact]
    public void SetX11ForwardingFilter_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _server.SetX11ForwardingFilter(null!));
    }

    [Fact]
    public void SetAuthenticationMethodGroups_accepts()
    {
        _server.SetAuthenticationMethodGroups(
            new[] { AMNetSshAuthenticationMethods.PublicKey });
    }

    [Fact]
    public void SetRateLimiter_accepts()
    {
        _server.SetRateLimiter(new AMNetConnectionRateLimiter(10, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void SetRateLimiter_null_is_allowed()
    {
        _server.SetRateLimiter(null);
    }
}
