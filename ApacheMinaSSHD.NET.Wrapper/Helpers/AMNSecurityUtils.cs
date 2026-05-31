using org.apache.sshd.common.util.security;
using org.apache.sshd.common.util.security.bouncycastle;

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
                if (state)
                {
                    SecurityUtils.setFipsMode();
                }

                SecurityUtils.setAPrioriDisabledProvider("BCFIPS", !state);

                SecurityUtils.setAPrioriDisabledProvider("BC", state);
        }

    }
}
