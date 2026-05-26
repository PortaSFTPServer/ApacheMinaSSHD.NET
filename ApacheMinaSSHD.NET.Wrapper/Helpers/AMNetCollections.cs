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
