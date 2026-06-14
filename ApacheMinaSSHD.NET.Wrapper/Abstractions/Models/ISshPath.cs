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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SFTP path-based events.
    /// </summary>
    public interface ISshPath
    {
        /// <summary>Gets the session associated with the path event.</summary>
        ISshSession Session { get; }
        /// <summary>Gets the local or reported path associated with the event.</summary>
        string Path { get; }
        /// <summary>Gets whether the path is a directory.</summary>
        public bool IsDirectory { get; }
        /// <summary>Gets file attributes associated with the path.</summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
        /// <summary>Gets the exception associated with the event when available.</summary>
        Exception Exception { get; }
    }
}
