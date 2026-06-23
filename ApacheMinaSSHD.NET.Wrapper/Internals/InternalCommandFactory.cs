using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using org.apache.sshd.server.channel;
using org.apache.sshd.server.command;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalCommandFactory : java.lang.Object, CommandFactory
    {
        private readonly IAMNetCommandHandler _handler;

        public InternalCommandFactory(IAMNetCommandHandler handler)
        {
            _handler = handler;
        }

        public Command createCommand(ChannelSession channelSession, string command)
        {
            return new InternalCommand(command, _handler);
        }
    }
}
