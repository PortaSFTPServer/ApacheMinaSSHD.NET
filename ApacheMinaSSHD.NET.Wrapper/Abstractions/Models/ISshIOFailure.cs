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
    /// Provides metadata for an SFTP I/O failure.
    /// </summary>
    public interface ISshIOFailure
    {
        /// <summary>Gets or sets the session associated with the failure.</summary>
        public ISshSession Session { get; set; }

        /// <summary>Gets or sets the remote handle associated with the failure.</summary>
        public string RemoteHandle { get; set; }

        /// <summary>Gets or sets the local path associated with the failure.</summary>
        public string LocalPath { get; set; }

        /// <summary>Gets or sets the exception associated with the failure.</summary>
        public Exception Exception { get; set; }

    }
}
