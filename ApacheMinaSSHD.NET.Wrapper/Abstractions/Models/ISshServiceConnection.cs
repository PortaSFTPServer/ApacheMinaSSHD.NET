// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides low-level connection metadata for I/O service events.
    /// </summary>
    public interface ISshServiceConnection
    {
        /// <summary>Gets the local endpoint for the connection.</summary>
        IPEndPoint LocalEndPoint { get; }
        /// <summary>Gets the remote endpoint for the connection.</summary>
        IPEndPoint RemoteEndPoint { get; }
        /// <summary>Gets the service endpoint associated with the connection.</summary>
        IPEndPoint ServiceEndPoint { get; }

        /// <summary>
        /// Gets connection attributes when available.
        /// </summary>
        IReadOnlyDictionary<string, object> Attributes { get; }

        /// <summary>Gets metadata for the I/O service handling the connection.</summary>
        ISshIoService IoService { get; }
        /// <summary>Gets the exception associated with the connection event when available.</summary>
        Exception Exception { get; }
    }
}
