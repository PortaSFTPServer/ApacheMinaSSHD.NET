using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Base class for handling SSH exec and shell commands. Override the virtual
    /// methods to implement custom command processing.
    /// </summary>
    public class AMNetCommandHandler : IAMNetCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AMNetCommandHandler"/> class.
        /// </summary>
        public AMNetCommandHandler()
        {
        }

        /// <summary>
        /// Executes an SSH exec request with the specified command string.
        /// </summary>
        /// <param name="command">The command string received from the client.</param>
        /// <param name="session">The SSH session that requested the command.</param>
        /// <returns>The exit code to return to the client.</returns>
        public virtual int ExecuteCommand(string command, ISshSession session)
        {
            return 1;
        }

        /// <summary>
        /// Executes an interactive shell session.
        /// </summary>
        /// <param name="session">The SSH session that requested the shell.</param>
        /// <returns>The exit code to return to the client.</returns>
        public virtual int ExecuteShell(ISshSession session)
        {
            return 1;
        }
    }
}
