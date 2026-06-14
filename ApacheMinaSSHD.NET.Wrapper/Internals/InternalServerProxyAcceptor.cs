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
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalServerProxyAcceptor : java.lang.Object, ServerProxyAcceptor
    {
        private readonly IAMNetServerProxyAcceptor acceptor;

        public InternalServerProxyAcceptor(IAMNetServerProxyAcceptor acceptor)
        {
            this.acceptor = acceptor;
        }

        public bool acceptServerProxyMetadata(ServerSession ss, org.apache.sshd.common.util.buffer.Buffer b)
        {

            // this will call the implementation from IAMNetServerProxyAcceptor/ acceptor provider
            return acceptor.acceptServerProxyMetadata(new ProxyMetadata(ss, b));
        }
    }
}
