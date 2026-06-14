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
