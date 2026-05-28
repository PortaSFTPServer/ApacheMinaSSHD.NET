namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class AuthenticationMethodsTests
{
    [Fact]
    public void RequireAll_single_method()
    {
        string result = AMNetSshAuthenticationMethods.RequireAll(AMNetSshAuthenticationMethods.PublicKey);
        Assert.Equal("publickey", result);
    }

    [Fact]
    public void RequireAll_multiple_methods()
    {
        string result = AMNetSshAuthenticationMethods.RequireAll(
            AMNetSshAuthenticationMethods.Password,
            AMNetSshAuthenticationMethods.KeyboardInteractive);
        Assert.Equal("password,keyboard-interactive", result);
    }

    [Fact]
    public void RequireAll_empty_throws()
    {
        Assert.Throws<ArgumentException>(() => AMNetSshAuthenticationMethods.RequireAll(Array.Empty<string>()));
    }

    [Fact]
    public void RequireAll_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => AMNetSshAuthenticationMethods.RequireAll((string[])null!));
    }

    [Fact]
    public void AllowAny_single_chain()
    {
        string result = AMNetSshAuthenticationMethods.AllowAny(AMNetSshAuthenticationMethods.PublicKey);
        Assert.Equal("publickey", result);
    }

    [Fact]
    public void AllowAny_multiple_chains()
    {
        string result = AMNetSshAuthenticationMethods.AllowAny(
            AMNetSshAuthenticationMethods.PublicKey,
            AMNetSshAuthenticationMethods.RequireAll(
                AMNetSshAuthenticationMethods.Password,
                AMNetSshAuthenticationMethods.KeyboardInteractive));
        Assert.Equal("publickey password,keyboard-interactive", result);
    }

    [Fact]
    public void AllowAny_empty_throws()
    {
        Assert.Throws<ArgumentException>(() => AMNetSshAuthenticationMethods.AllowAny(Array.Empty<string>()));
    }

    [Fact]
    public void Parse_empty_returns_empty()
    {
        var result = AMNetSshAuthenticationMethods.Parse(null);
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_whitespace_returns_empty()
    {
        var result = AMNetSshAuthenticationMethods.Parse("   ");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_single_chain()
    {
        var result = AMNetSshAuthenticationMethods.Parse("publickey");
        Assert.Single(result);
        Assert.Equal(["publickey"], result[0]);
    }

    [Fact]
    public void Parse_multiple_alternatives()
    {
        var result = AMNetSshAuthenticationMethods.Parse("publickey password,keyboard-interactive");
        Assert.Equal(2, result.Count);
        Assert.Equal(["publickey"], result[0]);
        Assert.Equal(["password", "keyboard-interactive"], result[1]);
    }

    [Fact]
    public void Parse_comma_separated_chain()
    {
        var result = AMNetSshAuthenticationMethods.Parse("publickey,password");
        Assert.Single(result);
        Assert.Equal(["publickey", "password"], result[0]);
    }

    [Fact]
    public void Parse_extra_whitespace_around_commas_filtered()
    {
        var result = AMNetSshAuthenticationMethods.Parse("publickey , password");
        Assert.Equal(3, result.Count);
        Assert.Equal(["publickey"], result[0]);
        Assert.Empty(result[1]);
        Assert.Equal(["password"], result[2]);
    }

    [Fact]
    public void Constants_have_expected_values()
    {
        Assert.Equal("password", AMNetSshAuthenticationMethods.Password);
        Assert.Equal("publickey", AMNetSshAuthenticationMethods.PublicKey);
        Assert.Equal("keyboard-interactive", AMNetSshAuthenticationMethods.KeyboardInteractive);
    }
}
