using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public sealed class AMNetDelegateCommandHandler : IAMNetCommandHandler
    {
        private readonly Func<string, ISshSession, int>? _execHandler;
        private readonly Func<ISshSession, int>? _shellHandler;

        public AMNetDelegateCommandHandler(
            Func<string, ISshSession, int>? execHandler = null,
            Func<ISshSession, int>? shellHandler = null)
        {
            _execHandler = execHandler;
            _shellHandler = shellHandler;
        }

        public int ExecuteCommand(string command, ISshSession session)
        {
            if (_execHandler != null)
                return _execHandler(command, session);
            return 1;
        }

        public int ExecuteShell(ISshSession session)
        {
            if (_shellHandler != null)
                return _shellHandler(session);
            return 1;
        }
    }
}
