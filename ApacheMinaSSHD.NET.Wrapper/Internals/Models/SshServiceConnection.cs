using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    /// <summary>
    /// This class encapsulates the parementers from the IoServiceEventListener.
    /// These are the <strong>IoConnector connector, SocketAddress local, AttributeRepository context, 
    /// SocketAddress remote, Exception reason</strong>.
    /// </summary>
    internal class SshServiceConnection: ISshServiceConnection
    {
        /// <summary>
        /// Local end point properties
        /// </summary>
        public IPEndPoint LocalEndPoint { get; set; } = null!;
        /// <summary>
        /// Remote end point properties
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; } = null!;
        /// <summary>
        /// Service end point properties
        /// </summary>
        public IPEndPoint ServiceEndPoint { get; set; } = null!;
        /// <summary>
        /// The I/O Manager (Acceptor/Connector)
        /// </summary>
        public ISshIoService IoService { get; set; } = null!;
        public IReadOnlyDictionary<string, object> Attributes { get;  set; } =
            new Dictionary<string, object>();

        /// <summary>
        /// Error message / information.
        /// </summary>
        public System.Exception Exception { get; internal set; } = null!;


    }
}
