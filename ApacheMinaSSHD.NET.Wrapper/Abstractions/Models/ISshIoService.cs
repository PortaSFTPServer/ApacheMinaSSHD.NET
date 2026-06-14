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

using System.Net;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{

    /// <summary>
    /// Provides metadata about the low-level I/O service handling a connection.
    /// </summary>
    public interface ISshIoService
    {
        /// <summary>Gets whether the service is accepting inbound connections.</summary>
        bool IsAcceptor { get; }
        /// <summary>Gets whether service shutdown has started.</summary>
        bool IsClosing { get; }
        /// <summary>Gets whether service shutdown is complete.</summary>
        bool IsClosed { get; }

        /// <summary>
        /// Gets all addresses this service is bound to.
        /// </summary>
        IEnumerable<IPEndPoint> BoundAddresses { get; }
    }




}
