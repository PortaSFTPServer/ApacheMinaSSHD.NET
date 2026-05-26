using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for completed SFTP directory read operations.
    /// </summary>
    public interface ISshReadEntries
    {
        /// <summary>Gets the session associated with the directory read.</summary>
        ISshSession Session { get; }
        /// <summary>Gets the remote directory handle identifier.</summary>
        string RemoteHandle { get; }
        /// <summary>Gets the local directory handle wrapper.</summary>
        ISshDirectoryHandle DirectoryHandle { get; }
        /// <summary>Gets the directory entries returned to the client.</summary>
        public IReadOnlyDictionary<string, object> Entries { get;  }
        /// <summary>Gets the exception associated with the read when available.</summary>
        public Exception Exception { get; }


    }
}
