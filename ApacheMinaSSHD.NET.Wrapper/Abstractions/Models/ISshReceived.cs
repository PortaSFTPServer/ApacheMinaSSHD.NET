using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for received SFTP messages.
    /// </summary>
    public interface ISshReceived
    {
        /// <summary>Gets the session associated with the received message.</summary>
        ISshSession SshSession { get; }

        /// <summary>Gets the numeric SFTP message type.</summary>
        public int Type { get; }

        /// <summary>Gets the extension name when the message is an extension.</summary>
        string Extension { get; }
        /// <summary>Gets the request identifier.</summary>
        int Id { get; }
    }
}
