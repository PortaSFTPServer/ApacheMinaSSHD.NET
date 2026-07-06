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
    /// Defines a contract for host-based authentication, where the client authenticates
    /// using a host key and the server verifies the client's hostname and username.
    /// </summary>
    public interface IAMNetHostBasedAuthenticator
    {
        /// <summary>
        /// Validates the host-based authentication attempt.
        /// </summary>
        /// <param name="username">The SSH username presented by the client.</param>
        /// <param name="publicKeyFingerprint">The fingerprint of the client's host key.</param>
        /// <param name="clientHostname">The client hostname as reported during the authentication exchange.</param>
        /// <param name="clientUsername">The username on the client host as reported during the exchange.</param>
        /// <param name="session">Metdata for the current SSH session, including the remote address and session identifier.</param>
        /// <returns><c>true</c> if the host-based credentials are valid; otherwise <c>false</c>.</returns>
        bool Authenticate(string username, string publicKeyFingerprint, string clientHostname, string clientUsername, ISshSession session);
    }
}
