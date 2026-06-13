// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Helpers;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
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
