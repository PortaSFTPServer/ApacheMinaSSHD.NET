using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Authenticates SSH users by comparing the client's public key fingerprint
    /// with application-managed key records.
    /// </summary>
    public interface IAMNetPublickeyAuthenticator
    {
        /// <summary>
        /// Returns whether the supplied public key fingerprint should be accepted for the user.
        /// </summary>
        /// <param name="username">The username requested by the client.</param>
        /// <param name="incomingFingerprint">The fingerprint of the public key presented by the client.</param>
        /// <param name="session">Session metadata for the connection being authenticated.</param>
        /// <returns><c>true</c> to accept the public key; otherwise <c>false</c>.</returns>
        bool Authenticate(string username, string incomingFingerprint, ISshSession session);
    }
}
