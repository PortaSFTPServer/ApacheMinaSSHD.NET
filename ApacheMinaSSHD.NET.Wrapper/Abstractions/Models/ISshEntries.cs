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
    /// Provides metadata for SFTP directory entry read events.
    /// </summary>
    public interface ISshEntries
    {

        /// <summary>Gets the session associated with the directory read.</summary>
        public ISshSession SshSession { get; }
        /// <summary>Gets the remote directory handle identifier.</summary>
        public string RemoteHandle { get; }
        /// <summary>Gets the local directory handle wrapper.</summary>
        public ISshDirectoryHandle localHandle { get; }
        /// <summary>Gets the directory entries returned to the client.</summary>
        public IReadOnlyDictionary<string, object> Entries { get; }
    }
}
