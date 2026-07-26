using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// A host-based authenticator that delegates authentication to a supplied callback.
    /// </summary>
    public sealed class AMNetDelegateHostBasedAuthenticator : IAMNetHostBasedAuthenticator
    {
        private readonly Func<string, string, string, string, ISshSession, bool> authenticate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AMNetDelegateHostBasedAuthenticator"/> class.
        /// </summary>
        /// <param name="authenticate">Callback that validates host-based authentication.</param>
        public AMNetDelegateHostBasedAuthenticator(Func<string, string, string, string, ISshSession, bool> authenticate)
        {
            this.authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
        }

        /// <inheritdoc/>
        public bool Authenticate(string username, string publicKeyFingerprint, string clientHostname, string clientUsername, ISshSession session)
        {
            return authenticate(username, publicKeyFingerprint, clientHostname, clientUsername, session);
        }
    }
}
