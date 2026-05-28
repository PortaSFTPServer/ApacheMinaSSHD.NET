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
                catch
                {
                }
            }
        }
    }
}
