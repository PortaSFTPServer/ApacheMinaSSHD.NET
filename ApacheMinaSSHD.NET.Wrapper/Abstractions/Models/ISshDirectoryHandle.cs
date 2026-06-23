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
    /// Represents an open SFTP directory handle.
    /// </summary>
    public interface ISshDirectoryHandle : IDisposable
    {
        /// <summary>Gets whether another directory entry is available.</summary>
        bool HasNext { get; }
        /// <summary>Gets whether the directory stream has completed.</summary>
        bool IsDone { get; }
        /// <summary>Gets whether the dot entry should still be sent.</summary>
        bool ShouldSendDot { get; }
        /// <summary>Gets whether the dot-dot entry should still be sent.</summary>
        bool ShouldSendDotDot { get; }
        /// <summary>Gets whether dot entries are included for this handle.</summary>
        bool IsWithDots { get; }
        /// <summary>Gets the local physical path associated with the handle.</summary>
        public string PhysicalPath { get; }
        /// <summary>Gets the next directory entry path as a string.</summary>
        string Next();
        /// <summary>Marks the directory stream as done.</summary>
        void MarkDone();
        /// <summary>Marks the dot entry as sent.</summary>
        void MarkDotSent();
        /// <summary>Marks the dot-dot entry as sent.</summary>
        void MarkDotDotSent();
        /// <summary>Removes the current directory entry when supported.</summary>
        void Remove();
        /// <summary>Closes the directory handle.</summary>
        void Close();
    }
}
