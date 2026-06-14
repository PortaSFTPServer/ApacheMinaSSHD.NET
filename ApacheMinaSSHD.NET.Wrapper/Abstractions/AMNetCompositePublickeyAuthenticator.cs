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
    /// Public key authenticator that accepts a key when any inner authenticator accepts it.
    /// </summary>
    public sealed class AMNetCompositePublickeyAuthenticator : IAMNetPublickeyAuthenticator
    {
        private readonly IReadOnlyList<IAMNetPublickeyAuthenticator> authenticators;

        /// <summary>
        /// Creates a composite public key authenticator.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try in order.</param>
        public AMNetCompositePublickeyAuthenticator(params IAMNetPublickeyAuthenticator[] authenticators)
            : this((IEnumerable<IAMNetPublickeyAuthenticator>)authenticators)
        {
        }

        /// <summary>
        /// Creates a composite public key authenticator.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try in order.</param>
        public AMNetCompositePublickeyAuthenticator(IEnumerable<IAMNetPublickeyAuthenticator> authenticators)
        {
            ArgumentNullException.ThrowIfNull(authenticators);
            this.authenticators = authenticators
                .Select(authenticator => authenticator ?? throw new ArgumentException("Authenticator entries cannot be null.", nameof(authenticators)))
                .ToArray();
        }

        /// <summary>
        /// Gets the configured public key authenticators in evaluation order.
        /// </summary>
        public IReadOnlyList<IAMNetPublickeyAuthenticator> Authenticators => authenticators;

        /// <inheritdoc />
        public bool Authenticate(string username, string incomingFingerprint, ISshSession session)
        {
            foreach (IAMNetPublickeyAuthenticator authenticator in authenticators)
            {
                if (authenticator.Authenticate(username, incomingFingerprint, session))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
