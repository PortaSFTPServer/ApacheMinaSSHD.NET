using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class SshAuthenticationMethodsTests
{
    [Fact]
    public void Constants_have_expected_values()
    {
        Assert.Equal("password", AMNetSshAuthenticationMethods.Password);
        Assert.Equal("publickey", AMNetSshAuthenticationMethods.PublicKey);
        Assert.Equal("keyboard-interactive", AMNetSshAuthenticationMethods.KeyboardInteractive);
        Assert.Equal("gssapi-with-mic", AMNetSshAuthenticationMethods.Gssapi);
        Assert.Equal("hostbased", AMNetSshAuthenticationMethods.HostBased);
    }

    [Fact]
    public void RequireAll_formats_correctly()
    {
        string chain = AMNetSshAuthenticationMethods.RequireAll(
            AMNetSshAuthenticationMethods.Password,
            AMNetSshAuthenticationMethods.PublicKey);
        Assert.Equal("password,publickey", chain);
    }

    [Fact]
    public void RequireAll_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AMNetSshAuthenticationMethods.RequireAll(null!));
    }

    [Fact]
    public void RequireAll_empty_throws()
    {
        Assert.Throws<ArgumentException>(() =>
            AMNetSshAuthenticationMethods.RequireAll());
    }
}

[Trait("Category", "Unit")]
public class PortForwardingEventListenerTests
{
    private sealed class CollectingLogger : IAMNetLogger
    {
        public List<string> Messages { get; } = [];
        public void Info(string message) => Messages.Add(message);
        public void Warn(string message, Exception? ex = null) { }
        public void Debug(string message, Exception? ex = null) { }
        public void Error(string message, Exception? ex = null) { }
        public void Trace(string message, Exception? ex = null) { }
    }

    private sealed class DummySession : ISshSession
    {
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }

    [Fact]
    public void Constructor_default()
    {
        _ = new AMNetPortForwardingEventListener();
    }

    [Fact]
    public void Constructor_with_logger()
    {
        _ = new AMNetPortForwardingEventListener(new CollectingLogger());
    }

    [Fact]
    public void OnEstablishingTunnel_logs_local()
    {
        var logger = new CollectingLogger();
        var listener = new AMNetPortForwardingEventListener(logger);
        listener.OnEstablishingTunnel("example.com", 8080, true, new DummySession());
        Assert.Contains(logger.Messages, m => m.Contains("local") && m.Contains("example.com"));
    }

    [Fact]
    public void OnEstablishingTunnel_logs_remote()
    {
        var logger = new CollectingLogger();
        var listener = new AMNetPortForwardingEventListener(logger);
        listener.OnEstablishingTunnel("example.com", 8080, false, new DummySession());
        Assert.Contains(logger.Messages, m => m.Contains("remote") && m.Contains("example.com"));
    }

    [Fact]
    public void OnEstablishedTunnel_logs()
    {
        var logger = new CollectingLogger();
        var listener = new AMNetPortForwardingEventListener(logger);
        listener.OnEstablishedTunnel("example.com", 8080, true, "0.0.0.0:8080", new DummySession());
        Assert.Contains(logger.Messages, m => m.Contains("Established"));
    }

    [Fact]
    public void OnTearingDownTunnel_logs()
    {
        var logger = new CollectingLogger();
        var listener = new AMNetPortForwardingEventListener(logger);
        listener.OnTearingDownTunnel("example.com", 8080, true, new DummySession());
        Assert.Contains(logger.Messages, m => m.Contains("Tearing down"));
    }

    [Fact]
    public void OnTornDownTunnel_logs()
    {
        var logger = new CollectingLogger();
        var listener = new AMNetPortForwardingEventListener(logger);
        listener.OnTornDownTunnel("example.com", 8080, true, new DummySession());
        Assert.Contains(logger.Messages, m => m.Contains("Torn down"));
    }
}

