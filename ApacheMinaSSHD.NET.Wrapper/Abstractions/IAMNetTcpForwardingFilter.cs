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
    /// <summary>Provides a filter to control TCP port forwarding for a session.</summary>
    public interface IAMNetTcpForwardingFilter
    {
        /// <summary>Determines whether the server may listen on the given host and port for forwarding.</summary>
        /// <param name="host">The host address to listen on.</param>
        /// <param name="port">The port number to listen on.</param>
        /// <param name="session">The SSH session requesting the listen.</param>
        /// <returns><c>true</c> if listening is permitted; otherwise <c>false</c>.</returns>
        bool CanListen(string host, int port, ISshSession session);
        /// <summary>Determines whether a forwarding connection to the given host and port is permitted.</summary>
        /// <param name="type">The direction of the forwarding (direct or forwarded).</param>
        /// <param name="host">The target host address.</param>
        /// <param name="port">The target port number.</param>
        /// <param name="session">The SSH session requesting the connection.</param>
        /// <returns><c>true</c> if the connection is permitted; otherwise <c>false</c>.</returns>
        bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session);
    }
}
