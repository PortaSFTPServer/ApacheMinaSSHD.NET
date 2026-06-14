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
public class PublicKeyAuthenticatorTests
{
    [Fact]
    public void FingerprintAuthenticator_correct_fingerprint()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.True(auth.Authenticate("user", "SHA256:abc123", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_wrong_fingerprint()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.False(auth.Authenticate("user", "SHA256:wrong", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_wrong_username()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("alice", "SHA256:abc123");
        Assert.False(auth.Authenticate("bob", "SHA256:abc123", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_multiple_fingerprints_per_user()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        auth.AddFingerprint("user", "SHA256:xyz789");
        Assert.True(auth.Authenticate("user", "SHA256:abc123", DummySession.Instance));
        Assert.True(auth.Authenticate("user", "SHA256:xyz789", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_empty_fingerprint_denies()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.False(auth.Authenticate("user", "", DummySession.Instance));
        Assert.False(auth.Authenticate("user", "   ", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_case_insensitive()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:ABC123");
        Assert.True(auth.Authenticate("user", "sha256:abc123", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_trim_whitespace()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("user", "SHA256:abc123");
        Assert.True(auth.Authenticate("user", "  SHA256:abc123  ", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_different_users_independent()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator("alice", "SHA256:abc");
        auth.AddFingerprint("bob", "SHA256:xyz");
        Assert.True(auth.Authenticate("alice", "SHA256:abc", DummySession.Instance));
        Assert.True(auth.Authenticate("bob", "SHA256:xyz", DummySession.Instance));
        Assert.False(auth.Authenticate("alice", "SHA256:xyz", DummySession.Instance));
        Assert.False(auth.Authenticate("bob", "SHA256:abc", DummySession.Instance));
    }

    [Fact]
    public void FingerprintAuthenticator_chaining()
    {
        var auth = new AMNetFingerprintPublickeyAuthenticator()
            .AddFingerprint("user", "SHA256:a")
            .AddFingerprint("user", "SHA256:b");
        Assert.True(auth.Authenticate("user", "SHA256:a", DummySession.Instance));
        Assert.True(auth.Authenticate("user", "SHA256:b", DummySession.Instance));
    }

    [Fact]
    public void DelegatePublickeyAuthenticator_calls_callback()
    {
        bool called = false;
        var auth = new AMNetDelegatePublickeyAuthenticator(
            (username, fingerprint, session) =>
            {
                called = true;
                return username == "user" && fingerprint == "SHA256:abc";
            });
        Assert.True(auth.Authenticate("user", "SHA256:abc", DummySession.Instance));
        Assert.True(called);
    }

    [Fact]
    public void DelegatePublickeyAuthenticator_null_callback_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetDelegatePublickeyAuthenticator(null!));
    }

    [Fact]
    public void CompositePublickeyAuthenticator_tries_in_order()
    {
        var alwaysNo = new AMNetDelegatePublickeyAuthenticator((_, _, _) => false);
        var alwaysYes = new AMNetDelegatePublickeyAuthenticator((_, _, _) => true);
        var composite = new AMNetCompositePublickeyAuthenticator(alwaysNo, alwaysYes);

        Assert.True(composite.Authenticate("user", "fp", DummySession.Instance));
    }

    [Fact]
    public void CompositePublickeyAuthenticator_all_deny()
    {
        var alwaysNo1 = new AMNetDelegatePublickeyAuthenticator((_, _, _) => false);
        var alwaysNo2 = new AMNetDelegatePublickeyAuthenticator((_, _, _) => false);
        var composite = new AMNetCompositePublickeyAuthenticator(alwaysNo1, alwaysNo2);

        Assert.False(composite.Authenticate("user", "fp", DummySession.Instance));
    }

    [Fact]
    public void CompositePublickeyAuthenticator_empty_creates_no_authenticators()
    {
        var composite = new AMNetCompositePublickeyAuthenticator();
        Assert.Empty(composite.Authenticators);
    }

    [Fact]
    public void CompositePublickeyAuthenticator_null_entry_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetCompositePublickeyAuthenticator(
            new IAMNetPublickeyAuthenticator[] { null! }));
    }

    [Fact]
    public void DefaultPublickeyAuthenticator_denies_all()
    {
        var auth = new AMNetPublickeyAuthenticator();
        Assert.False(auth.Authenticate("any", "SHA256:anything", DummySession.Instance));
    }

    private sealed class DummySession : ISshSession
    {
        public static readonly DummySession Instance = new();
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
    }
}
