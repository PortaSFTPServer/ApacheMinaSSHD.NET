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

using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class VirtualFileSystemFactoryTests
{
    [Fact]
    public void Constructor_sets_base_path()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        Assert.Equal("/sftp/root", factory.BasePath);
        Assert.True(factory.CreateUserDirectory);
    }

    [Fact]
    public void Constructor_with_createUserDirectory_flag()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root", createUserDirectory: false);
        Assert.Equal("/sftp/root", factory.BasePath);
        Assert.False(factory.CreateUserDirectory);
    }

    [Fact]
    public void Constructor_empty_basePath_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetVirtualFileSystemFactory(""));
    }

    [Fact]
    public void Constructor_whitespace_basePath_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetVirtualFileSystemFactory("   "));
    }

    [Fact]
    public void Constructor_null_basePath_throws()
    {
        Assert.Throws<ArgumentException>(() => new AMNetVirtualFileSystemFactory(null!));
    }

    [Fact]
    public void ResolveUserHomeDirectory_combines_path()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        string home = factory.ResolveUserHomeDirectory("alice");
        Assert.Equal(Path.Combine("/sftp/root", "alice"), home);
    }

    [Fact]
    public void ResolveUserHomeDirectory_different_users()
    {
        var factory = new AMNetVirtualFileSystemFactory("/sftp/root");
        Assert.NotEqual(
            factory.ResolveUserHomeDirectory("alice"),
            factory.ResolveUserHomeDirectory("bob"));
    }

    [Fact]
    public void ResolveUserHomeDirectory_can_be_overridden()
    {
        var factory = new CustomFactory("/base");
        Assert.Equal("/custom/john", factory.ResolveUserHomeDirectory("john"));
    }

    private sealed class CustomFactory : AMNetVirtualFileSystemFactory
    {
        public CustomFactory(string basePath) : base(basePath) { }
        public override string ResolveUserHomeDirectory(string username) => $"/custom/{username}";
    }
}
