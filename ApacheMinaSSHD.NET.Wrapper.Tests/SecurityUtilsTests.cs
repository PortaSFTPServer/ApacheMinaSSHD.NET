using ApacheMinaSSHD.NET.Helpers;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

public class SecurityUtilsTests
{
    [Fact]
    public void SetFipsMode_false_does_not_throw()
    {
        AMNSecurityUtils.SetFipsMode(false);
    }

    [Fact]
    public void SetFipsMode_true_does_not_throw()
    {
        AMNSecurityUtils.SetFipsMode(true);
    }
}
