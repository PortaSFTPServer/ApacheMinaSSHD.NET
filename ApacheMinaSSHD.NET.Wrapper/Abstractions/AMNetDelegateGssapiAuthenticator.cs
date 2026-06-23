using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public sealed class AMNetDelegateGssapiAuthenticator : IAMNetGssapiAuthenticator
    {
        private readonly Func<ISshSession, string, bool> validateIdentity;

        public AMNetDelegateGssapiAuthenticator(Func<ISshSession, string, bool> validateIdentity)
        {
            this.validateIdentity = validateIdentity ?? throw new ArgumentNullException(nameof(validateIdentity));
        }

        public bool ValidateIdentity(ISshSession session, string identity)
        {
            return validateIdentity(session, identity);
        }
    }
}
