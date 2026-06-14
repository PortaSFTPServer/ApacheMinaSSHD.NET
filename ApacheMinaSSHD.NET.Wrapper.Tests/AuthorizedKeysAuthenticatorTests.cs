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
