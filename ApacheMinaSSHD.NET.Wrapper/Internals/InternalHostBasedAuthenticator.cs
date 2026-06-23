using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.security;
using java.security.cert;
using java.util;
using org.apache.sshd.common.config.keys;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalHostBasedAuthenticator : java.lang.Object, org.apache.sshd.server.auth.hostbased.HostBasedAuthenticator
    {
        private readonly IAMNetHostBasedAuthenticator authenticator;

        public InternalHostBasedAuthenticator(IAMNetHostBasedAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public bool authenticate(ServerSession session, string username, PublicKey key, string clientHostname, string clientUsername, List certificates)
        {
            var wrappedSession = new SshSession(session);
            string fingerprint = KeyUtils.getFingerPrint(key);
            return authenticator.Authenticate(username, fingerprint, clientHostname, clientUsername, wrappedSession);
        }
    }
}
