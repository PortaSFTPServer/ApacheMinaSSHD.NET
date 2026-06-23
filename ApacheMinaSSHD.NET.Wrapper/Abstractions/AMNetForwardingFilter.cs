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
    /// <summary>Composite forwarding filter that delegates to separate TCP, agent, and X11 filters.</summary>
    public class AMNetForwardingFilter : IAMNetForwardingFilter
    {
        private readonly IAMNetTcpForwardingFilter? _tcp;
        private readonly IAMNetAgentForwardingFilter? _agent;
        private readonly IAMNetX11ForwardingFilter? _x11;

        /// <summary>Creates a composite filter with optional sub-filters for each forwarding category.</summary>
        /// <param name="tcp">TCP forwarding filter, or <c>null</c> to allow all TCP forwarding.</param>
        /// <param name="agent">Agent forwarding filter, or <c>null</c> to reject all agent forwarding.</param>
        /// <param name="x11">X11 forwarding filter, or <c>null</c> to reject all X11 forwarding.</param>
        public AMNetForwardingFilter(
            IAMNetTcpForwardingFilter? tcp = null,
            IAMNetAgentForwardingFilter? agent = null,
            IAMNetX11ForwardingFilter? x11 = null)
        {
            _tcp = tcp;
            _agent = agent;
            _x11 = x11;
        }

        /// <inheritdoc />
        public bool CanListen(string host, int port, ISshSession session)
            => _tcp?.CanListen(host, port, session) ?? true;

        /// <inheritdoc />
        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
            => _tcp?.CanConnect(type, host, port, session) ?? true;

        /// <inheritdoc />
        public bool CanForwardDynamic(string host, int port, ISshSession session)
            => _tcp?.CanForwardDynamic(host, port, session) ?? true;

        /// <inheritdoc />
        public bool CanForwardAgent(ISshSession session, string requestType)
            => _agent?.CanForwardAgent(session, requestType) ?? false;

        /// <inheritdoc />
        public bool CanForwardX11(ISshSession session, string requestType)
            => _x11?.CanForwardX11(session, requestType) ?? false;

        /// <summary>Gets a composite filter that accepts all forwarding requests.</summary>
        public static AMNetForwardingFilter AcceptAll => new(
            AMNetTcpForwardingFilter.AcceptAll, null, null
        );

        /// <summary>Gets a composite filter that rejects all forwarding requests.</summary>
        public static AMNetForwardingFilter RejectAll => new(
            AMNetTcpForwardingFilter.RejectAll, null, null
        );

        /// <summary>Creates a composite filter from a TCP forwarding policy, with agent and X11 forwarding rejected.</summary>
        /// <param name="policy">The TCP forwarding policy to apply.</param>
        /// <returns>A new composite filter that uses the given TCP policy and denies agent/X11.</returns>
        public static AMNetForwardingFilter FromPolicy(AMNetTcpForwardingPolicy policy)
            => new(new AMNetTcpForwardingFilter(policy), null, null);
    }
}
