using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public sealed class AMNetDelegateGssapiAuthenticator : IAMNetGssapiAuthenticator
    {
        private readonly Func<ISshSession, string, bool> validateIdentity;
        private readonly Func<ISshSession, string, bool>? validateInitialUser;
        private readonly string? servicePrincipalName;
        private readonly string? keytabFile;

        public AMNetDelegateGssapiAuthenticator(
            Func<ISshSession, string, bool> validateIdentity,
            Func<ISshSession, string, bool>? validateInitialUser = null,
            string? servicePrincipalName = null,
            string? keytabFile = null)
        {
            this.validateIdentity = validateIdentity ?? throw new ArgumentNullException(nameof(validateIdentity));
            this.validateInitialUser = validateInitialUser;
            this.servicePrincipalName = servicePrincipalName;
            this.keytabFile = keytabFile;
        }

        public bool ValidateIdentity(ISshSession session, string identity)
        {
            return validateIdentity(session, identity);
        }

        public bool ValidateInitialUser(ISshSession session, string username)
        {
            return validateInitialUser?.Invoke(session, username) ?? true;
        }

        public string? ServicePrincipalName => servicePrincipalName;

        public string? KeytabFile => keytabFile;
    }
}
