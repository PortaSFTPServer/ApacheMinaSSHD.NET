namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Supplies an authorized_keys file path for public key authentication.
    /// </summary>
    public interface IAMNetAuthorizedKeysAuthenticator
    {
        /// <summary>
        /// Gets the authorized_keys file path used to validate client public keys.
        /// </summary>
        string KeysFilePath { get; }
    }
}
