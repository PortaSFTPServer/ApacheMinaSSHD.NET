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
        /// <remarks>
        /// Implementations may return a relative path, but the wrapper normalizes
        /// built-in configurations to an absolute path before calling Apache MINA SSHD.
        /// </remarks>
        string KeysFilePath { get; }
    }
}
