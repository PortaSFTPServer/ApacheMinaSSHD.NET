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
    /// Secure default password authenticator implementation.
    /// </summary>
    /// <remarks>
    /// This implementation denies all passwords. Override
    /// <see cref="Authenticate(string, string, ISshSession)"/>, use
    /// <see cref="AMNetDelegatePasswordAuthenticator"/>, or provide your own
    /// <see cref="IAMNetPasswordAuthenticator"/> to enforce application password policy.
    /// </remarks>
    public class AMNetPasswordAuthenticator : IAMNetPasswordAuthenticator
    {
        /// <summary>
        /// Creates a default password authenticator.
        /// </summary>
        public AMNetPasswordAuthenticator()
        {
        }

        /// <inheritdoc />
        public virtual bool Authenticate(string username, string password, ISshSession session)
        {
            return false;
        }
    }
}
