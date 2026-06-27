using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions;

public interface IAMNetChannelListener
{
    void OnChannelInitialized(ISshSession session);
    void OnChannelOpenSuccess(ISshSession session);
    void OnChannelOpenFailure(ISshSession session, Exception? reason);
    void OnChannelStateChanged(ISshSession session, string? hint);
    void OnChannelClosed(ISshSession session, Exception? reason);
}
