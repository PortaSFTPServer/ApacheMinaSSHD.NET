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
    /// Default SCP transfer listener that logs transfer lifecycle events.
    /// </summary>
    public class AMNetScpTransferEventListener : IAMNetScpTransferEventListener
    {
        private readonly IAMNetLogger logger;

        /// <summary>
        /// Creates an SCP transfer listener using the default logger.
        /// </summary>
        public AMNetScpTransferEventListener()
            : this(new AMNetLogger(typeof(AMNetScpTransferEventListener), AMNetLogger.LogLevel.Info))
        {
        }

        /// <summary>
        /// Creates an SCP transfer listener using the supplied logger.
        /// </summary>
        /// <param name="logger">The logger used by the default event handlers.</param>
        public AMNetScpTransferEventListener(IAMNetLogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public virtual void OnStartFile(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} started for file {context.Path}.");
        }

        /// <inheritdoc />
        public virtual void OnEndFile(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} ended for file {context.Path}.");
        }

        /// <inheritdoc />
        public virtual void OnFileAck(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} acknowledgement for file {context.Path}: {context.AckStatusCode}");
        }

        /// <inheritdoc />
        public virtual void OnReceiveCommandAck(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP receive command acknowledgement for {context.Command}: {context.AckStatusCode}");
        }

        /// <inheritdoc />
        public virtual void OnStartFolder(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} started for folder {context.Path}.");
        }

        /// <inheritdoc />
        public virtual void OnEndFolder(ISshScpTransferEvent context)
        {
            logger.Debug($"SCP {context.Operation} ended for folder {context.Path}.");
        }
    }
}
