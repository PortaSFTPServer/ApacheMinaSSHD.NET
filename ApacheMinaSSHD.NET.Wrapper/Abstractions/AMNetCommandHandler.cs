using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public class AMNetCommandHandler : IAMNetCommandHandler
    {
        public virtual int ExecuteCommand(string command, ISshSession session)
        {
            return 1;
        }

        public virtual int ExecuteShell(ISshSession session)
        {
            return 1;
        }
    }
}