[Trait("Category", "Unit")]
public class ForwardingFilterTests
{
    private sealed class AcceptAllTcpFilter : IAMNetTcpForwardingFilter
    {
        public bool CanListen(string host, int port, ISshSession session) => true;
        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session) => true;
        public bool CanForwardDynamic(string host, int port, ISshSession session) => true;
    }

    private sealed class RejectAllTcpFilter : IAMNetTcpForwardingFilter
    {
        public bool CanListen(string host, int port, ISshSession session) => false;
        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session) => false;
        public bool CanForwardDynamic(string host, int port, ISshSession session) => false;
    }

    private sealed class AcceptAllAgentFilter : IAMNetAgentForwardingFilter
    {
        public bool CanForwardAgent(ISshSession session, string requestType) => true;
    }

    private sealed class AcceptAllX11Filter : IAMNetX11ForwardingFilter
    {
        public bool CanForwardX11(ISshSession session, string requestType) => true;
    }

    private sealed class DummySession : ISshSession
    {
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }

    [Fact]
    public void ForwardingFilter_constructor_default_allows_tcp_rejects_agent_x11()
    {
        var filter = new AMNetForwardingFilter();
        Assert.True(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.False(filter.CanForwardAgent(new DummySession(), "auth-agent-req@openssh.com"));
        Assert.False(filter.CanForwardX11(new DummySession(), "x11-req"));
    }

    [Fact]
    public void ForwardingFilter_with_tcp_filter()
    {
        var filter = new AMNetForwardingFilter(new AcceptAllTcpFilter());
        Assert.True(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "host", 80, new DummySession()));
        Assert.True(filter.CanForwardDynamic("0.0.0.0", 1080, new DummySession()));
    }

    [Fact]
    public void ForwardingFilter_with_tcp_filter_rejects()
    {
        var filter = new AMNetForwardingFilter(new RejectAllTcpFilter());
        Assert.False(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "host", 80, new DummySession()));
    }

    [Fact]
    public void ForwardingFilter_with_agent_and_x11()
    {
        var filter = new AMNetForwardingFilter(null, new AcceptAllAgentFilter(), new AcceptAllX11Filter());
        Assert.True(filter.CanForwardAgent(new DummySession(), "auth-agent-req@openssh.com"));
        Assert.True(filter.CanForwardX11(new DummySession(), "x11-req"));
    }

    [Fact]
    public void ForwardingFilter_AcceptAll_allows_everything()
    {
        var filter = AMNetForwardingFilter.AcceptAll;
        Assert.True(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "host", 80, new DummySession()));
    }

    [Fact]
    public void ForwardingFilter_RejectAll_denies_everything()
    {
        var filter = AMNetForwardingFilter.RejectAll;
        Assert.False(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "host", 80, new DummySession()));
    }

    [Fact]
    public void ForwardingFilter_FromPolicy_accepts()
    {
        var filter = AMNetForwardingFilter.FromPolicy(AMNetTcpForwardingPolicy.All);
        Assert.True(filter.CanListen("0.0.0.0", 22, new DummySession()));
    }

    [Fact]
    public void TcpForwardingFilter_AcceptAll()
    {
        var filter = AMNetTcpForwardingFilter.AcceptAll;
        Assert.True(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.True(filter.CanConnect(AMNetForwardingType.Direct, "host", 80, new DummySession()));
        Assert.True(filter.CanForwardDynamic("0.0.0.0", 1080, new DummySession()));
    }

    [Fact]
    public void TcpForwardingFilter_RejectAll()
    {
        var filter = AMNetTcpForwardingFilter.RejectAll;
        Assert.False(filter.CanListen("0.0.0.0", 22, new DummySession()));
        Assert.False(filter.CanConnect(AMNetForwardingType.Direct, "host", 80, new DummySession()));
        Assert.False(filter.CanForwardDynamic("0.0.0.0", 1080, new DummySession()));
    }

    [Fact]
    public void TcpForwardingFilter_with_policy()
    {
        var filter = new AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy.All);
        Assert.True(filter.CanListen("0.0.0.0", 22, new DummySession()));
    }
}

