// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
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

    [Fact]
    public void Path_traversal_sequences_are_rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new AMNetSimpleGeneratorHostKeyProvider("../../etc/hostkey.ser"));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Path_traversal_with_backslashes_is_rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new AMNetSimpleGeneratorHostKeyProvider("..\\..\\etc\\hostkey.ser"));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Normal_absolute_path_is_accepted()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), "hostkey_" + Guid.NewGuid() + ".ser");
        try
        {
            var provider = new AMNetSimpleGeneratorHostKeyProvider(tempFile);
            Assert.Equal(tempFile, provider.KeyPath);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void Normal_relative_path_is_accepted()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser");
        Assert.Equal("hostkey.ser", provider.KeyPath);
    }

    [Fact]
    public void ResolveKeyPath_sets_resolved_path()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "KeyProviderTest_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        try
        {
            string keyPath = Path.Combine(tempDir, "hostkey.ser");
            var provider = new AMNetSimpleGeneratorHostKeyProvider(keyPath);
            Assert.Equal("", provider.ResolvedKeyPath);

            provider.ResolveKeyPath();

            Assert.Equal(Path.GetFullPath(keyPath), provider.ResolvedKeyPath);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    [Fact]
    public void Empty_keyPath_has_no_resolved_path()
    {
        var provider = new AMNetSimpleGeneratorHostKeyProvider();
        provider.ResolveKeyPath();
        Assert.Equal("", provider.ResolvedKeyPath);
    }

    [Fact]
    public void Symbolically_nested_path_is_resolved_canonically()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "KeyNestedTest_" + Guid.NewGuid());
        string subDir = Path.Combine(tempDir, "sub", "dir");
        Directory.CreateDirectory(subDir);
        try
        {
            // Use a path with ".." that stays within the temp dir
            string pathWithDotDot = Path.Combine(subDir, "..", "hostkey.ser");
            var ex = Assert.Throws<ArgumentException>(() =>
                new AMNetSimpleGeneratorHostKeyProvider(pathWithDotDot));
            Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
