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
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using org.apache.sshd.server.session;
using org.apache.sshd.sftp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    /// <summary>
    /// Bridges SFTP event callbacks from Apache MINA SSHD to the .NET IAMNetSftpEventListener abstraction.
    /// </summary>
    internal class InternalSftpEventListener : AbstractSftpEventListenerAdapter
    {

        private readonly IAMNetSftpEventListener sftpEventListener;
        static readonly IAMNetLogger logger = new AMNetLogger(typeof(InternalSftpEventListener), AMNetLogger.LogLevel.Info);

        private static readonly string?[] SftpTypeNames = new string?[]
        {
            null, "INIT", null, "OPEN", "CLOSE", "READ", "WRITE", "LSTAT", "FSTAT",
            "SETSTAT", "FSETSTAT", "OPENDIR", "READDIR", "REMOVE", "MKDIR", "RMDIR",
            "REALPATH", "STAT", "RENAME", "READLINK", "SYMLINK"
        };

        private static readonly string?[] SftpTypeDescriptions = new string?[]
        {
            null, "Session initialization", null, "Open file", "Close handle", "Read file data", "Write file data",
            "Get attributes (no follow)", "Get attributes (handle)", "Set attributes (path)", "Set attributes (handle)",
            "Open directory", "Read directory entries", "Delete file", "Create directory", "Remove directory",
            "Resolve path", "Get file attributes", "Rename file", "Read symlink target", "Create symlink"
        };

        private static string FmtHandle(string handle) =>
            string.IsNullOrEmpty(handle) ? "" : $" ({handle})";

        public InternalSftpEventListener(IAMNetSftpEventListener sftpEventListener)
        {
            this.sftpEventListener = sftpEventListener;

        }

        private static string SessionInfo(ServerSession session)
        {
            var user = session.getUsername() ?? "?";
            string addr;
            try { addr = session.getIoSession()?.getRemoteAddress()?.toString() ?? "?"; } catch { addr = "?"; }
            return $"{user}@{addr}";
        }

        public override void initialized(ServerSession session, int version)
        {
            ISshSession sshSession = new SshSession(session);
            logger.Debug($"[{SessionInfo(session)}] SFTP initialized version {version}");
            sftpEventListener?.OnInitialized(sshSession, version);
        }

        public override void destroying(ServerSession session)
        {
            ISshSession sshSession = new SshSession(session);
            logger.Debug($"[{SessionInfo(session)}] SFTP session destroyed");
            sftpEventListener?.OnDestroying(sshSession);
        }


        public override void readingEntries(ServerSession session, string remoteHandle, DirectoryHandle localHandle)
        {
            logger.Debug($"[{SessionInfo(session)}] Listing directory entries{FmtHandle(remoteHandle)}");
            sftpEventListener?.OnReadingEntries(CreateEntriesModel(session, remoteHandle, localHandle, null!));
        }



        public override void readEntries(ServerSession session, string remoteHandle, DirectoryHandle localHandle, java.util.Map entries)
        {
            logger.Debug($"[{SessionInfo(session)}] Listed directory entries{FmtHandle(remoteHandle)} -> {entries?.size()} items");
            sftpEventListener?.OnReadEntries(CreateEntriesModel(session, remoteHandle, localHandle, entries));
        }

        public override void exiting(ServerSession session, Handle handle)
        {
            ISshSession sshSession = new SshSession(session);
            ISshHandle sshHandle = new SshHandle(handle);
            logger.Debug($"[{SessionInfo(session)}] SFTP exiting handle {sshHandle.PhysicalPath}");
            sftpEventListener?.OnExiting(sshSession, sshHandle);

        }

        public override void receivedExtension(ServerSession session, string extension, int id)
        {
            logger.Debug($"[{SessionInfo(session)}] SFTP extension {extension} (id={id})");
            sftpEventListener?.OnReceivedExtension(CreateReceivedModel(session, 0, extension, id));

        }
        public override void received(ServerSession session, int type, int id)
        {
            ISshSession sshSession = new SshSession(session);
            var typeName = type >= 0 && type < SftpTypeNames.Length ? SftpTypeNames[type] : null;
            var typeDesc = type >= 0 && type < SftpTypeDescriptions.Length ? SftpTypeDescriptions[type] : null;
            if (typeName != null)
                logger.Debug($"[{SessionInfo(session)}] {typeName} ({typeDesc}) id={id}");
            else
                logger.Debug($"[{SessionInfo(session)}] SFTP type={type} id={id}");
            sftpEventListener?.OnReceived(CreateReceivedModel(session, type, string.Empty, id));
        }

        // --- Handle Based Events ---
        private string LocalPath(Handle handle)
        {
            try { return handle?.toString() ?? "?"; } catch { return "?"; }
        }

        public override void opening(ServerSession session, string remoteHandle, Handle localHandle)
        {
            logger.Debug($"[{SessionInfo(session)}] Opening file {LocalPath(localHandle)}{FmtHandle(remoteHandle)}");
            sftpEventListener?.OnOpening(CreateHandleModel(session, remoteHandle, localHandle, null!));
        }

        public override void open(ServerSession session, string remoteHandle, Handle localHandle)
        {
            logger.Debug($"[{SessionInfo(session)}] Opened file {LocalPath(localHandle)}{FmtHandle(remoteHandle)}");
            sftpEventListener?.OnOpen(CreateHandleModel(session, remoteHandle, localHandle, null!));
        }


        public override void openFailed(ServerSession session, string remotePath, java.nio.file.Path localPath, bool isDirectory, System.Exception thrown)
        {
            logger.Debug($"[{SessionInfo(session)}] Failed to open {remotePath} -> {localPath}: {thrown?.Message}");
            sftpEventListener?.OnOpenFailed(CreateIOFailureModel(session, remotePath, localPath, thrown));
        }

        public override void closing(ServerSession session, string remoteHandle, Handle localHandle)
        {
            logger.Debug($"[{SessionInfo(session)}] Closing file {LocalPath(localHandle)}{FmtHandle(remoteHandle)}");
            sftpEventListener?.OnClosing(CreateHandleModel(session, remoteHandle, localHandle, null!));
        }

        public override void closed(ServerSession session, string remoteHandle, Handle localHandle, Exception thrown)
        {
            logger.Debug($"[{SessionInfo(session)}] Closed file {LocalPath(localHandle)}{FmtHandle(remoteHandle)}");
            sftpEventListener?.OnClosed(CreateHandleModel(session, remoteHandle, localHandle, thrown));
        }

        public override void reading(ServerSession session, string remoteHandle, FileHandle localHandle,
            long offset, byte[] data, int dataOffset, int dataLen)
        {
            logger.Debug($"[{SessionInfo(session)}] Downloading {LocalPath(localHandle)} offset={offset} len={dataLen}");
            sftpEventListener?.OnReading(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, dataLen));

        }

        public override void read(ServerSession session, string remoteHandle, FileHandle localHandle,
                                  long offset, byte[] data, int dataOffset, int dataLen, int readLen, Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Download {LocalPath(localHandle)} offset={offset} len={readLen} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Downloaded {LocalPath(localHandle)} offset={offset} len={readLen}");
            sftpEventListener?.OnRead(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, readLen, thrown));
        }

        public override void writing(ServerSession session, string remoteHandle, FileHandle localHandle,
            long offset, byte[] data, int dataOffset, int dataLen)
        {
            logger.Debug($"[{SessionInfo(session)}] Uploading {LocalPath(localHandle)} offset={offset} len={dataLen}");
            sftpEventListener?.OnWriting(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, dataLen));
        }

        public override void written(ServerSession session, string remoteHandle, FileHandle localHandle,
            long offset, byte[] data, int dataOffset, int dataLen, System.Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Upload {LocalPath(localHandle)} offset={offset} len={dataLen} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Uploaded {LocalPath(localHandle)} offset={offset} len={dataLen}");
            sftpEventListener?.OnWrite(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, dataLen, thrown));

        }

        // --- Path Based Events ---
        public override void creating(ServerSession session, java.nio.file.Path path, java.util.Map attrs)
        {
            logger.Debug($"[{SessionInfo(session)}] Creating {path}");
            sftpEventListener?.OnCreating(CreatePathModel(session, path, attrs, null!));

        }
        public override void created(ServerSession session, java.nio.file.Path path, java.util.Map attrs, Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Create {path} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Created {path}");
            sftpEventListener?.OnCreated(CreatePathModel(session, path, attrs, thrown));
        }

        public override void moving(ServerSession session, java.nio.file.Path srcPath, java.nio.file.Path dstPath, java.util.Collection opts)
        {
            logger.Debug($"[{SessionInfo(session)}] Moving {srcPath} -> {dstPath}");
            sftpEventListener?.OnMoving(CreateMoveContext(session, srcPath, dstPath, opts, null!));

        }

        public override void moved(ServerSession session, java.nio.file.Path src, java.nio.file.Path dst, java.util.Collection opts, Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Move {src} -> {dst} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Moved {src} -> {dst}");
            sftpEventListener?.OnMoved(CreateMoveContext(session, src, dst, opts, thrown));
        }


        public override void linking(ServerSession session, java.nio.file.Path source, java.nio.file.Path target, bool symLink)
        {
            logger.Debug($"[{SessionInfo(session)}] Linking {source} -> {target} (symLink={symLink})");
            sftpEventListener?.OnLinking(CreateLinkContext(session, source, target, symLink, null!));
        }

        public override void linked(ServerSession session, java.nio.file.Path source, java.nio.file.Path target, bool symLink, System.Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Link {source} -> {target} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Linked {source} -> {target}");
            sftpEventListener?.OnLink(CreateLinkContext(session, source, target, symLink, null!));

        }
        public override void removing(ServerSession session, java.nio.file.Path path, bool isDirectory)
        {
            logger.Debug($"[{SessionInfo(session)}] Deleting {(isDirectory ? "directory" : "file")} {path}");
            sftpEventListener?.OnRemoving(CreatePathModel(session, path, null!, null!));
        }

        public override void removed(ServerSession session, java.nio.file.Path path, bool isDirectory, Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Delete {(isDirectory ? "directory" : "file")} {path} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Deleted {(isDirectory ? "directory" : "file")} {path}");
            sftpEventListener?.OnRemoved(CreatePathModel(session, path, null!, thrown));
        }

        public override void modifyingAttributes(ServerSession session, java.nio.file.Path path, java.util.Map attrs)
        {
            logger.Debug($"[{SessionInfo(session)}] Modifying attributes for {path}");
            sftpEventListener?.OnModifyingAttributes(CreatePathModel(session, path, attrs, null!));

        }

        public override void modifiedAttributes(ServerSession session, java.nio.file.Path path, java.util.Map attrs, System.Exception thrown)
        {
            if (thrown != null)
                logger.Debug($"[{SessionInfo(session)}] Modify attributes for {path} FAILED: {thrown.Message}");
            else
                logger.Debug($"[{SessionInfo(session)}] Modified attributes for {path}");
            sftpEventListener?.OnModifiedAttributes(CreatePathModel(session, path, attrs, null!));

        }

        // --- Helper Mappers ---
        private static ISshSysLink CreateLinkContext(ServerSession session, java.nio.file.Path src, java.nio.file.Path dst, bool symLink, Exception thrown = null!)
        {
            return new SshSysLink
            {
                Session = new SshSession(session),
                SourcePath = src.toString(),
                DestPath = dst.toString(),
                SymLink = symLink,
                Exception = thrown != null ? new System.Exception(thrown.Message) : null!
            };
        }
        private ISshMove CreateMoveContext(ServerSession session, java.nio.file.Path src, java.nio.file.Path dst, java.util.Collection opts, Exception thrown = null!)
        {
            return new SshMove
            {
                Session = new SshSession(session),
                SourcePath = src.toString(),
                DestPath = dst.toString(),
                Options = MapCollection(opts),
                Exception = thrown != null ? new System.Exception(thrown.Message) : null!
            };
        }

        private static ISshEvent CreateHandleModel(ServerSession s, string rh, Handle lh, Exception t)
        {


            return new SshEvent
            {
                Session = new SshSession(s),
                RemoteHandle = rh,
                SshHandle = new SshHandle(lh),
                Exception = t != null ? new System.Exception(t.Message) : null!

            };
        }

        private static ISshIOFailure CreateIOFailureModel(ServerSession s, string rh, java.nio.file.Path path, Exception t)
        {
            return new SshIOFailure
            {
                Session = new SshSession(s),
                RemoteHandle = rh,
                LocalPath = path.toString(),
                Exception = t != null ? new System.Exception(t.Message) : null!

            };
        }

        private ISshPath CreatePathModel(ServerSession s, java.nio.file.Path p, java.util.Map attrs, Exception t = null!)
        {
            return new SshPath
            {
                Session = new SshSession(s),
                Path = p.toString(),
                IsDirectory = Directory.Exists(p.toString()),
                Attributes = TranslateJavaMapToDictionary(attrs),
                Exception = t != null ? new System.Exception(t.Message) : null!
            };
        }

        private static ISshReadWrite CreateReadWriteModel(ServerSession s, string rh, FileHandle lh, long off, byte[] data, int len, Exception t = null!) => new SshReadWrite()
        {

            Session = new SshSession(s),
            RemoteHandle = rh,
            SshHandle = new SshHandle(lh),
            Offset = off,
            Length = len,
            Data = data,
            Exception = t
        };


        private static IReadOnlyDictionary<string, object> TranslateJavaMapToDictionary(java.util.Map map)
        {
            // translate to java dictionary
            var dict = new Dictionary<string, object>();
            if (map == null) return dict;
            var iterator = map.entrySet().iterator();
            while (iterator.hasNext())
            {
                var entry = (java.util.Map.Entry)iterator.next();
                string? key = entry.getKey()?.ToString();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    dict[key] = entry.getValue();
                }
            }
            return dict;
        }

        private ISshEntries CreateEntriesModel(ServerSession session, string remoteHandle, DirectoryHandle localHandle, java.util.Map entries)
        {
            return new SshEntries()
            {
                SshSession = new SshSession(session),
                RemoteHandle = remoteHandle,
                localHandle = new SshDirectoryHandle(localHandle),
                Entries = entries == null ? new Dictionary<string, object>() : TranslateJavaMapToDictionary(entries)
            };

        }

        private static ISshReceived CreateReceivedModel(ServerSession session, int type, string extension, int id)
        {
            return new SshReceived
            {
                SshSession = new SshSession(session),
                Type = type,
                Extension = extension,
                Id = id
            };
        }

        private static IEnumerable<string> MapCollection(java.util.Collection col)
        {
            if (col == null) yield break;
            var iterator = col.iterator();
            while (iterator.hasNext()) yield return iterator.next()?.ToString() ?? string.Empty;
        }
    }
}
