using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Represents an open SFTP handle exposed through .NET-safe metadata.
    /// </summary>
    public interface ISshHandle : IDisposable
    {
        /// <summary>Gets the server handle identifier.</summary>
        string Id { get; }
        /// <summary>Gets the local physical path associated with the handle.</summary>
        string PhysicalPath { get; }
        /// <summary>Gets whether the handle is still open.</summary>
        bool IsOpen { get; }
    }
}
