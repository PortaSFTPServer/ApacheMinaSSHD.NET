using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetHostBasedAuthenticator
    {
        bool Authenticate(string username, string publicKeyFingerprint, string clientHostname, string clientUsername, ISshSession session);
    }
}
