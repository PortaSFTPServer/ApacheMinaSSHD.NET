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

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Receives SFTP subsystem, file handle, path, and message events.
    /// </summary>
    public interface IAMNetSftpEventListener
    {

        // Entry events
        /// <summary>Called when the SFTP subsystem is initialized for a session.</summary>
        /// <param name="sshSession">The session metadata.</param>
        /// <param name="version">The negotiated SFTP protocol version.</param>
        public void OnInitialized(ISshSession sshSession, int version);
        /// <summary>Called when the SFTP subsystem is being destroyed for a session.</summary>
        /// <param name="sshSession">The session metadata.</param>
        public void OnDestroying(ISshSession sshSession);
        /// <summary>Called before directory entries are read.</summary>
        /// <param name="sshEntries">Directory read metadata.</param>
        public void OnReadingEntries(ISshEntries sshEntries);
        /// <summary>Called after directory entries are read.</summary>
        /// <param name="sshEntries">Directory read metadata and entries.</param>
        public void OnReadEntries(ISshEntries sshEntries);
        /// <summary>Called when an SFTP handle exits.</summary>
        /// <param name="sshSession">The session metadata.</param>
        /// <param name="sshHandle">The handle metadata.</param>
        void OnExiting(ISshSession sshSession, ISshHandle sshHandle);
        /// <summary>Called when an SFTP extension message is received.</summary>
        /// <param name="sshReceived">Received message metadata.</param>
        void OnReceivedExtension(ISshReceived sshReceived);
        /// <summary>Called when an SFTP message is received.</summary>
        /// <param name="sshReceived">Received message metadata.</param>
        void OnReceived(ISshReceived sshReceived);

        // Handle-based events
        /// <summary>Called before a handle is opened.</summary>
        /// <param name="ctx">The handle event metadata.</param>
        public void OnOpening(ISshEvent ctx);
        /// <summary>Called after a handle is opened.</summary>
        /// <param name="ctx">The handle event metadata.</param>
        public void OnOpen(ISshEvent ctx);
        /// <summary>Called after a handle open fails.</summary>
        /// <param name="ctx">The failure metadata.</param>
        public void OnOpenFailed(ISshIOFailure ctx);
        /// <summary>Called before a handle is closed.</summary>
        /// <param name="ctx">The handle event metadata.</param>
        public void OnClosing(ISshEvent ctx);
        /// <summary>Called after a handle is closed.</summary>
        /// <param name="ctx">The handle event metadata.</param>
        public void OnClosed(ISshEvent ctx);
        /// <summary>Called before file data is read.</summary>
        /// <param name="ctx">Read metadata.</param>
        public void OnReading(ISshReadWrite ctx);
        /// <summary>Called after file data is read.</summary>
        /// <param name="ctx">Read metadata.</param>
        public void OnRead(ISshReadWrite ctx);
        /// <summary>Called before file data is written.</summary>
        /// <param name="ctx">Write metadata.</param>
        public void OnWriting(ISshReadWrite ctx);
        /// <summary>Called after file data is written.</summary>
        /// <param name="ctx">Write metadata.</param>
        public void OnWrite(ISshReadWrite ctx);

        // Path-based events (Create, Move, Remove, Attributes)
        /// <summary>Called before a path is created.</summary>
        /// <param name="ctx">Path metadata.</param>
        public void OnCreating(ISshPath ctx);
        /// <summary>Called after a path is created.</summary>
        /// <param name="ctx">Path metadata.</param>
        public void OnCreated(ISshPath ctx);
        /// <summary>Called before a path is removed.</summary>
        /// <param name="ctx">Path metadata.</param>
        public void OnRemoving(ISshPath ctx);
        /// <summary>Called after a path is removed.</summary>
        /// <param name="ctx">Path metadata.</param>
        public void OnRemoved(ISshPath ctx);
        /// <summary>Called before a path is moved.</summary>
        /// <param name="ctx">Move metadata.</param>
        public void OnMoving(ISshMove ctx);
        /// <summary>Called after a path is moved.</summary>
        /// <param name="ctx">Move metadata.</param>
        public void OnMoved(ISshMove ctx);
        /// <summary>Called before path attributes are modified.</summary>
        /// <param name="ctx">Path metadata.</param>
        public void OnModifyingAttributes(ISshPath ctx);
        /// <summary>Called after path attributes are modified.</summary>
        /// <param name="ctx">Path metadata.</param>
        public void OnModifiedAttributes(ISshPath ctx);

        // Sys Link based events
        /// <summary>Called before a hard link or symbolic link is created.</summary>
        /// <param name="ctx">Link metadata.</param>
        public void OnLinking(ISshSysLink ctx);
        /// <summary>Called after a hard link or symbolic link is created.</summary>
        /// <param name="ctx">Link metadata.</param>
        public void OnLink(ISshSysLink ctx);

    }
}
