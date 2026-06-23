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
using System.Net;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshIoService : ISshIoService
    {

        // We store the Java object internally but don't expose it
        private readonly org.apache.sshd.common.io.IoService _inner;

        public SshIoService(org.apache.sshd.common.io.IoService inner) => _inner = inner;

        public bool IsAcceptor => _inner is org.apache.sshd.common.io.IoAcceptor;
        // Corrected methods for SSHD 2.18.0
        public bool IsClosing => _inner.isClosing();
        public bool IsClosed => _inner.isClosed();

        public IEnumerable<IPEndPoint> BoundAddresses
        {
            get
            {
                if (_inner is org.apache.sshd.common.io.IoAcceptor acceptor)
                {
                    var bound = acceptor.getBoundAddresses().toArray();
                    foreach (var addr in bound)
                    {
                        if (addr is java.net.InetSocketAddress isa)
                            yield return new IPEndPoint(IPAddress.Parse(isa.getAddress().getHostAddress()), isa.getPort());
                    }
                }
            }
        }
    }
}

