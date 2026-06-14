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
using java.util.concurrent.atomic;
using org.apache.sshd.sftp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshFileHandle : ISshFileHandle
    {

        private readonly FileHandle _handle;

        private bool _isDisposed;

        internal SshFileHandle(FileHandle handle)
        {
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        public int AccessMask => _handle.getAccessMask();
        public bool IsOpenAppend => _handle.isOpenAppend();
        public ISshHandle SshHandle => new SshHandle(_handle);

        public IReadOnlyDictionary<string, object> Attributes
        {
            get
            {
                var dict = new Dictionary<string, object>();
                var javaAttrs = _handle.getFileAttributes();
                if (javaAttrs == null) return dict;

                var iterator = javaAttrs.iterator();
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
        }

        public void Append(byte[] data, int offset = 0, int? length = null)
        {
            if (length == null) _handle.append(data);
            else _handle.append(data, offset, length.Value);
        }

        public int Read(byte[] data, long fileOffset, int dataOffset = 0, int? length = null)
        {
            return _handle.read(data, dataOffset, length ?? (data.Length - dataOffset), fileOffset);
        }

        public int Read(byte[] data, long fileOffset, out bool isEof, int dataOffset = 0, int? length = null)
        {
            var eofRef = new AtomicReference(java.lang.Boolean.valueOf(false)); // Boolean.FalseString()

            int len = length ?? (data.Length - dataOffset);

            int bytesRead = _handle.read(data, dataOffset, len, fileOffset, eofRef);

            var javaBool = (java.lang.Boolean)eofRef.get();

            isEof = javaBool.booleanValue();

            return bytesRead;
        }

        public void Write(byte[] data, long fileOffset, int dataOffset = 0, int? length = null)
        {
            // avoid the user of Linq for faster processing (Zero Buffer Slicing)
            _handle.write(data, dataOffset, length ?? (data.Length - dataOffset), fileOffset);
        }

        public void Lock(long offset, long length, int mask)
        {
            try
            {
                _handle.@lock(offset, length, mask);
            }
            catch (java.io.IOException ex)
            {
                throw new System.IO.IOException(ex.getMessage(), ex);
            }
        }
        public void Unlock(long offset, long length)
        {
            try
            {
                _handle.unlock(offset, length);
            }
            catch (java.io.IOException ex)
            {
                throw new System.IO.IOException(ex.getMessage(), ex);
            }
        }
        public void Close() => Dispose();

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _handle.close();
                _isDisposed = true;
            }
        }

        public Stream AsStream()
        {
            // We can wrap this in a C# Stream by delegating Read/Write/Seek calls.
            return new SshFileStream(this);
        }
    }
}

