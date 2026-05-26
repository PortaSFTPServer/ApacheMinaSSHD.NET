using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for an SFTP handle event.
    /// </summary>
    public interface ISshEvent
    {
        /// <summary>Gets the session associated with the event.</summary>
        ISshSession Session { get; }
        /// <summary>Gets the remote handle identifier associated with the event.</summary>
        string RemoteHandle { get; }
        /// <summary>Gets the SFTP handle associated with the event.</summary>
        public ISshHandle SshHandle { get; }

        /// <summary>Gets the exception associated with the event when available.</summary>
        Exception Exception { get; }
    }
}
