using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetGssapiAuthenticator
    {
        bool ValidateIdentity(ISshSession session, string identity);
    }
}
