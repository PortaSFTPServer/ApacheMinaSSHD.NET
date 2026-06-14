// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using org.apache.sshd.common.util.security;
using org.apache.sshd.common.util.security.bouncycastle;

namespace ApacheMinaSSHD.NET.Wrapper.Helpers
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
