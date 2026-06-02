using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Public key authenticator backed by application-managed SSH key fingerprints.
    /// </summary>
    /// <remarks>
    /// Fingerprints should use the same format returned by Apache MINA SSHD, for
    /// example <c>SHA256:...</c>. Use this when the application stores fingerprints
    /// in a database or configuration store instead of an authorized_keys file.
    /// </remarks>
    public sealed class AMNetFingerprintPublickeyAuthenticator : IAMNetPublickeyAuthenticator
    {
        private readonly Dictionary<string, HashSet<string>> userFingerprints =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates an empty fingerprint authenticator.
        /// </summary>
        public AMNetFingerprintPublickeyAuthenticator()
        {
        }

        /// <summary>
        /// Creates a fingerprint authenticator for one username and fingerprint.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="fingerprint">The public key fingerprint to accept for the user.</param>
        public AMNetFingerprintPublickeyAuthenticator(string username, string fingerprint)
        {
            AddFingerprint(username, fingerprint);
        }

        /// <summary>
        /// Adds an accepted fingerprint for a username.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="fingerprint">The public key fingerprint to accept for the user.</param>
        /// <returns>The current authenticator so calls can be chained.</returns>
        public AMNetFingerprintPublickeyAuthenticator AddFingerprint(string username, string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(fingerprint))
            {
                throw new ArgumentException("Fingerprint cannot be empty.", nameof(fingerprint));
            }

            if (!userFingerprints.TryGetValue(username, out HashSet<string>? fingerprints))
            {
                fingerprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                userFingerprints.Add(username, fingerprints);
            }

            fingerprints.Add(fingerprint.Trim());
            return this;
        }

        /// <inheritdoc />
        public bool Authenticate(string username, string incomingFingerprint, ISshSession session)
        {
            return userFingerprints.TryGetValue(username, out HashSet<string>? fingerprints)
                && !string.IsNullOrWhiteSpace(incomingFingerprint)
                && fingerprints.Contains(incomingFingerprint.Trim());
        }
    }
}
