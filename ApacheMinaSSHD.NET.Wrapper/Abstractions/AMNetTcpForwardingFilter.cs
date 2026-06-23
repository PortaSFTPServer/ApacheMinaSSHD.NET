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
    /// <summary>Filters TCP port forwarding requests based on a configured policy.</summary>
    public class AMNetTcpForwardingFilter : IAMNetTcpForwardingFilter
    {
        private readonly AMNetTcpForwardingPolicy _policy;

        /// <summary>Creates a filter with the specified TCP forwarding policy.</summary>
        /// <param name="policy">The policy that governs listen and connect decisions.</param>
        public AMNetTcpForwardingFilter(AMNetTcpForwardingPolicy policy)
        {
            _policy = policy;
        }

        /// <inheritdoc />
        public virtual bool CanListen(string host, int port, ISshSession session)
        {
            return _policy != AMNetTcpForwardingPolicy.None;
        }

        /// <inheritdoc />
        public virtual bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
        {
            return type switch
            {
                AMNetForwardingType.Direct => _policy is AMNetTcpForwardingPolicy.All or AMNetTcpForwardingPolicy.Local,
                AMNetForwardingType.Forwarded => _policy is AMNetTcpForwardingPolicy.All or AMNetTcpForwardingPolicy.Remote,
                AMNetForwardingType.Dynamic => _policy is AMNetTcpForwardingPolicy.All,
                _ => false
            };
        }

        /// <inheritdoc />
        public virtual bool CanForwardDynamic(string host, int port, ISshSession session)
        {
            return _policy is AMNetTcpForwardingPolicy.All;
        }

        /// <summary>Gets a filter that accepts all TCP forwarding requests.</summary>
        public static AMNetTcpForwardingFilter AcceptAll => new(AMNetTcpForwardingPolicy.All);
        /// <summary>Gets a filter that rejects all TCP forwarding requests.</summary>
        public static AMNetTcpForwardingFilter RejectAll => new(AMNetTcpForwardingPolicy.None);
    }
}
