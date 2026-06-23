using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.server.auth.gss;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalGssapiAuthenticator : GSSAuthenticator
    {
        private readonly IAMNetGssapiAuthenticator authenticator;

        public InternalGssapiAuthenticator(IAMNetGssapiAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public override bool validateIdentity(ServerSession session, string identity)
        {
            var wrappedSession = new SshSession(session);
            return authenticator.ValidateIdentity(wrappedSession, identity);
        }
    }
}
