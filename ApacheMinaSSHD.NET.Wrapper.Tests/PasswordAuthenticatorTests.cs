// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class PasswordAuthenticatorTests
{
    [Fact]
    public void FixedPasswordAuthenticator_correct_credentials()
    {
        var auth = new AMNetFixedPasswordAuthenticator("testuser", "testpass");
        Assert.True(auth.Authenticate("testuser", "testpass", DummySession.Instance));
    }

    [Fact]
    public void FixedPasswordAuthenticator_wrong_password()
    {
        var auth = new AMNetFixedPasswordAuthenticator("testuser", "testpass");
        Assert.False(auth.Authenticate("testuser", "wrongpass", DummySession.Instance));
    }

    [Fact]
    public void FixedPasswordAuthenticator_wrong_username()
    {
        var auth = new AMNetFixedPasswordAuthenticator("testuser", "testpass");
        Assert.False(auth.Authenticate("otheruser", "testpass", DummySession.Instance));
    }

    [Fact]
    public void FixedPasswordAuthenticator_constant_time_comparison()
    {
        var auth = new AMNetFixedPasswordAuthenticator("user", "secret");
        Assert.False(auth.Authenticate("user", "secrett", DummySession.Instance));
        Assert.False(auth.Authenticate("user", "secre", DummySession.Instance));
        Assert.True(auth.Authenticate("user", "secret", DummySession.Instance));
    }

    [Fact]
    public void FixedPasswordAuthenticator_null_username_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetFixedPasswordAuthenticator("", "pass"));
        Assert.Throws<ArgumentException>(() => new AMNetFixedPasswordAuthenticator("  ", "pass"));
    }

    [Fact]
    public void FixedPasswordAuthenticator_null_password_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetFixedPasswordAuthenticator("user", null!));
    }

    [Fact]
    public void DelegatePasswordAuthenticator_calls_callback()
    {
        bool called = false;
        var auth = new AMNetDelegatePasswordAuthenticator(
            (username, password, session) =>
            {
                called = true;
                return username == "user" && password == "pass";
            });
        Assert.True(auth.Authenticate("user", "pass", DummySession.Instance));
        Assert.True(called);
    }

    [Fact]
    public void DelegatePasswordAuthenticator_false_result()
    {
        var auth = new AMNetDelegatePasswordAuthenticator((_, _, _) => false);
        Assert.False(auth.Authenticate("user", "pass", DummySession.Instance));
    }

    [Fact]
    public void DelegatePasswordAuthenticator_null_callback_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetDelegatePasswordAuthenticator(null!));
    }

    [Fact]
    public void CompositePasswordAuthenticator_tries_in_order()
    {
        var first = new AMNetFixedPasswordAuthenticator("user", "first");
        var second = new AMNetFixedPasswordAuthenticator("user", "second");
        var composite = new AMNetCompositePasswordAuthenticator(first, second);

        Assert.True(composite.Authenticate("user", "first", DummySession.Instance));
        Assert.True(composite.Authenticate("user", "second", DummySession.Instance));
        Assert.False(composite.Authenticate("user", "none", DummySession.Instance));
    }

    [Fact]
    public void CompositePasswordAuthenticator_empty_creates_no_authenticators()
    {
        var composite = new AMNetCompositePasswordAuthenticator();
        Assert.Empty(composite.Authenticators);
    }

    [Fact]
    public void CompositePasswordAuthenticator_null_entry_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetCompositePasswordAuthenticator(
            new IAMNetPasswordAuthenticator[] { null! }));
    }

    [Fact]
    public void DefaultPasswordAuthenticator_denies_all()
    {
        var auth = new AMNetPasswordAuthenticator();
        Assert.False(auth.Authenticate("any", "any", DummySession.Instance));
    }

    private sealed class DummySession : ISshSession
    {
        public static readonly DummySession Instance = new();
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
    }
}
