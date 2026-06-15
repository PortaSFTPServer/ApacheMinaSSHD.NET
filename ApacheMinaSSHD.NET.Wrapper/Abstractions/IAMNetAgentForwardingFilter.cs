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
    /// <summary>Provides a filter to control whether SSH agent forwarding is permitted for a session.</summary>
    public interface IAMNetAgentForwardingFilter
    {
        /// <summary>Determines whether agent forwarding is allowed for the given session and request type.</summary>
        /// <param name="session">The SSH session requesting agent forwarding.</param>
        /// <param name="requestType">The type of agent forwarding request.</param>
        /// <returns><c>true</c> if agent forwarding is permitted; otherwise <c>false</c>.</returns>
        bool CanForwardAgent(ISshSession session, string requestType);
    }
}
