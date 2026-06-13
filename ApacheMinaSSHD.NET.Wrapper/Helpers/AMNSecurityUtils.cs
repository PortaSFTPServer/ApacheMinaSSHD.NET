// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using org.apache.sshd.common.util.security;
using org.apache.sshd.common.util.security.bouncycastle;

namespace ApacheMinaSSHD.NET.Helpers
{
    /// <summary>
    /// Provides security-provider helper methods for the SSH runtime.
    /// </summary>
    public static class AMNSecurityUtils
    {
        private static bool fipsConfigured;

        /// <summary>
        /// Enables or disables FIPS-oriented provider selection.
        /// </summary>
        /// <param name="state">
        /// <c>true</c> to prefer the BCFIPS provider and disable the standard Bouncy Castle provider;
        /// <c>false</c> to use the standard provider behavior.
        /// </param>
        /// <remarks>
        /// The underlying Java <c>SecurityUtils.setFipsMode()</c> can only be called once
        /// per JVM lifetime. Subsequent calls are no-ops since the JVM state is already established.
        /// </remarks>
        public static void SetFipsMode(bool state)
        {
            if (fipsConfigured)
            {
                return;
            }

            if (state)
            {
                SecurityUtils.setFipsMode();
            }

            SecurityUtils.setAPrioriDisabledProvider("BCFIPS", !state);

            SecurityUtils.setAPrioriDisabledProvider("BC", state);

            fipsConfigured = true;
        }

    }
}
