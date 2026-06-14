// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> or <paramref name="fingerprint"/> is null or whitespace.</exception>
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
