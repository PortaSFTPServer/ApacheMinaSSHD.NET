using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class SftpSubsystemFactoryTests
{
    [Fact]
    public void Constructor()
    {
        var factory = new AMNetSftpSubsystemFactory();
        Assert.NotNull(factory);
    }

    [Fact]
    public void addSftpEventListener_accepts()
    {
        var factory = new AMNetSftpSubsystemFactory();
        factory.addSftpEventListener(new AMNetSftpEventListener());
    }

    [Fact]
    public void addSftpEventListener_null_throws()
    {
        var factory = new AMNetSftpSubsystemFactory();
        Assert.Throws<ArgumentNullException>(() => factory.addSftpEventListener(null!));
    }

    [Fact]
    public void setFileSystemAccessor_accepts()
    {
        var factory = new AMNetSftpSubsystemFactory();
        factory.setFileSystemAccessor(new AMNetSftpFileSystemAccessor());
    }

    [Fact]
    public void setFileSystemAccessor_null_throws()
    {
        var factory = new AMNetSftpSubsystemFactory();
        Assert.Throws<ArgumentNullException>(() => factory.setFileSystemAccessor(null!));
    }
}
