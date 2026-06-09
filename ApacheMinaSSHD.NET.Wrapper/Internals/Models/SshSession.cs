namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshSession : Abstractions.Models.ISshSession
    {
        private readonly org.apache.sshd.server.session.ServerSession _javaSession;

        public SshSession(org.apache.sshd.server.session.ServerSession javaSession)
        {
            _javaSession = javaSession;
        }

        public string RemoteAddress => _javaSession.getIoSession().getRemoteAddress().toString();
        public Guid SessionId { get; } = Guid.NewGuid();
    }

}
