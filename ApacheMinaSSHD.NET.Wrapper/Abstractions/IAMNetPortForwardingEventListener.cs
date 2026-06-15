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
    /// <summary>Provides notifications for port forwarding tunnel lifecycle events.</summary>
    public interface IAMNetPortForwardingEventListener
    {
        /// <summary>Called when a tunnel is being established.</summary>
        /// <param name="host">The target host for the tunnel.</param>
        /// <param name="port">The target port for the tunnel.</param>
        /// <param name="isLocalForwarding"><c>true</c> for local (outbound) forwarding; <c>false</c> for remote (inbound).</param>
        /// <param name="session">The SSH session that owns the tunnel.</param>
        void OnEstablishingTunnel(string host, int port, bool isLocalForwarding, ISshSession session) { }
        /// <summary>Called after a tunnel has been successfully established.</summary>
        /// <param name="host">The target host for the tunnel.</param>
        /// <param name="port">The target port for the tunnel.</param>
        /// <param name="isLocalForwarding"><c>true</c> for local (outbound) forwarding; <c>false</c> for remote (inbound).</param>
        /// <param name="boundAddress">The local address the tunnel is bound to.</param>
        /// <param name="session">The SSH session that owns the tunnel.</param>
        void OnEstablishedTunnel(string host, int port, bool isLocalForwarding, string boundAddress, ISshSession session) { }
        /// <summary>Called when a tunnel is being torn down.</summary>
        /// <param name="host">The target host for the tunnel.</param>
        /// <param name="port">The target port for the tunnel.</param>
        /// <param name="isLocalForwarding"><c>true</c> for local (outbound) forwarding; <c>false</c> for remote (inbound).</param>
        /// <param name="session">The SSH session that owns the tunnel.</param>
        void OnTearingDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session) { }
        /// <summary>Called after a tunnel has been fully torn down.</summary>
        /// <param name="host">The target host for the tunnel.</param>
        /// <param name="port">The target port for the tunnel.</param>
        /// <param name="isLocalForwarding"><c>true</c> for local (outbound) forwarding; <c>false</c> for remote (inbound).</param>
        /// <param name="session">The SSH session that owned the tunnel.</param>
        void OnTornDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session) { }
    }
}
