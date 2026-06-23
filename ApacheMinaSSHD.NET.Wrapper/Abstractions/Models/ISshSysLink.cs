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

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SFTP hard link or symbolic link events.
    /// </summary>
    public interface ISshSysLink
    {
        /// <summary>Gets the session associated with the link event.</summary>
        ISshSession Session { get; }
        /// <summary>Gets the source path.</summary>
        string SourcePath { get; }
        /// <summary>Gets the destination path.</summary>
        string DestPath { get; }
        /// <summary>Gets or sets whether the link is symbolic.</summary>
        public bool SymLink { get; set; }
        /// <summary>Gets the exception associated with the event when available.</summary>
        Exception Exception { get; }
    }
}
