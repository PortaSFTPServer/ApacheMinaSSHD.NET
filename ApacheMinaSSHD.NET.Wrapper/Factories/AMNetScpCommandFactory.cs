using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals;
using org.apache.sshd.scp.server;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    /// <summary>
    /// Configures SCP command support for an <see cref="AMNetSshServer"/>.
    /// </summary>
    public class AMNetScpCommandFactory
    {
        private readonly ScpCommandFactory factory = new();
        private readonly Dictionary<IAMNetScpTransferEventListener, InternalScpTransferEventListener> eventListeners = new();

        /// <summary>
        /// Creates an SCP command factory with default file handling.
        /// </summary>
        public AMNetScpCommandFactory()
        {
        }

        /// <summary>
        /// Creates an SCP command factory with an application file policy hook.
        /// </summary>
        /// <param name="fileOpener">The SCP file policy hook.</param>
        public AMNetScpCommandFactory(IAMNetScpFileOpener fileOpener)
        {
            setFileOpener(fileOpener);
        }

        internal ScpCommandFactory JavaFactory => factory;

        /// <summary>
        /// Gets or sets the outgoing SCP send buffer size in bytes.
        /// </summary>
        public int SendBufferSize
        {
            get => factory.getSendBufferSize();
            set => factory.setSendBufferSize(value);
        }

        /// <summary>
        /// Gets or sets the incoming SCP receive buffer size in bytes.
        /// </summary>
        public int ReceiveBufferSize
        {
            get => factory.getReceiveBufferSize();
            set => factory.setReceiveBufferSize(value);
        }

        /// <summary>
        /// Registers an SCP transfer event listener.
        /// </summary>
        /// <param name="eventListener">The listener to add.</param>
        /// <returns><c>true</c> when the listener was added; <c>false</c> if it was already registered.</returns>
        public bool addEventListener(IAMNetScpTransferEventListener eventListener)
        {
            ArgumentNullException.ThrowIfNull(eventListener);
            if (eventListeners.ContainsKey(eventListener))
            {
                return false;
            }

            var internalListener = new InternalScpTransferEventListener(eventListener);
            eventListeners[eventListener] = internalListener;

            return factory.addEventListener(internalListener);
        }

        /// <summary>
        /// Removes a previously registered SCP transfer event listener.
        /// </summary>
        /// <param name="eventListener">The listener to remove.</param>
        /// <returns><c>true</c> when the listener was removed; otherwise <c>false</c>.</returns>
        public bool removeEventListener(IAMNetScpTransferEventListener eventListener)
        {
            ArgumentNullException.ThrowIfNull(eventListener);
            if (!eventListeners.TryGetValue(eventListener, out var internalListener))
            {
                return false;
            }

            eventListeners.Remove(eventListener);
            return factory.removeEventListener(internalListener);
        }

        /// <summary>
        /// Sets the SCP file policy hook used for path resolution, filtering, and stream events.
        /// </summary>
        /// <param name="fileOpener">The SCP file policy hook.</param>
        public void setFileOpener(IAMNetScpFileOpener fileOpener)
        {
            ArgumentNullException.ThrowIfNull(fileOpener);
            factory.setScpFileOpener(new InternalScpFileOpener(fileOpener));
        }
    }
}
