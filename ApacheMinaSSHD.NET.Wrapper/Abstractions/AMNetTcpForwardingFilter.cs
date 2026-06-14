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
    public class AMNetTcpForwardingFilter : IAMNetTcpForwardingFilter
    {
        private readonly AMNetTcpForwardingPolicy _policy;

        public AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy policy)
        {
            _policy = policy;
        }

        public virtual bool CanListen(string host, int port, ISshSession session)
        {
            return _policy != AMNetTcpForwardingPolicy.None;
        }

        public virtual bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
        {
            return type switch
            {
                AMNetForwardingType.Direct => _policy is AMNetTcpForwardingPolicy.All or AMNetTcpForwardingPolicy.Local,
                AMNetForwardingType.Forwarded => _policy is AMNetTcpForwardingPolicy.All or AMNetTcpForwardingPolicy.Remote,
                _ => false
            };
        }

        public static AMNetTcpForwardingFilter AcceptAll => new(AMNetTcpForwardingPolicy.All);
        public static AMNetTcpForwardingFilter RejectAll => new(AMNetTcpForwardingPolicy.None);
    }
}