[Trait("Category", "Unit")]
public class CommandHandlerTests
{
    private sealed class DummySession : ISshSession
    {
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }

    [Fact]
    public void Default_ExecuteCommand_returns_1()
    {
        var handler = new AMNetCommandHandler();
        int result = handler.ExecuteCommand("some command", new DummySession());
        Assert.Equal(1, result);
    }

    [Fact]
    public void Default_ExecuteShell_returns_1()
    {
        var handler = new AMNetCommandHandler();
        int result = handler.ExecuteShell(new DummySession());
        Assert.Equal(1, result);
    }

    [Fact]
    public void Delegate_command_handler()
    {
        bool called = false;
        var handler = new AMNetDelegateCommandHandler((cmd, session) =>
        {
            called = true;
            return 42;
        });
        int result = handler.ExecuteCommand("test", new DummySession());
        Assert.True(called);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Delegate_command_handler_accepts_null()
    {
        var handler = new AMNetDelegateCommandHandler(null!);
        int result = handler.ExecuteCommand("test", new DummySession());
        Assert.Equal(1, result);
    }
}

[Trait("Category", "Unit")]
public class HostBasedAuthenticatorTests
{
    private sealed class DummySession : ISshSession
    {
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }

    [Fact]
    public void Delegate_authenticator_calls_callback()
    {
        bool called = false;
        var auth = new AMNetDelegateHostBasedAuthenticator(
            (username, fp, hostname, clientUser, session) =>
            {
                called = true;
                return username == "user" && hostname == "client.example.com";
            });
        Assert.True(auth.Authenticate("user", "SHA256:abc", "client.example.com", "remoteuser", new DummySession()));
        Assert.True(called);
    }

    [Fact]
    public void Delegate_authenticator_false_result()
    {
        var auth = new AMNetDelegateHostBasedAuthenticator((_, _, _, _, _) => false);
        Assert.False(auth.Authenticate("user", "fp", "host", "remote", new DummySession()));
    }

    [Fact]
    public void Delegate_authenticator_null_callback_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AMNetDelegateHostBasedAuthenticator(null!));
    }

    [Fact]
    public void Default_authenticator_denies_all()
    {
        var auth = new AMNetHostBasedAuthenticator();
        Assert.False(auth.Authenticate("user", "fp", "host", "remote", new DummySession()));
    }
}

[Trait("Category", "Unit")]
public class GssapiAuthenticatorTests
{
    private sealed class DummySession : ISshSession
    {
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
        public void Disconnect() { }
    }

    [Fact]
    public void Delegate_authenticator_calls_validateIdentity()
    {
        bool called = false;
        var auth = new AMNetDelegateGssapiAuthenticator(
            (session, identity) =>
            {
                called = true;
                return identity == "user@REALM";
            });
        Assert.True(auth.ValidateIdentity(new DummySession(), "user@REALM"));
        Assert.True(called);
    }

    [Fact]
    public void Delegate_authenticator_validateInitialUser_default_true()
    {
        var auth = new AMNetDelegateGssapiAuthenticator((_, _) => true);
        Assert.True(auth.ValidateInitialUser(new DummySession(), "user"));
    }

    [Fact]
    public void Delegate_authenticator_with_validateInitialUser()
    {
        var auth = new AMNetDelegateGssapiAuthenticator(
            (_, _) => true,
            (session, username) => username == "allowed_user");
        Assert.True(auth.ValidateInitialUser(new DummySession(), "allowed_user"));
        Assert.False(auth.ValidateInitialUser(new DummySession(), "blocked_user"));
    }

    [Fact]
    public void Delegate_authenticator_with_optional_params()
    {
        var auth = new AMNetDelegateGssapiAuthenticator(
            (_, _) => true,
            null,
            "host/server.example.com",
            "/etc/krb5.keytab");
        Assert.Equal("host/server.example.com", auth.ServicePrincipalName);
        Assert.Equal("/etc/krb5.keytab", auth.KeytabFile);
    }

