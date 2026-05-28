using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

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
