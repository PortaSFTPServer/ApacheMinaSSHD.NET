// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SFTP directory entry read events.
    /// </summary>
    public interface ISshEntries
    {

        /// <summary>Gets the session associated with the directory read.</summary>
        public ISshSession SshSession { get; }
        /// <summary>Gets the remote directory handle identifier.</summary>
        public string RemoteHandle { get; }
        /// <summary>Gets the local directory handle wrapper.</summary>
        public ISshDirectoryHandle localHandle { get; }
        /// <summary>Gets the directory entries returned to the client.</summary>
        public IReadOnlyDictionary<string, object> Entries { get; }
    }
}
