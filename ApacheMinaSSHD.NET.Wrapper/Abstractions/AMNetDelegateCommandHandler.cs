using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// A command handler that delegates exec and shell requests to supplied callbacks.
    /// </summary>
    public sealed class AMNetDelegateCommandHandler : IAMNetCommandHandler
    {
        private readonly Func<string, ISshSession, int>? _execHandler;
        private readonly Func<ISshSession, int>? _shellHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AMNetDelegateCommandHandler"/> class.
        /// </summary>
        /// <param name="execHandler">Callback invoked for exec requests, or <c>null</c> to reject.</param>
        /// <param name="shellHandler">Callback invoked for shell requests, or <c>null</c> to reject.</param>
        public AMNetDelegateCommandHandler(
            Func<string, ISshSession, int>? execHandler = null,
            Func<ISshSession, int>? shellHandler = null)
        {
            _execHandler = execHandler;
            _shellHandler = shellHandler;
        }

        /// <inheritdoc/>
        public int ExecuteCommand(string command, ISshSession session)
        {
            if (_execHandler != null)
                return _execHandler(command, session);
            return 1;
        }

        /// <inheritdoc/>
        public int ExecuteShell(ISshSession session)
        {
            if (_shellHandler != null)
                return _shellHandler(session);
            return 1;
        }
    }
}
