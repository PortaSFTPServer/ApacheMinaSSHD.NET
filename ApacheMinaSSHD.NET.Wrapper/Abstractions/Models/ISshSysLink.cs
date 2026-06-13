// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for SFTP hard link or symbolic link events.
    /// </summary>
    public interface ISshSysLink
    {
        /// <summary>Gets the session associated with the link event.</summary>
        ISshSession Session { get; }
        /// <summary>Gets the source path.</summary>
        string SourcePath { get; }
        /// <summary>Gets the destination path.</summary>
        string DestPath { get; }
        /// <summary>Gets or sets whether the link is symbolic.</summary>
        public bool SymLink { get; set; }
        /// <summary>Gets the exception associated with the event when available.</summary>
        Exception Exception { get; }
    }
}
