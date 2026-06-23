using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public sealed class AMNetDelegateHostBasedAuthenticator : IAMNetHostBasedAuthenticator
    {
        private readonly Func<string, string, string, string, ISshSession, bool> authenticate;

        public AMNetDelegateHostBasedAuthenticator(Func<string, string, string, string, ISshSession, bool> authenticate)
        {
            this.authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
        }

        public bool Authenticate(string username, string publicKeyFingerprint, string clientHostname, string clientUsername, ISshSession session)
        {
            return authenticate(username, publicKeyFingerprint, clientHostname, clientUsername, session);
        }
    }
}
