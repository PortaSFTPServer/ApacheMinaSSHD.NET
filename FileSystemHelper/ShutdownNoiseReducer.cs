// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Text;

namespace FileSystemHelper
{
    public static class ShutdownNoiseReducer
    {

        public static bool IgnoreNoise(string logNoise)
        {
            // ignore any line that looks like the NIO2 shutdown crash
            return (logNoise.Contains("java.lang.IllegalStateException") ||
                logNoise.Contains("org.apache.sshd.common.util") ||
                logNoise.Contains("sun.nio.ch") ||
                logNoise.Contains("IoServiceFactoryFactory") ||
                logNoise.Contains("Nio2ServiceFactoryFactory") ||
                logNoise.Contains("at java.lang.Thread") ||
                logNoise.Contains("Exception in thread"));
        }
    }
}
