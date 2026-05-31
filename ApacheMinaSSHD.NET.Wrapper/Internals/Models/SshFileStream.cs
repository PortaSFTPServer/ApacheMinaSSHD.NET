using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshFileStream : Stream
    {
        private readonly ISshFileHandle _handle;

        private long _position;

        public SshFileStream(ISshFileHandle handle) => _handle = handle;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException("Check attributes for length");
        public override long Position { get => _position; set => _position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _handle.Read(buffer, _position, offset, count);
            if (read > 0) _position += read;
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _handle.Write(buffer, _position, offset, count);
            _position += count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => throw new NotSupportedException("Seek from end is not supported."),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            return _position;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Flush() { } // SFTP is usually unbuffered at this level

    }

}