using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class SimpleGeneratorHostKeyProviderTests
{
    [Fact]
    public void Constructor_default_values()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        Assert.Equal("", provider.KeyPath);
        Assert.Equal("RSA", provider.Algorithm);
        Assert.Equal(3072, provider.KeySize);
        Assert.True(provider.StrictFilePermissions);
    }

    [Fact]
    public void Constructor_with_keyPath()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider("/path/to/key");
        Assert.Equal("/path/to/key", provider.KeyPath);
    }

    [Fact]
    public void setAlgorithm_valid_value()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setAlgorithm("EC");
        Assert.Equal("EC", provider.Algorithm);
        Assert.Equal("EC", provider.getAlgorithm());
    }

    [Fact]
    public void setAlgorithm_empty_throws()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        Assert.Throws<ArgumentException>(() => provider.setAlgorithm(""));
    }

    [Fact]
    public void setAlgorithm_whitespace_throws()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        Assert.Throws<ArgumentException>(() => provider.setAlgorithm("   "));
    }

    [Fact]
    public void setAlgorithm_null_throws()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        Assert.Throws<ArgumentException>(() => provider.setAlgorithm(null!));
    }

    [Fact]
    public void setKeySize_valid()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setKeySize(4096);
        Assert.Equal(4096, provider.KeySize);
        Assert.Equal(4096, provider.getKeySize());
    }

    [Fact]
    public void setKeySize_zero_throws()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        Assert.Throws<ArgumentOutOfRangeException>(() => provider.setKeySize(0));
    }

    [Fact]
    public void setKeySize_negative_throws()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        Assert.Throws<ArgumentOutOfRangeException>(() => provider.setKeySize(-1));
    }

    [Fact]
    public void setStrictFilePermissions_true()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setStrictFilePermissions(false);
        Assert.False(provider.StrictFilePermissions);
        Assert.False(provider.hasStrictFilePermissions());
    }

    [Fact]
    public void setStrictFilePermissions_false()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.setStrictFilePermissions(false);
        provider.setStrictFilePermissions(true);
        Assert.True(provider.StrictFilePermissions);
        Assert.True(provider.hasStrictFilePermissions());
    }

    [Fact]
    public void Can_configure_before_conversion()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider("/path");
        provider.setAlgorithm("EdDSA");
        provider.setKeySize(4096);
        provider.setStrictFilePermissions(false);
        Assert.Equal("/path", provider.KeyPath);
        Assert.Equal("EdDSA", provider.Algorithm);
        Assert.Equal(4096, provider.KeySize);
        Assert.False(provider.StrictFilePermissions);
    }
}
