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

using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for received SFTP messages.
    /// </summary>
    public interface ISshReceived
    {
        /// <summary>Gets the session associated with the received message.</summary>
        ISshSession SshSession { get; }

        /// <summary>Gets the numeric SFTP message type.</summary>
        public int Type { get; }

        /// <summary>Gets the extension name when the message is an extension.</summary>
        string Extension { get; }
        /// <summary>Gets the request identifier.</summary>
        int Id { get; }
    }
}
