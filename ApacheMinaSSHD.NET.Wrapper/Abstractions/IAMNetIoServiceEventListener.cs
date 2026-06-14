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
    /// <summary>
    /// Receives low-level connection events before or around SSH session creation.
    /// </summary>
    public interface IAMNetIoServiceEventListener
    {
        /// <summary>
        /// Called when a new client connects. Return <c>false</c> to block or disconnect the connection immediately.
        /// </summary>
        /// <param name="context">Connection metadata for the accepted connection.</param>
        /// <returns><c>true</c> to allow the connection; otherwise <c>false</c>.</returns>
        bool OnConnectionAccepted(ISshServiceConnection context);

        /// <summary>
        /// Called if an accepted connection is closed before it's fully established.
        /// </summary>
        /// <param name="context">Connection metadata and the failure exception.</param>
        void OnConnectionAborted(ISshServiceConnection context);

        /// <summary>
        /// Only relevant if your server makes outbound connections (Forwarding).
        /// </summary>
        /// <param name="context">Connection metadata for the outbound connection.</param>
        void OnOutboundConnectionEstablished(ISshServiceConnection context);

        /// <summary>
        /// Only relevant if an outbound connection attempt fails.
        /// </summary>
        /// <param name="context">Connection metadata and the failure exception.</param>
        void OnOutboundConnectionAborted(ISshServiceConnection context);
    }
}
