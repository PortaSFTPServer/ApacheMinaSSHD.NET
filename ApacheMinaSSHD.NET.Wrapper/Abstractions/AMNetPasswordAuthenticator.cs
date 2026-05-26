
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default password authenticator implementation.
    /// </summary>
    /// <remarks>
    /// Override <see cref="Authenticate(string, string, ISshSession)"/> or provide
    /// your own <see cref="IAMNetPasswordAuthenticator"/> to enforce application
    /// password policy.
    /// </remarks>
    public class AMNetPasswordAuthenticator : IAMNetPasswordAuthenticator
    {
        /// <summary>
        /// Creates a default password authenticator.
        /// </summary>
        public AMNetPasswordAuthenticator()
        {
        }

        /// <inheritdoc />
        public virtual bool Authenticate(string username, string password, ISshSession session)
        {
            return true;
        }
    }
}
