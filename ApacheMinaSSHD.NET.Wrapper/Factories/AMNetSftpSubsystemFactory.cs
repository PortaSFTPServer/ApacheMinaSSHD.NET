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
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals;
using org.apache.sshd.common;
using org.apache.sshd.sftp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    public class AMNetSftpSubsystemFactory
    {
        private readonly SftpSubsystemFactory factory = new();

        public static readonly IAMNetSftpEventListener DefaultListener = new NoOpSftpEventListener();

        public AMNetSftpSubsystemFactory()
        {
        }

        internal SftpSubsystemFactory JavaFactory => factory;

        internal const string SftpMaxVersionProperty = "sftp-max-version";

        /// <summary>
        /// Gets or sets the maximum SFTP protocol version to negotiate with clients.
        /// Valid range: 3–6. Must be set before the factory is passed to <c>setSubsystemFactories</c>.
        /// Default is 6 (the highest supported by Apache MINA SSHD).
        /// </summary>
        public int MaximumVersion
        {
            get => PropertyResolverUtils.getIntProperty((PropertyResolver)(object)factory, SftpMaxVersionProperty, 6);
            set
            {
                if (value < 3 || value > 6)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "SFTP version must be between 3 and 6.");
                PropertyResolverUtils.updateProperty((PropertyResolver)(object)factory, SftpMaxVersionProperty, value);
            }
        }

        public void addSftpEventListener(IAMNetSftpEventListener? sftpEventListener)
        {
            factory.addSftpEventListener(new InternalSftpEventListener(sftpEventListener));
        }

        private sealed class NoOpSftpEventListener : IAMNetSftpEventListener
        {
            public void OnInitialized(ISshSession sshSession, int version) { }
            public void OnDestroying(ISshSession sshSession) { }
            public void OnReadingEntries(ISshEntries sshEntries) { }
            public void OnReadEntries(ISshEntries sshEntries) { }
            public void OnExiting(ISshSession sshSession, ISshHandle sshHandle) { }
            public void OnReceivedExtension(ISshReceived sshReceived) { }
            public void OnReceived(ISshReceived sshReceived) { }
            public void OnOpening(ISshEvent ctx) { }
            public void OnOpen(ISshEvent ctx) { }
            public void OnOpenFailed(ISshIOFailure ctx) { }
            public void OnClosing(ISshEvent ctx) { }
            public void OnClosed(ISshEvent ctx) { }
            public void OnReading(ISshReadWrite ctx) { }
            public void OnRead(ISshReadWrite ctx) { }
            public void OnWriting(ISshReadWrite ctx) { }
            public void OnWrite(ISshReadWrite ctx) { }
            public void OnCreating(ISshPath ctx) { }
            public void OnCreated(ISshPath ctx) { }
            public void OnRemoving(ISshPath ctx) { }
            public void OnRemoved(ISshPath ctx) { }
            public void OnMoving(ISshMove ctx) { }
            public void OnMoved(ISshMove ctx) { }
            public void OnModifyingAttributes(ISshPath ctx) { }
            public void OnModifiedAttributes(ISshPath ctx) { }
            public void OnLinking(ISshSysLink ctx) { }
            public void OnLink(ISshSysLink ctx) { }
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
