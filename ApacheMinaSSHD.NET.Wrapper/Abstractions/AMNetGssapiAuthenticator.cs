using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetGssapiAuthenticator : IAMNetGssapiAuthenticator
    {
        public virtual bool ValidateIdentity(ISshSession session, string identity)
        {
            return false;
        }

        public virtual bool ValidateInitialUser(ISshSession session, string username) => true;

        public virtual string? ServicePrincipalName => null;

        public virtual string? KeytabFile => null;
    }
}
