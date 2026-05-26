using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SFTP path-based events.
    /// </summary>
    public interface ISshPath
    {
        /// <summary>Gets the session associated with the path event.</summary>
        ISshSession Session { get; }
        /// <summary>Gets the local or reported path associated with the event.</summary>
        string Path { get; }
        /// <summary>Gets whether the path is a directory.</summary>
        public bool IsDirectory { get;  }
        /// <summary>Gets file attributes associated with the path.</summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
        /// <summary>Gets the exception associated with the event when available.</summary>
        Exception Exception { get; }
    }
}
