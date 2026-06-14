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
    /// Secure default keyboard-interactive authenticator implementation.
    /// </summary>
    /// <remarks>
    /// This implementation sends no prompts and denies all responses. Override this
    /// class, use <see cref="AMNetDelegateKeyboardInteractiveAuthenticator"/>, or
    /// implement <see cref="IAMNetKeyboardInteractiveAuthenticator"/> to enforce
    /// application-specific challenge and response validation.
    /// </remarks>
    public class AMNetKeyboardInteractiveAuthenticator : IAMNetKeyboardInteractiveAuthenticator
    {
        /// <summary>
        /// Creates a default keyboard-interactive authenticator.
        /// </summary>
        public AMNetKeyboardInteractiveAuthenticator()
        {
        }

        /// <inheritdoc />
        public virtual bool Authenticate(ISshSession session, string username, IResponseList response)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void GenerateChallenge(string username, ISshChallenge challenge)
        {
        }
    }
}
