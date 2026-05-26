namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Authorized_keys authenticator configuration.
    /// </summary>
    /// <remarks>
    /// This authenticator delegates OpenSSH-style authorized_keys parsing to Apache
    /// MINA SSHD while keeping Java types out of the public .NET API.
    /// </remarks>
    public class AMNetAuthorizedKeysAuthenticator : IAMNetAuthorizedKeysAuthenticator
    {
        /// <inheritdoc />
        public string KeysFilePath { get; }

        /// <summary>
        /// Creates an authorized_keys authenticator configuration.
        /// </summary>
        /// <param name="path">The authorized_keys file path used to validate public keys.</param>
        public AMNetAuthorizedKeysAuthenticator(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The authorized_keys file path cannot be empty.", nameof(path));
            }

            KeysFilePath = Path.GetFullPath(path);
        }

        /// <summary>
        /// Creates an authorized_keys authenticator configuration from a file path.
        /// </summary>
        /// <param name="path">The authorized_keys file path used to validate public keys.</param>
        /// <returns>An authorized_keys authenticator configuration.</returns>
        public static AMNetAuthorizedKeysAuthenticator FromFile(string path)
        {
            return new AMNetAuthorizedKeysAuthenticator(path);
        }
    }
}
