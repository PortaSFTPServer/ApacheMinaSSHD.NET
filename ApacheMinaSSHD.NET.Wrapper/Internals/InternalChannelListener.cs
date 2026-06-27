using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.common.channel;

namespace ApacheMinaSSHD.NET.Wrapper.Internals;

internal sealed class InternalChannelListener : java.lang.Object, ChannelListener
{
    private readonly IAMNetChannelListener _listener;

    public InternalChannelListener(IAMNetChannelListener listener)
    {
        _listener = listener;
    }

    public IAMNetChannelListener WrappedListener => _listener;

    public void channelInitialized(Channel channel)
    {
        _listener.OnChannelInitialized(ToSshSession(channel));
    }

    public void channelOpenSuccess(Channel channel)
    {
        _listener.OnChannelOpenSuccess(ToSshSession(channel));
    }

    public void channelOpenFailure(Channel channel, Exception reason)
    {
        _listener.OnChannelOpenFailure(ToSshSession(channel), reason);
    }

    public void channelStateChanged(Channel channel, string hint)
    {
        _listener.OnChannelStateChanged(ToSshSession(channel), hint);
    }

    public void channelClosed(Channel channel, Exception reason)
    {
        _listener.OnChannelClosed(ToSshSession(channel), reason);
    }

    private static ISshSession ToSshSession(Channel channel)
    {
        var session = channel.getSession();
        if (session is org.apache.sshd.server.session.ServerSession serverSession)
            return new SshSession(serverSession);
        return new SshSession();
    }
}
