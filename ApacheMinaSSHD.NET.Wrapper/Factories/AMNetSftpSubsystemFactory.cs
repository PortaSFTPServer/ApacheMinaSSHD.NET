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

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals;
using org.apache.sshd.sftp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    /// <summary>
    /// Configures the SFTP subsystem for an <see cref="AMNetSshServer"/>.
    /// </summary>
    public class AMNetSftpSubsystemFactory
    {
        private readonly SftpSubsystemFactory factory = new();

        /// <summary>
        /// Creates an SFTP subsystem factory.
        /// </summary>
        public AMNetSftpSubsystemFactory()
        {
        }

        internal SftpSubsystemFactory JavaFactory => factory;

        /// <summary>
        /// Registers an SFTP event listener.
        /// </summary>
        /// <param name="sftpEventListener">The listener that receives SFTP lifecycle and file events.</param>
        public void addSftpEventListener(IAMNetSftpEventListener sftpEventListener)
        {
            ArgumentNullException.ThrowIfNull(sftpEventListener);
            factory.addSftpEventListener(new InternalSftpEventListener(sftpEventListener));
        }

        /// <summary>
        /// Sets the SFTP filesystem policy hook.
        /// </summary>
        /// <param name="sftpFileSystemAccessor">The SFTP policy hook for paths, attributes, and filesystem operations.</param>
        public void setFileSystemAccessor(IAMNetSftpFileSystemAccessor sftpFileSystemAccessor)
        {
            ArgumentNullException.ThrowIfNull(sftpFileSystemAccessor);
            factory.setFileSystemAccessor(new InternalSftpFileSystemAccessor(sftpFileSystemAccessor, this));
        }
    }
}
