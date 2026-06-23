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
    /// <summary>TCP forwarding filter backed by user-supplied delegate functions.</summary>
    public sealed class AMNetDelegateTcpForwardingFilter : IAMNetTcpForwardingFilter
    {
        private readonly Func<string, int, ISshSession, bool> _canListen;
        private readonly Func<AMNetForwardingType, string, int, ISshSession, bool> _canConnect;
        private readonly Func<string, int, ISshSession, bool>? _canForwardDynamic;

        /// <summary>Creates a filter that delegates forwarding decisions to the supplied functions.</summary>
        /// <param name="canListen">Function that determines whether listening on a given host and port is allowed.</param>
        /// <param name="canConnect">Function that determines whether a forwarding connection to a given host and port is allowed.</param>
        /// <param name="canForwardDynamic">Optional function that determines whether dynamic forwarding is allowed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="canListen"/> or <paramref name="canConnect"/> is <c>null</c>.</exception>
        public AMNetDelegateTcpForwardingFilter(
            Func<string, int, ISshSession, bool> canListen,
            Func<AMNetForwardingType, string, int, ISshSession, bool> canConnect,
            Func<string, int, ISshSession, bool>? canForwardDynamic = null)
        {
            _canListen = canListen ?? throw new ArgumentNullException(nameof(canListen));
            _canConnect = canConnect ?? throw new ArgumentNullException(nameof(canConnect));
            _canForwardDynamic = canForwardDynamic;
        }

        /// <inheritdoc />
        public bool CanListen(string host, int port, ISshSession session)
            => _canListen(host, port, session);

        /// <inheritdoc />
        public bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session)
            => _canConnect(type, host, port, session);

        /// <inheritdoc />
        public bool CanForwardDynamic(string host, int port, ISshSession session)
            => _canForwardDynamic?.Invoke(host, port, session) ?? false;
    }
}
