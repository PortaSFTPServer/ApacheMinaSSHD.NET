using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetHostBasedAuthenticator : IAMNetHostBasedAuthenticator
    {
        public virtual bool Authenticate(string username, string publicKeyFingerprint, string clientHostname, string clientUsername, ISshSession session)
        {
            return false;
        }
    }
}
