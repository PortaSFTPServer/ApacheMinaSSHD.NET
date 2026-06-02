using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class ScpCommandFactoryTests
{
    [Fact]
    public void Constructor_default()
    {
        var factory = new AMNetScpCommandFactory();
        Assert.NotNull(factory);
    }

    [Fact]
    public void Constructor_with_fileOpener()
    {
        var factory = new AMNetScpCommandFactory(new AMNetScpFileOpener());
        Assert.NotNull(factory);
    }

    [Fact]
    public void Constructor_null_fileOpener_throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AMNetScpCommandFactory(null!));
    }

    [Fact]
    public void SendBufferSize_default_not_zero()
    {
        var factory = new AMNetScpCommandFactory();
        Assert.True(factory.SendBufferSize > 0);
    }

    [Fact]
    public void SendBufferSize_roundtrip()
    {
        var factory = new AMNetScpCommandFactory();
        factory.SendBufferSize = 65536;
        Assert.Equal(65536, factory.SendBufferSize);
    }

    [Fact]
    public void ReceiveBufferSize_default_not_zero()
    {
        var factory = new AMNetScpCommandFactory();
        Assert.True(factory.ReceiveBufferSize > 0);
    }

    [Fact]
    public void ReceiveBufferSize_roundtrip()
    {
        var factory = new AMNetScpCommandFactory();
        factory.ReceiveBufferSize = 32768;
        Assert.Equal(32768, factory.ReceiveBufferSize);
    }

    [Fact]
    public void addEventListener_returns_true()
    {
        var factory = new AMNetScpCommandFactory();
        var listener = new AMNetScpTransferEventListener();
        Assert.True(factory.addEventListener(listener));
    }

    [Fact]
    public void addEventListener_null_throws()
    {
        var factory = new AMNetScpCommandFactory();
        Assert.Throws<ArgumentNullException>(() => factory.addEventListener(null!));
    }

    [Fact]
    public void addEventListener_duplicate_returns_false()
    {
        var factory = new AMNetScpCommandFactory();
        var listener = new AMNetScpTransferEventListener();
        Assert.True(factory.addEventListener(listener));
        Assert.False(factory.addEventListener(listener));
    }

    [Fact]
    public void removeEventListener_returns_true()
    {
        var factory = new AMNetScpCommandFactory();
        var listener = new AMNetScpTransferEventListener();
        factory.addEventListener(listener);
        Assert.True(factory.removeEventListener(listener));
    }

    [Fact]
    public void removeEventListener_not_registered_returns_false()
    {
        var factory = new AMNetScpCommandFactory();
        var listener = new AMNetScpTransferEventListener();
        Assert.False(factory.removeEventListener(listener));
    }

    [Fact]
    public void removeEventListener_null_throws()
    {
        var factory = new AMNetScpCommandFactory();
        Assert.Throws<ArgumentNullException>(() => factory.removeEventListener(null!));
    }

    [Fact]
    public void setFileOpener_accepts_opener()
    {
        var factory = new AMNetScpCommandFactory();
        factory.setFileOpener(new AMNetScpFileOpener("/root"));
    }

    [Fact]
    public void setFileOpener_null_throws()
    {
        var factory = new AMNetScpCommandFactory();
        Assert.Throws<ArgumentNullException>(() => factory.setFileOpener(null!));
    }
}
