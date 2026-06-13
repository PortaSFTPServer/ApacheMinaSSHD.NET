// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class AuthorizedKeysAuthenticatorTests
{
    [Fact]
    public void Constructor_resolves_full_path()
    {
        string relativePath = "authorized_keys";
        var auth = new AMNetAuthorizedKeysAuthenticator(relativePath);
        Assert.True(Path.IsPathRooted(auth.KeysFilePath));
        Assert.EndsWith("authorized_keys", auth.KeysFilePath);
    }

    [Fact]
    public void Constructor_empty_path_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetAuthorizedKeysAuthenticator(""));
    }

    [Fact]
    public void Constructor_whitespace_path_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetAuthorizedKeysAuthenticator("   "));
    }

    [Fact]
    public void Constructor_null_path_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetAuthorizedKeysAuthenticator(null!));
    }

    [Fact]
    public void Constructor_absolute_path_preserved()
    {
        string absPath = Path.GetFullPath(".");
        var auth = new AMNetAuthorizedKeysAuthenticator(absPath);
        Assert.Equal(absPath, auth.KeysFilePath);
    }

    [Fact]
    public void FromFile_returns_config()
    {
        var auth = AMNetAuthorizedKeysAuthenticator.FromFile("test_keys");
        Assert.NotNull(auth);
        Assert.EndsWith("test_keys", auth.KeysFilePath);
    }

    [Fact]
    public void Multiple_instances_have_independent_paths()
    {
        var auth1 = new AMNetAuthorizedKeysAuthenticator("keys1");
        var auth2 = new AMNetAuthorizedKeysAuthenticator("keys2");
        Assert.NotEqual(auth1.KeysFilePath, auth2.KeysFilePath);
    }
}
