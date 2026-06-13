// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System.Net;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
  
    /// <summary>
    /// Provides metadata about the low-level I/O service handling a connection.
    /// </summary>
    public interface ISshIoService
    {
        /// <summary>Gets whether the service is accepting inbound connections.</summary>
        bool IsAcceptor { get; }
        /// <summary>Gets whether service shutdown has started.</summary>
        bool IsClosing { get; }
        /// <summary>Gets whether service shutdown is complete.</summary>
        bool IsClosed { get; }

        /// <summary>
        /// Gets all addresses this service is bound to.
        /// </summary>
        IEnumerable<IPEndPoint> BoundAddresses { get; }
    }


   

}
