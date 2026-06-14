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
    /// Password authenticator that accepts a login when any inner authenticator accepts it.
    /// </summary>
    public sealed class AMNetCompositePasswordAuthenticator : IAMNetPasswordAuthenticator
    {
        private readonly IReadOnlyList<IAMNetPasswordAuthenticator> authenticators;

        /// <summary>
        /// Creates a composite password authenticator.
        /// </summary>
        /// <param name="authenticators">The password authenticators to try in order.</param>
        public AMNetCompositePasswordAuthenticator(params IAMNetPasswordAuthenticator[] authenticators)
            : this((IEnumerable<IAMNetPasswordAuthenticator>)authenticators)
        {
        }

        /// <summary>
        /// Creates a composite password authenticator.
        /// </summary>
        /// <param name="authenticators">The password authenticators to try in order.</param>
        public AMNetCompositePasswordAuthenticator(IEnumerable<IAMNetPasswordAuthenticator> authenticators)
        {
            ArgumentNullException.ThrowIfNull(authenticators);
            this.authenticators = authenticators
                .Select(authenticator => authenticator ?? throw new ArgumentException("Authenticator entries cannot be null.", nameof(authenticators)))
                .ToArray();
        }

        /// <summary>
        /// Gets the configured password authenticators in evaluation order.
        /// </summary>
        public IReadOnlyList<IAMNetPasswordAuthenticator> Authenticators => authenticators;

        /// <inheritdoc />
        public bool Authenticate(string username, string password, ISshSession session)
        {
            foreach (IAMNetPasswordAuthenticator authenticator in authenticators)
            {
                if (authenticator.Authenticate(username, password, session))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
