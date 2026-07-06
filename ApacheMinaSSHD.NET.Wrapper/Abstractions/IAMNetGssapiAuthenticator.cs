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
    /// Defines a contract for GSSAPI/Kerberos authentication, enabling single sign-on
    /// integration with Kerberos infrastructure.
    /// </summary>
    public interface IAMNetGssapiAuthenticator
    {
        /// <summary>
        /// Validates the GSSAPI identity presented by the client.
        /// </summary>
        /// <param name="session">Metadata for the current SSH session.</param>
        /// <param name="identity">The GSSAPI identity string from the client.</param>
        /// <returns><c>true</c> if the identity is valid; otherwise <c>false</c>.</returns>
        bool ValidateIdentity(ISshSession session, string identity);

        /// <summary>
        /// Called before <see cref="ValidateIdentity"/> to pre-validate the username
        /// before performing the full GSSAPI exchange.
        /// </summary>
        /// <param name="session">Metadata for the current SSH session.</param>
        /// <param name="username">The SSH username to pre-validate.</param>
        /// <returns><c>true</c> if the username is allowed to proceed; <c>false</c> to reject early.</returns>
        bool ValidateInitialUser(ISshSession session, string username) => true;

        /// <summary>
        /// Gets the Kerberos service principal name (e.g., "host/server.example.com@REALM").
        /// When non-null, the server uses this principal for service credential validation.
        /// </summary>
        string? ServicePrincipalName => null;

        /// <summary>
        /// Gets the path to the Kerberos keytab file used for service credential validation.
        /// When non-null, the server reads the service key from this file.
        /// </summary>
        string? KeytabFile => null;
    }
}
