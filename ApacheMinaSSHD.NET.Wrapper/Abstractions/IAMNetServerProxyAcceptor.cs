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
    /// Provides a way to implement proxied connections where metadata about the
    /// client is sent before the actual SSH protocol is executed.
    /// </summary>
    public interface IAMNetServerProxyAcceptor
    {
        /// <summary>
        /// Parses and validates metadata sent before the SSH handshake, such as PROXY protocol headers.
        /// </summary>
        /// <param name="proxyMetadata">The incoming metadata buffer and connection attributes.</param>
        /// <returns><c>true</c> when SSH handshake processing may continue; otherwise <c>false</c>.</returns>
        bool acceptServerProxyMetadata(IProxyMetadata proxyMetadata);
    }
}
