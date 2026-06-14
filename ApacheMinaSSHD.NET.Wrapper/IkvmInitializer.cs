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

using System.Reflection;
using System.Runtime.CompilerServices;

namespace ApacheMinaSSHD.NET.Wrapper
{
    internal static class IkvmInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            string[] ikvmAssemblies =
            [
                "org.slf4j",
                "org.slf4j.simple",
                "org.apache.sshd.common",
                "org.apache.sshd.core",
                "org.apache.sshd.sftp",
                "org.apache.sshd.scp",
                "org.apache.commons.logging"
            ];

            foreach (string name in ikvmAssemblies)
            {
                try
                {
                    Assembly asm = Assembly.Load(name);
                    ikvm.runtime.Startup.addBootClassPathAssembly(asm);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[{nameof(IkvmInitializer)}] Failed to load IKVM assembly '{name}': {ex.Message}");
                }
            }
        }
    }
}
