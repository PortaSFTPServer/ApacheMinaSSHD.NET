using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System.Security.Cryptography;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Password authenticator for a single fixed username and password pair.
    /// </summary>
    /// <remarks>
    /// This class is useful for samples, tests, embedded appliances, or simple
    /// deployments. Production applications should normally validate credentials
    /// against their own identity store and auditing policy.
    /// </remarks>
    public sealed class AMNetFixedPasswordAuthenticator : IAMNetPasswordAuthenticator
    {
        private readonly string username;
        private readonly byte[] passwordBytes;

        /// <summary>
        /// Creates a fixed password authenticator.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="password">The exact password to accept.</param>
        public AMNetFixedPasswordAuthenticator(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            }

            if (password is null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            this.username = username;
            passwordBytes = Encoding.UTF8.GetBytes(password);
        }

        /// <inheritdoc />
        public bool Authenticate(string username, string password, ISshSession session)
        {
            if (!string.Equals(this.username, username, StringComparison.Ordinal))
            {
                return false;
            }

            byte[] incomingBytes = Encoding.UTF8.GetBytes(password ?? string.Empty);
            return incomingBytes.Length == passwordBytes.Length
                && CryptographicOperations.FixedTimeEquals(passwordBytes, incomingBytes);
        }
    }
}
