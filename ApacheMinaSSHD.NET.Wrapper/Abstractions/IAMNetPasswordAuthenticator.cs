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
    /// Authenticates SSH users with a username and password supplied by the client.
    /// </summary>
    public interface IAMNetPasswordAuthenticator
    {
        /// <summary>
        /// Returns whether the supplied username and password should be accepted.
        /// </summary>
        /// <param name="username">The username requested by the client.</param>
        /// <param name="password">The password supplied by the client.</param>
        /// <param name="session">Session metadata for the connection being authenticated.</param>
        /// <returns><c>true</c> to accept the credentials; otherwise <c>false</c>.</returns>
        bool Authenticate(string username, string password, ISshSession session);

        /// <summary>
        /// Handles a client-initiated password change request.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="oldPassword">The current password.</param>
        /// <param name="newPassword">The requested new password.</param>
        /// <param name="session">Session metadata.</param>
        /// <returns><c>true</c> if the password change is accepted; otherwise <c>false</c>.</returns>
        bool HandlePasswordChangeRequest(string username, string oldPassword, string newPassword, ISshSession session) => false;
    }
}
