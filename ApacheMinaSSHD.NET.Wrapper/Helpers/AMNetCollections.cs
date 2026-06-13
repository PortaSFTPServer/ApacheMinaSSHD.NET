// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Factories;
using java.util;

namespace ApacheMinaSSHD.NET.Wrapper.Helpers
{
    internal static class AMNetCollections
    {

        public static List getftpFactorySingleton(AMNetSftpSubsystemFactory sftpFactory)
        {
            return Collections.singletonList(sftpFactory.JavaFactory);
        }
    }
}
