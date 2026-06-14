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
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.nio.file.attribute;
using java.util;
using org.apache.sshd.common.session;
using org.apache.sshd.scp.common;
using org.apache.sshd.scp.common.helpers;
using Path = java.nio.file.Path;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class InternalScpTransferEventListener : java.lang.Object, ScpTransferEventListener
    {
        private readonly IAMNetScpTransferEventListener eventListener;

        public InternalScpTransferEventListener(IAMNetScpTransferEventListener eventListener)
        {
            this.eventListener = eventListener;
        }

        public void startFileEvent(
            Session session,
            ScpTransferEventListener.FileOperation operation,
            Path file,
            long length,
            Set permissions)
        {
            eventListener.OnStartFile(CreateContext(session, operation, file, length, permissions));
        }

        public void endFileEvent(
            Session session,
            ScpTransferEventListener.FileOperation operation,
            Path file,
            long length,
            Set permissions,
            Exception thrown)
        {
            eventListener.OnEndFile(CreateContext(session, operation, file, length, permissions, thrown: thrown));
        }

        public void handleFileEventAckInfo(
            Session session,
            ScpTransferEventListener.FileOperation operation,
            Path file,
            long length,
            Set permissions,
            ScpAckInfo ackInfo)
        {
            eventListener.OnFileAck(CreateContext(session, operation, file, length, permissions, ackInfo: ackInfo));
        }

        public void handleReceiveCommandAckInfo(Session session, string command, ScpAckInfo ackInfo)
        {
            eventListener.OnReceiveCommandAck(new SshScpTransferEvent
            {
                Session = new SshSession((org.apache.sshd.server.session.ServerSession)session),
                Operation = "RECEIVE",
                Path = string.Empty,
                Command = command,
                AckStatusCode = ackInfo?.getStatusCode(),
                AckLine = ackInfo?.getLine()
            });
        }

        public void startFolderEvent(
            Session session,
            ScpTransferEventListener.FileOperation operation,
            Path file,
            Set permissions)
        {
            eventListener.OnStartFolder(CreateContext(session, operation, file, 0, permissions));
        }

        public void endFolderEvent(
            Session session,
            ScpTransferEventListener.FileOperation operation,
            Path file,
            Set permissions,
            Exception thrown)
        {
            eventListener.OnEndFolder(CreateContext(session, operation, file, 0, permissions, thrown: thrown));
        }

        private static SshScpTransferEvent CreateContext(
            Session session,
            ScpTransferEventListener.FileOperation operation,
            Path file,
            long length,
            Set permissions,
            ScpAckInfo? ackInfo = null,
            Exception? thrown = null)
        {
            return new SshScpTransferEvent
            {
                Session = new SshSession((org.apache.sshd.server.session.ServerSession)session),
                Operation = operation.name(),
                Path = file.toString(),
                Length = length,
                Permissions = ToPermissionList(permissions),
                AckStatusCode = ackInfo?.getStatusCode(),
                AckLine = ackInfo?.getLine(),
                Exception = thrown
            };
        }

        private static IReadOnlyList<string> ToPermissionList(Set? permissions)
        {
            if (permissions == null)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            var iterator = permissions.iterator();
            while (iterator.hasNext())
            {
                result.Add(iterator.next()?.ToString() ?? string.Empty);
            }

            return result;
        }
    }
}
