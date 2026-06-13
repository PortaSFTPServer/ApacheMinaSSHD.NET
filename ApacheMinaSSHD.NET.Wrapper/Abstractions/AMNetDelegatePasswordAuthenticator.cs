// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
