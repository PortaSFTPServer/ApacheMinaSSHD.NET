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
using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>Default implementation of <see cref="IAMNetPortForwardingEventListener"/> that logs tunnel lifecycle events.</summary>
    public class AMNetPortForwardingEventListener : IAMNetPortForwardingEventListener
    {
        private readonly IAMNetLogger _logger;

        /// <summary>Creates a listener that logs tunnel events at the Info level.</summary>
        /// <param name="logger">The logger to write events to, or <c>null</c> to use a default console logger.</param>
        public AMNetPortForwardingEventListener(IAMNetLogger? logger = null)
        {
            _logger = logger ?? new AMNetLogger(typeof(AMNetPortForwardingEventListener), AMNetLogger.LogLevel.Info);
        }

        /// <inheritdoc />
        public virtual void OnEstablishingTunnel(string host, int port, bool isLocalForwarding, ISshSession session)
        {
            _logger.Info($"Establishing {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port}");
        }

        /// <inheritdoc />
        public virtual void OnEstablishedTunnel(string host, int port, bool isLocalForwarding, string boundAddress, ISshSession session)
        {
            _logger.Info($"Established {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port} bound at {boundAddress}");
        }

        /// <inheritdoc />
        public virtual void OnTearingDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session)
        {
            _logger.Info($"Tearing down {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port}");
        }

        /// <inheritdoc />
        public virtual void OnTornDownTunnel(string host, int port, bool isLocalForwarding, ISshSession session)
        {
            _logger.Info($"Torn down {(isLocalForwarding ? "local" : "remote")} tunnel to {host}:{port}");
        }
    }
}
