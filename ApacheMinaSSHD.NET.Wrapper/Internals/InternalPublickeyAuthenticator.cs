using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.security;
using org.apache.sshd.common;
using org.apache.sshd.common.config.keys;
using org.apache.sshd.common.session;
using org.apache.sshd.server.session;


namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalPublickeyAuthenticator : java.lang.Object, org.apache.sshd.server.auth.pubkey.PublickeyAuthenticator
    {

        // enforce the same naming convention
        private readonly IAMNetPublickeyAuthenticator _publickeyAuthenticator;

        public InternalPublickeyAuthenticator(IAMNetPublickeyAuthenticator PublickeyAuthenticator)
        {

            _publickeyAuthenticator = PublickeyAuthenticator;
        }

        /// <summary>
        /// This is the mapping for the Public key Authenticaton using the PublickeyAuthenticator
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pk"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool authenticate(string username, PublicKey pk, ServerSession session)
        {
            var wrappedSession = new SshSession(session);

            string incomingFingerprint = KeyUtils.getFingerPrint(pk);

            var result = _publickeyAuthenticator.Authenticate(username, incomingFingerprint, wrappedSession);

            //PropertyResolverUtils.updateProperty(session, "auth-methods", "publickey,keyboard-interactive");

            return result;
        }

    }

}
