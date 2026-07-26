using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// A GSSAPI authenticator that delegates identity and user validation to supplied callbacks.
    /// </summary>
    public sealed class AMNetDelegateGssapiAuthenticator : IAMNetGssapiAuthenticator
    {
        private readonly Func<ISshSession, string, bool> validateIdentity;
        private readonly Func<ISshSession, string, bool>? validateInitialUser;
        private readonly string? servicePrincipalName;
        private readonly string? keytabFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="AMNetDelegateGssapiAuthenticator"/> class.
        /// </summary>
        /// <param name="validateIdentity">Callback that validates a GSSAPI identity.</param>
        /// <param name="validateInitialUser">Optional callback that validates the initial user, or <c>null</c> to accept all.</param>
        /// <param name="servicePrincipalName">The Kerberos service principal name, or <c>null</c> for the default.</param>
        /// <param name="keytabFile">Path to the keytab file, or <c>null</c> for the default.</param>
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

        /// <inheritdoc/>
        public bool ValidateIdentity(ISshSession session, string identity)
        {
            return validateIdentity(session, identity);
        }

        /// <inheritdoc/>
        public bool ValidateInitialUser(ISshSession session, string username)
        {
            return validateInitialUser?.Invoke(session, username) ?? true;
        }

        /// <inheritdoc/>
        public string? ServicePrincipalName => servicePrincipalName;

        /// <inheritdoc/>
        public string? KeytabFile => keytabFile;
    }
}
