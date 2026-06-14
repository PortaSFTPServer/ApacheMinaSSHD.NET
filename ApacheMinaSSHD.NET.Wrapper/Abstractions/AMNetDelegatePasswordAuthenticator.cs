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
    /// Password authenticator that delegates validation to an application callback.
    /// </summary>
    public sealed class AMNetDelegatePasswordAuthenticator : IAMNetPasswordAuthenticator
    {
        private readonly Func<string, string, ISshSession, bool> authenticate;

        /// <summary>
        /// Creates a password authenticator backed by a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, password, and session metadata and returns
        /// whether the credentials should be accepted.
        /// </param>
        public AMNetDelegatePasswordAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            this.authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
        }

        /// <inheritdoc />
        public bool Authenticate(string username, string password, ISshSession session)
        {
            return authenticate(username, password, session);
        }
    }
}
