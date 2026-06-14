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
    /// Authenticates SSH users by comparing the client's public key fingerprint
    /// with application-managed key records.
    /// </summary>
    public interface IAMNetPublickeyAuthenticator
    {
        /// <summary>
        /// Returns whether the supplied public key fingerprint should be accepted for the user.
        /// </summary>
        /// <param name="username">The username requested by the client.</param>
        /// <param name="incomingFingerprint">The fingerprint of the public key presented by the client.</param>
        /// <param name="session">Session metadata for the connection being authenticated.</param>
        /// <returns><c>true</c> to accept the public key; otherwise <c>false</c>.</returns>
        bool Authenticate(string username, string incomingFingerprint, ISshSession session);
    }
}
