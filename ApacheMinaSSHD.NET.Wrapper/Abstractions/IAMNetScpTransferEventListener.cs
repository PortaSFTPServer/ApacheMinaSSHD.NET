using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Receives SCP transfer lifecycle and acknowledgement events.
    /// </summary>
    public interface IAMNetScpTransferEventListener
    {
        /// <summary>Called when a file transfer starts.</summary>
        /// <param name="context">The SCP transfer metadata.</param>
        void OnStartFile(ISshScpTransferEvent context) { }

        /// <summary>Called when a file transfer ends.</summary>
        /// <param name="context">The SCP transfer metadata.</param>
        void OnEndFile(ISshScpTransferEvent context) { }

        /// <summary>Called when a file acknowledgement is observed.</summary>
        /// <param name="context">The SCP transfer metadata.</param>
        void OnFileAck(ISshScpTransferEvent context) { }

        /// <summary>Called when an SCP receive command acknowledgement is observed.</summary>
        /// <param name="context">The SCP transfer metadata.</param>
        void OnReceiveCommandAck(ISshScpTransferEvent context) { }

        /// <summary>Called when a folder transfer starts.</summary>
        /// <param name="context">The SCP transfer metadata.</param>
        void OnStartFolder(ISshScpTransferEvent context) { }

        /// <summary>Called when a folder transfer ends.</summary>
        /// <param name="context">The SCP transfer metadata.</param>
        void OnEndFolder(ISshScpTransferEvent context) { }
    }
}
