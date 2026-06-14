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
