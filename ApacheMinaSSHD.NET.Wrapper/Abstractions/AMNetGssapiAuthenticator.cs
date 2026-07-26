using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Base class for GSSAPI (Kerberos) authentication. Override the virtual members
    /// to implement custom GSSAPI validation.
    /// </summary>
    public class AMNetGssapiAuthenticator : IAMNetGssapiAuthenticator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AMNetGssapiAuthenticator"/> class.
        /// </summary>
        public AMNetGssapiAuthenticator()
        {
        }

        /// <inheritdoc/>
        public virtual bool ValidateIdentity(ISshSession session, string identity)
        {
            return false;
        }

        /// <inheritdoc/>
        public virtual bool ValidateInitialUser(ISshSession session, string username) => true;

        /// <inheritdoc/>
        public virtual string? ServicePrincipalName => null;

        /// <inheritdoc/>
        public virtual string? KeytabFile => null;
    }
}
