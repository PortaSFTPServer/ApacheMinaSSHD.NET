using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetCommandHandler
    {
        int ExecuteCommand(string command, ISshSession session);
        int ExecuteShell(ISshSession session);
    }
}
