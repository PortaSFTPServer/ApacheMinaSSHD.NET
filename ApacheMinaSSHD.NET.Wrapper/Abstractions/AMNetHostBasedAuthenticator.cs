using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Base class for host-based public key authentication. Override the virtual
    /// method to implement custom host-based authentication.
    /// </summary>
    public class AMNetHostBasedAuthenticator : IAMNetHostBasedAuthenticator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AMNetHostBasedAuthenticator"/> class.
        /// </summary>
        public AMNetHostBasedAuthenticator()
        {
        }

        /// <inheritdoc/>
        public virtual bool Authenticate(string username, string publicKeyFingerprint, string clientHostname, string clientUsername, ISshSession session)
        {
            return false;
        }
    }
}
