using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SFTP read and write events.
    /// </summary>
    public interface ISshReadWrite : ISshEvent
    {
        /// <summary>Gets the file offset for the read or write operation.</summary>
        public long Offset { get;}
        /// <summary>Gets the number of bytes requested or processed.</summary>
        public int Length { get;  }
        /// <summary>Gets the data buffer associated with the read or write operation.</summary>
        public byte[] Data { get;  }
    }
}
