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
    /// <summary>
    /// Default low-level connection listener that logs connection events.
    /// </summary>
    public class AMNetIoServiceEventListener : IAMNetIoServiceEventListener
    {
        IAMNetLogger logger = new AMNetLogger(typeof(AMNetIoServiceEventListener), AMNetLogger.LogLevel.Info);

        /// <summary>
        /// Creates a default low-level connection listener.
        /// </summary>
        public AMNetIoServiceEventListener()
        {
        }

        /// <inheritdoc />
        public virtual void OnConnectionAborted(ISshServiceConnection context)
        {
            logger.Error(context.Exception?.Message!);
        }

        /// <inheritdoc />
        public virtual bool OnConnectionAccepted(ISshServiceConnection context)
        {
            // The developer sees clean properties via the interface
            logger.Info($"Evaluating connection from {context.RemoteEndPoint.Address}...");

            if (context.IoService.IsAcceptor)
            {
                // Access properties of the nested interface
                var currentLoad = context.IoService.BoundAddresses.Count();
            }

            return true;
        }

        /// <inheritdoc />
        public virtual void OnOutboundConnectionAborted(ISshServiceConnection context)
        {
            logger.Error(context.Exception?.Message!);

        }

        /// <inheritdoc />
        public virtual void OnOutboundConnectionEstablished(ISshServiceConnection context)
        {
            logger.Info("Outbound connection established.");
        }
    }
}
