using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    /// <summary>
    /// This is the bridge for the Apache Mina SSHD PasswordAuthenticator
    /// </summary>
    /// <param name="authenticator"></param>
    internal class InternalPasswordAuthenticator : java.lang.Object, org.apache.sshd.server.auth.password.PasswordAuthenticator
    {
        
        private readonly IAMNetPasswordAuthenticator authenticator;

        public InternalPasswordAuthenticator(IAMNetPasswordAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }
        public bool authenticate(string username, string password, ServerSession session)
        {

            var wrappedSession = new SshSession(session);

            bool result = authenticator.Authenticate(username, password, wrappedSession);


            // this is for implementing the password change request
            // throw new PasswordChangeRequiredException("Password expired", $"Please change password for {username}" , "en-US");



            return result;
        }

        // handle client password change request from client side (if needed)
        public  bool handleClientPasswordChangeRequest(ServerSession session, string username, string oldPassword, string newPassword)
        {
            return false;
        }
    }
}
