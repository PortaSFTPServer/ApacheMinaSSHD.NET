using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetGssapiAuthenticator : IAMNetGssapiAuthenticator
    {
        public virtual bool ValidateIdentity(ISshSession session, string identity)
        {
            return false;
        }
    }
}
