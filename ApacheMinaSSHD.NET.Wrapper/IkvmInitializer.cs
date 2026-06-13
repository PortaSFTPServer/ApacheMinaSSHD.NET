// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
