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
using org.apache.sshd.sftp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshDirectoryHandle : ISshDirectoryHandle
    {
        private readonly DirectoryHandle _handle;
        private bool _isDisposed;

        /// <summary>
        /// Internal constructor: Client cannot instantiate this directly
        /// </summary>
        /// <param name="handle"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal SshDirectoryHandle(DirectoryHandle handle)
        {
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        /// <summary>
        /// Mapping properties 1:1
        /// </summary>
        public bool HasNext => _handle.hasNext();
        public bool IsDone => _handle.isDone();
        public bool ShouldSendDot => _handle.isSendDot();
        public bool ShouldSendDotDot => _handle.isSendDotDot();
        public bool IsWithDots => _handle.isWithDots();

        public string PhysicalPath => _handle.getFile()?.toString()!;


        /// <summary>
        /// Mapping methods
        /// </summary>
        /// <returns></returns>
        public string Next()
        {
            java.nio.file.Path path = _handle.next();
            return path?.toString()!; // Convert Java Path to C# string
        }

        public void MarkDone() => _handle.markDone();
        public void MarkDotSent() => _handle.markDotSent();
        public void MarkDotDotSent() => _handle.markDotDotSent();
        public void Remove() => _handle.remove();

        public void Close() => Dispose();

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _handle.close();
                _isDisposed = true;
            }
        }
    }
}