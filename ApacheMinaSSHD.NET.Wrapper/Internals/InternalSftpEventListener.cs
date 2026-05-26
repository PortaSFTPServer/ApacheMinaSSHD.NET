using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using com.sun.org.apache.bcel.@internal.generic;
using com.sun.tools.@internal.xjc.generator.bean.field;
using java.nio.file;
using jdk.@internal.util.xml.impl;
using org.apache.sshd.server.session;
using org.apache.sshd.sftp.server;
using System.Security.Cryptography;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    /// <summary>
    /// This is the 
    /// </summary>
    internal class InternalSftpEventListener : AbstractSftpEventListenerAdapter
    {

        private readonly IAMNetSftpEventListener sftpEventListener;

        public InternalSftpEventListener(IAMNetSftpEventListener sftpEventListener)
        {
            this.sftpEventListener = sftpEventListener;

        }

        public override void initialized(ServerSession session, int version)
        {
            ISshSession sshSession = new SshSession(session);
            sftpEventListener.OnInitialized(sshSession, version);
        }

        public override void destroying(ServerSession session)
        {
            ISshSession sshSession = new SshSession(session);
            sftpEventListener.OnDestroying(sshSession);
        }


        public override void readingEntries(ServerSession session, string remoteHandle, DirectoryHandle localHandle)
        {
            sftpEventListener.OnReadingEntries(CreateEntriesModel(session, remoteHandle, localHandle, null!));
        }



        public override void readEntries(ServerSession session, string remoteHandle, DirectoryHandle localHandle, java.util.Map entries)
        {
            sftpEventListener.OnReadEntries(CreateEntriesModel(session, remoteHandle, localHandle, entries));
        }

        public override void exiting(ServerSession session, Handle handle)
        {
            ISshSession sshSession = new SshSession(session);
            ISshHandle sshHandle = new SshHandle(handle);
            sftpEventListener.OnExiting(sshSession, sshHandle);

        }

        public override void receivedExtension(ServerSession session, string extension, int id)
        {

            sftpEventListener.OnReceivedExtension(CreateReceivedModel(session, 0, extension, id));

        }
        public override void received(ServerSession session, int type, int id)
        {
            ISshSession sshSession = new SshSession(session);
            sftpEventListener.OnReceived(CreateReceivedModel(session, type, string.Empty, id));
        }

        // --- Handle Based Events ---
        public override void opening(ServerSession session, string remoteHandle, Handle localHandle)
        {
            sftpEventListener.OnOpening(CreateHandleModel(session, remoteHandle, localHandle, null!));
        }

        public override void open(ServerSession session, string remoteHandle, Handle localHandle)
        {
            sftpEventListener.OnOpen(CreateHandleModel(session, remoteHandle, localHandle, null!));
        }


        public override void openFailed(ServerSession session, string remotePath, java.nio.file.Path localPath, bool isDirectory, System.Exception thrown)
        {
            sftpEventListener.OnOpenFailed(CreateIOFailureModel(session, remotePath, localPath, thrown));
        }

        public override void closing(ServerSession session, string remoteHandle, Handle localHandle)
        {
            sftpEventListener.OnClosing(CreateHandleModel(session, remoteHandle, localHandle, null!));
        }

        public override void closed(ServerSession session, string remoteHandle, Handle localHandle, Exception thrown)
        {
            sftpEventListener.OnClosed(CreateHandleModel(session, remoteHandle, localHandle, thrown));
        }

        public override void reading(ServerSession session, string remoteHandle, FileHandle localHandle,
            long offset, byte[] data, int dataOffset, int dataLen)
        {
            // we can pass this for the custom handler to provide more use case
            // var sshFileHandle = new SshFileHandle(localHandle);

            sftpEventListener.OnReading(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, dataLen));

        }

        public override void read(ServerSession session, string remoteHandle, FileHandle localHandle,
                                  long offset, byte[] data, int dataOffset, int dataLen, int readLen, Exception thrown)
        {

            sftpEventListener.OnRead(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, readLen, thrown));
        }

        public override void writing(ServerSession session, string remoteHandle, FileHandle localHandle,
            long offset, byte[] data, int dataOffset, int dataLen)
        {
            sftpEventListener.OnWriting(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, dataLen));
        }

        public override void written(ServerSession session, string remoteHandle, FileHandle localHandle,
            long offset, byte[] data, int dataOffset, int dataLen, System.Exception thrown)
        {
            sftpEventListener.OnWrite(CreateReadWriteModel(session, remoteHandle, localHandle, offset, data, dataLen, thrown));

        }

        // --- Path Based Events ---
        public override void creating(ServerSession session, java.nio.file.Path path, java.util.Map attrs)
        {
            sftpEventListener.OnCreating(CreatePathModel(session, path, attrs, null!));

        }
        public override void created(ServerSession session, java.nio.file.Path path, java.util.Map attrs, Exception thrown)
        {
            sftpEventListener.OnCreated(CreatePathModel(session, path, attrs, thrown));
        }

        public override void moving(ServerSession session, java.nio.file.Path srcPath, java.nio.file.Path dstPath, java.util.Collection opts)
        {
            sftpEventListener.OnMoving(CreateMoveContext(session, srcPath, dstPath, opts, null!));

        }

        public override void moved(ServerSession session, java.nio.file.Path src, java.nio.file.Path dst, java.util.Collection opts, Exception thrown)
        {
            sftpEventListener.OnMoved(CreateMoveContext(session, src, dst, opts, thrown));
        }


        public override void linking(ServerSession session, java.nio.file.Path source, java.nio.file.Path target, bool symLink)
        {
            sftpEventListener.OnLinking(CreateLinkContext(session, source, target, symLink, null!));
        }

        public override void linked(ServerSession session, java.nio.file.Path source, java.nio.file.Path target, bool symLink, System.Exception thrown)
        {
            sftpEventListener.OnLink(CreateLinkContext(session, source, target, symLink, null!));

        }
        public override void removing(ServerSession session, java.nio.file.Path path, bool isDirectory)
        {
            sftpEventListener.OnRemoving(CreatePathModel(session, path, null!, null!));
        }

        public override void removed(ServerSession session, java.nio.file.Path path, bool isDirectory, Exception thrown)
        {
            sftpEventListener.OnRemoved(CreatePathModel(session, path, null!, thrown));
        }

        public override void modifyingAttributes(ServerSession session, java.nio.file.Path path, java.util.Map attrs)
        {
            sftpEventListener.OnModifyingAttributes(CreatePathModel(session, path, attrs, null!));

        }

        public override void modifiedAttributes(ServerSession session, java.nio.file.Path path, java.util.Map attrs, System.Exception thrown)
        {
            sftpEventListener.OnModifiedAttributes(CreatePathModel(session, path, attrs, null!));

        }

        // --- Helper Mappers ---
        private ISshSysLink CreateLinkContext(ServerSession session, java.nio.file.Path src, java.nio.file.Path dst, bool symLink, Exception thrown = null!)
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

        private ISshEvent CreateHandleModel(ServerSession s, string rh, Handle lh, Exception t)
        {


            return new SshEvent
            {
                Session = new SshSession(s),
                RemoteHandle = rh,
                SshHandle = new SshHandle(lh),// LocalPath = lh?.getFile()?.toString() ?? "unknown",
                Exception = t != null ? new System.Exception(t.Message) : null!

            };
        }

        private ISshIOFailure CreateIOFailureModel(ServerSession s, string rh, java.nio.file.Path path, Exception t)
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
                IsDirectory = Directory.Exists(p.toString()), // File.GetAttributes(p.toString()).HasFlag(FileAttributes.Directory),
                Attributes = TranslateJavaMapToDictionary(attrs),
                Exception = t != null ? new System.Exception(t.Message) : null!
            };
        }

        private ISshReadWrite CreateReadWriteModel(ServerSession s, string rh, FileHandle lh, long off, byte[] data, int len, Exception t = null!) => new SshReadWrite()
        {

            Session = new SshSession(s),
            RemoteHandle = rh,
            SshHandle = new SshHandle(lh), // we can also use SshFileHandle BUT not need. This is Orginally LocalPath = lh?.getFile()?.toString() ?? "unknown",
            Offset = off,
            Length = len,
            Data = data,
            Exception = t
        };

        // private Exception MapEx(java.lang.Throwable t) => t != null ? new Exception(t.getMessage()) : null!;

        private IReadOnlyDictionary<string, object> TranslateJavaMapToDictionary(java.util.Map map)
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
           return  new SshEntries()
            {
                SshSession = new SshSession(session),
                RemoteHandle = remoteHandle,
                localHandle = new SshDirectoryHandle(localHandle),
                Entries = entries==null? new Dictionary<string, object>() : TranslateJavaMapToDictionary(entries)
            };

        }

        private ISshReceived CreateReceivedModel(ServerSession session, int type, string extension, int id)
        {
            return new SshReceived
            {
                SshSession = new SshSession(session),
                Type = type,
                Extension = extension,
                Id = id
            };
        }

        private IEnumerable<string> MapCollection(java.util.Collection col)
        {
            if (col == null) yield break;
            var iterator = col.iterator();
            while (iterator.hasNext()) yield return iterator.next()?.ToString() ?? string.Empty;
        }
    }
}
