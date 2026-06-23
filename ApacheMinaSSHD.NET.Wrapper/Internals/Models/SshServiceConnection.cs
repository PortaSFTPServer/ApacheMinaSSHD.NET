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
using System.Net;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    /// <summary>
    /// This class encapsulates the parameters from the IoServiceEventListener.
    /// These are the <strong>IoConnector connector, SocketAddress local, AttributeRepository context, 
    /// SocketAddress remote, Exception reason</strong>.
    /// </summary>
    internal class SshServiceConnection : ISshServiceConnection
    {
        /// <summary>
        /// Local end point properties
        /// </summary>
        public IPEndPoint LocalEndPoint { get; set; } = null!;
        /// <summary>
        /// Remote end point properties
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; } = null!;
        /// <summary>
        /// Service end point properties
        /// </summary>
        public IPEndPoint ServiceEndPoint { get; set; } = null!;
        /// <summary>
        /// The I/O Manager (Acceptor/Connector)
        /// </summary>
        public ISshIoService IoService { get; set; } = null!;
        public IReadOnlyDictionary<string, object> Attributes { get; set; } =
            new Dictionary<string, object>();

        /// <summary>
        /// Error message / information.
        /// </summary>
        public System.Exception Exception { get; internal set; } = null!;


    }
}
