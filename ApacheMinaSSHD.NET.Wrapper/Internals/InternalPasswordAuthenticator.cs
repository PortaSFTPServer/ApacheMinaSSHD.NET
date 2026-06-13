// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalPasswordAuthenticator : java.lang.Object, org.apache.sshd.server.auth.password.PasswordAuthenticator
    {
        private readonly IAMNetPasswordAuthenticator authenticator;

        public InternalPasswordAuthenticator(IAMNetPasswordAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public bool authenticate(string username, string password, ServerSession session)
        {
            var wrappedSession = new SshSession(session);
            return authenticator.Authenticate(username, password, wrappedSession);
        }

        public bool handleClientPasswordChangeRequest(ServerSession session, string username, string oldPassword, string newPassword)
        {
            var wrappedSession = new SshSession(session);
            return authenticator.HandlePasswordChangeRequest(username, oldPassword, newPassword, wrappedSession);
        }
    }
}
