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
    internal class SshHandle: ISshHandle
    {

        // Reference to the Java Handle
        internal readonly Handle _javaHandle;

        private bool _isDisposed;

        /// <summary>
        /// java server handle
        /// </summary>
        /// <param name="javaHandle">The Java Handle instance to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when javaHandle is null.</exception>
        internal SshHandle(Handle javaHandle)
        {
            _javaHandle = javaHandle ?? throw new ArgumentNullException(nameof(javaHandle));
        }

        // getFileHandle() returns the remote ID string
        public string Id => _javaHandle.getFileHandle();

        // getFile() returns the java.nio.file.Path
        public string PhysicalPath => _javaHandle.getFile()?.toString()!;

        public bool IsOpen => _javaHandle.isOpen();

        public void Close() => Dispose();

        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    // Always close the Java handle to release the file system resource
                    _javaHandle.close();
                }
                catch (java.io.IOException ex)
                {
                    // Map Java IO exception to C#
                    throw new System.IO.IOException(ex.getMessage(), ex);
                }
                _isDisposed = true;
            }
        }

    }
}
