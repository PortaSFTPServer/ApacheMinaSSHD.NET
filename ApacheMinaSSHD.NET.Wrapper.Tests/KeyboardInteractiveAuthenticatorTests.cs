// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class KeyboardInteractiveAuthenticatorTests
{
    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_correct_response()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("secret123");
        var challenge = new DummyChallenge();
        auth.GenerateChallenge("user", challenge);

        Assert.Equal("Verification code", challenge.Prompts[0].Prompt);
        Assert.False(challenge.Prompts[0].Echo);

        var response = new DummyResponseList(["secret123"]);
        Assert.True(auth.Authenticate(DummySession.Instance, "user", response));
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_wrong_response()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("secret123");
        var challenge = new DummyChallenge();
        auth.GenerateChallenge("user", challenge);

        var response = new DummyResponseList(["wrong"]);
        Assert.False(auth.Authenticate(DummySession.Instance, "user", response));
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_username_filter()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("secret", username: "specificUser");
        var response = new DummyResponseList(["secret"]);

        Assert.True(auth.Authenticate(DummySession.Instance, "specificUser", response));
        Assert.False(auth.Authenticate(DummySession.Instance, "otherUser", response));
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_null_expectedResponse_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetFixedKeyboardInteractiveAuthenticator(null!));
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_constant_time_comparison()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("secret");

        Assert.False(auth.Authenticate(DummySession.Instance, "user", new DummyResponseList(["secre"])));
        Assert.False(auth.Authenticate(DummySession.Instance, "user", new DummyResponseList(["secrett"])));
        Assert.True(auth.Authenticate(DummySession.Instance, "user", new DummyResponseList(["secret"])));
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_custom_prompt()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("pass", prompt: "Token:");
        var challenge = new DummyChallenge();
        auth.GenerateChallenge("user", challenge);

        Assert.Equal("Token:", challenge.Prompts[0].Prompt);
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_custom_interaction_name()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("pass", interactionName: "MFA");
        var challenge = new DummyChallenge();
        auth.GenerateChallenge("user", challenge);

        Assert.Equal("MFA", challenge.InteractionName);
    }

    [Fact]
    public void FixedKeyboardInteractiveAuthenticator_empty_prompt_falls_back()
    {
        var auth = new AMNetFixedKeyboardInteractiveAuthenticator("pass", prompt: "");
        var challenge = new DummyChallenge();
        auth.GenerateChallenge("user", challenge);

        Assert.Equal("Verification code", challenge.Prompts[0].Prompt);
    }

    [Fact]
    public void DelegateKeyboardInteractiveAuthenticator_calls_both_callbacks()
    {
        bool genCalled = false;
        bool authCalled = false;

        var auth = new AMNetDelegateKeyboardInteractiveAuthenticator(
            (username, challenge) =>
            {
                genCalled = true;
                challenge.AddPrompt("Code:", echo: false);
            },
            (session, username, response) =>
            {
                authCalled = true;
                var list = response.GetResponses();
                return list is ["pass"];
            });

        var challenge = new DummyChallenge();
        auth.GenerateChallenge("user", challenge);
        Assert.True(genCalled);
        Assert.Equal("Code:", challenge.Prompts[0].Prompt);

        Assert.True(auth.Authenticate(DummySession.Instance, "user", new DummyResponseList(["pass"])));
        Assert.True(authCalled);
    }

    [Fact]
    public void DelegateKeyboardInteractiveAuthenticator_null_generateChallenge_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetDelegateKeyboardInteractiveAuthenticator(null!, (_, _, _) => true));
    }

    [Fact]
    public void DelegateKeyboardInteractiveAuthenticator_null_authenticate_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetDelegateKeyboardInteractiveAuthenticator((_, _) => { }, null!));
    }

    [Fact]
    public void DefaultKeyboardInteractiveAuthenticator_denies_all()
    {
        var auth = new AMNetKeyboardInteractiveAuthenticator();
        Assert.False(auth.Authenticate(DummySession.Instance, "user", new DummyResponseList(["anything"])));
    }

    private sealed class DummyChallenge : ISshChallenge
    {
        private readonly List<(string Prompt, bool Echo)> prompts = [];
        public IReadOnlyList<(string Prompt, bool Echo)> Prompts => prompts.AsReadOnly();
        public string InteractionName { get; set; } = "";
        public string InteractionInstruction { get; set; } = "";
        public string LanguageTag { get; set; } = "";

        public void AddPrompt(string prompt, bool echo = false)
        {
            prompts.Add((prompt, echo));
        }
    }

    private sealed class DummyResponseList : IResponseList
    {
        private readonly List<string> responses;
        public DummyResponseList(params string[] responses) => this.responses = [.. responses];
        public List<string> GetResponses() => [.. responses];
    }

    private sealed class DummySession : ISshSession
    {
        public static readonly DummySession Instance = new();
        public Guid SessionId => Guid.Empty;
        public string RemoteAddress => "127.0.0.1";
    }
}
