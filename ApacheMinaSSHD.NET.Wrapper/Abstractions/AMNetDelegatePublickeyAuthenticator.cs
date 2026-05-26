using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Public key authenticator that delegates fingerprint validation to an application callback.
    /// </summary>
    public sealed class AMNetDelegatePublickeyAuthenticator : IAMNetPublickeyAuthenticator
    {
        private readonly Func<string, string, ISshSession, bool> authenticate;

        /// <summary>
        /// Creates a public key authenticator backed by a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, public key fingerprint, and session metadata
        /// and returns whether the key should be accepted.
        /// </param>
        public AMNetDelegatePublickeyAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            this.authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
        }

        /// <inheritdoc />
        public bool Authenticate(string username, string incomingFingerprint, ISshSession session)
        {
            return authenticate(username, incomingFingerprint, session);
        }
    }
}
