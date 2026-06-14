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

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshSession : Abstractions.Models.ISshSession
    {
        private readonly org.apache.sshd.server.session.ServerSession? _javaSession;

        public SshSession(org.apache.sshd.server.session.ServerSession javaSession)
        {
            _javaSession = javaSession;
        }

        public SshSession()
        {
            _javaSession = null;
        }

        public string RemoteAddress
        {
            get
            {
                try
                {
                    if (_javaSession?.getIoSession()?.getRemoteAddress() != null)
                        return _javaSession.getIoSession().getRemoteAddress().toString();
                }
                catch
                {
                }
                return "unknown";
            }
        }

        public Guid SessionId { get; } = Guid.NewGuid();
    }
}
