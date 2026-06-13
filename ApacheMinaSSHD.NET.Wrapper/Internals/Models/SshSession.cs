namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshSession : Abstractions.Models.ISshSession
    {
        private readonly org.apache.sshd.server.session.ServerSession? _javaSession;

        public SshSession(org.apache.sshd.server.session.ServerSession javaSession)
        {
            _javaSession = javaSession;
        }

        public SshSession()
        {
            _javaSession = null;
        }

        public string RemoteAddress
        {
            get
            {
                try
                {
                    if (_javaSession?.getIoSession()?.getRemoteAddress() != null)
                        return _javaSession.getIoSession().getRemoteAddress().toString();
                }
                catch
                {
                }
                return "unknown";
            }
        }

        public Guid SessionId { get; } = Guid.NewGuid();
    }
}
