using org.apache.sshd.common.util.security;
using org.apache.sshd.common.util.security.bouncycastle;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Helpers
{
    /// <summary>
    /// Provides security-provider helper methods for the SSH runtime.
    /// </summary>
    public static class AMNSecurityUtils
    {
        /// <summary>
        /// Enables or disables FIPS-oriented provider selection.
        /// </summary>
        /// <param name="state">
        /// <c>true</c> to prefer the BCFIPS provider and disable the standard Bouncy Castle provider;
        /// <c>false</c> to use the standard provider behavior.
        /// </param>
        public static void SetFipsMode(bool state)
        {

                // SecurityUtils.setFipsMode();
               
                // to enable/disable the FIPS
                SecurityUtils.setAPrioriDisabledProvider("BCFIPS", !state);

                // to enable/disable the standard Bouncy Castle provider
                SecurityUtils.setAPrioriDisabledProvider("BC", state);
           
        }

    }
}
