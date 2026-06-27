using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetGssapiAuthenticator
    {
        bool ValidateIdentity(ISshSession session, string identity);

        /// <summary>Called before <see cref="ValidateIdentity"/> to pre-validate the username.</summary>
        bool ValidateInitialUser(ISshSession session, string username) => true;

        /// <summary>Kerberos service principal name (e.g., "host/server.example.com@@REALM").</summary>
        string? ServicePrincipalName => null;

        /// <summary>Path to the Kerberos keytab file for service credential validation.</summary>
        string? KeytabFile => null;
    }
}
