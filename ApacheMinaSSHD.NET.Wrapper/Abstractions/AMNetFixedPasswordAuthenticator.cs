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
    public sealed class AMNetFixedPasswordAuthenticator : IAMNetPasswordAuthenticator, IDisposable
    {
        private readonly string username;
        private byte[]? passwordBytes;

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

            if (passwordBytes == null)
            {
                return false;
            }

            byte[] incomingBytes = Encoding.UTF8.GetBytes(password ?? string.Empty);
            return incomingBytes.Length == passwordBytes.Length
                && CryptographicOperations.FixedTimeEquals(passwordBytes, incomingBytes);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (passwordBytes != null)
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                passwordBytes = null;
            }
        }
    }
}