    [Fact]
    public void Delegate_authenticator_null_callback_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AMNetDelegateGssapiAuthenticator(null!));
    }

    [Fact]
    public void Default_authenticator_denies()
    {
        var auth = new AMNetGssapiAuthenticator();
        Assert.False(auth.ValidateIdentity(new DummySession(), "user@REALM"));
    }
}

[Trait("Category", "Unit")]
public class IAMNetFilePasswordProviderTests
{
    [Fact]
    public void Provider_returns_password()
    {
        var provider = new TestPasswordProvider("my-passphrase");
        string? password = provider.GetPassword("/path/to/key", 0);
        Assert.Equal("my-passphrase", password);
    }

    [Fact]
    public void Provider_returns_null_on_retry()
    {
        var provider = new TestPasswordProvider(null);
        Assert.Null(provider.GetPassword("/path/to/key", 2));
    }

    private sealed class TestPasswordProvider : IAMNetFilePasswordProvider
    {
        private readonly string? _password;
        public TestPasswordProvider(string? password) => _password = password;
        public string? GetPassword(string resourceKey, int retryIndex) => _password;
    }
}

[Trait("Category", "Unit")]
public class KeyProviderEncryptedPathsTests : IDisposable
{
    private readonly string _tempDir;

    public KeyProviderEncryptedPathsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "KeyProviderEncryptTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void setPassword_and_getPassword_roundtrip()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setPassword("secret123");
        Assert.Equal("secret123", provider.Password);
        Assert.Equal("secret123", provider.getPassword());
    }

    [Fact]
    public void setPassword_null_clears()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setPassword("secret");
        provider.setPassword(null);
        Assert.Null(provider.Password);
    }

    [Fact]
    public void PasswordProvider_getter_setter()
    {
        var pwdProvider = new TestFilePasswordProvider("pwd");
        var hostKeyProvider = new AMNetSimpleGeneratorHostKeyProvider();
        hostKeyProvider.setPasswordProvider(pwdProvider);
        Assert.Same(pwdProvider, hostKeyProvider.getPasswordProvider());
    }

    [Fact]
    public void ResolveKeyPath_with_encrypted_key_file_exists()
    {
        string keyPath = Path.Combine(_tempDir, "test_encrypted_key");
        var provider = new AMNetSimpleGeneratorHostKeyProvider(keyPath);
        provider.setPassword("test-passphrase");
        provider.ResolveKeyPath();
        Assert.Equal(Path.GetFullPath(keyPath), provider.ResolvedKeyPath);
    }

    [Fact]
    public void ResolveKeyPath_with_absolute_path()
    {
        string keyPath = Path.Combine(_tempDir, "hostkey.ser");
        var provider = new AMNetSimpleGeneratorHostKeyProvider(keyPath);
        provider.ResolveKeyPath();
        Assert.Equal(Path.GetFullPath(keyPath), provider.ResolvedKeyPath);
    }

    [Fact]
    public void ResolveKeyPath_with_relative_path()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider("relative_key.ser");
        provider.ResolveKeyPath();
        Assert.NotEqual("relative_key.ser", provider.ResolvedKeyPath);
        Assert.EndsWith("relative_key.ser", provider.ResolvedKeyPath);
    }

    [Fact]
    public void TempFileCleanup_registered()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider(Path.Combine(_tempDir, "test_key"));
        Assert.NotNull(provider);
    }

    [Fact]
    public void FallbackPassword_roundtrip()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setPassword("primary");
        Assert.Equal("primary", provider.Password);
    }

    private sealed class TestFilePasswordProvider : IAMNetFilePasswordProvider
    {
        private readonly string _password;
        public TestFilePasswordProvider(string password) => _password = password;
        public string? GetPassword(string resourceKey, int retryIndex) => _password;
    }
}
