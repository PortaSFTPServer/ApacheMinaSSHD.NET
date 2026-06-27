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

            var certList = new System.Collections.Generic.List<byte[]>();
            if (certificates != null)
            {
                var iter = certificates.iterator();
                while (iter.hasNext())
                {
                    var cert = (X509Certificate)iter.next();
                    try { certList.Add(cert.getEncoded()); } catch { }
                }
            }

            string? keyAlgorithm = null;
            byte[]? keyData = null;
            try
            {
                keyAlgorithm = key.getAlgorithm();
                keyData = key.getEncoded();
            }
            catch { }

            wrappedSession.SetHostKeyInfo(keyAlgorithm, keyData, certList, clientUsername);

            return authenticator.Authenticate(username, fingerprint, clientHostname, clientUsername, wrappedSession);
        }
    }
}