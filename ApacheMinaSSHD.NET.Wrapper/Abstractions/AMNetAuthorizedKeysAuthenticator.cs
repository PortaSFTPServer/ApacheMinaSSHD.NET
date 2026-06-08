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
        /// Gets the optional base path used to validate the authorized_keys file path.
        /// When set, the resolved path must be within this directory.
        /// </summary>
        public string? AllowedBasePath { get; }

        /// <summary>
        /// Creates an authorized_keys authenticator configuration.
        /// </summary>
        /// <param name="path">The authorized_keys file path used to validate public keys.</param>
        /// <param name="allowedBasePath">Optional base path. When set, the resolved path must be within this directory to prevent path traversal.</param>
        public AMNetAuthorizedKeysAuthenticator(string path, string? allowedBasePath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The authorized_keys file path cannot be empty.", nameof(path));
            }

            KeysFilePath = Path.GetFullPath(path);

            if (!string.IsNullOrWhiteSpace(allowedBasePath))
            {
                AllowedBasePath = Path.GetFullPath(allowedBasePath);
                ValidatePathWithinBase(KeysFilePath, AllowedBasePath);
            }
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

        /// <summary>
        /// Validates that <paramref name="resolvedPath"/> is within <paramref name="allowedBase"/>.
        /// Throws if the path escapes the allowed base directory.
        /// </summary>
        private static void ValidatePathWithinBase(string resolvedPath, string allowedBase)
        {
            string normalizedPath = resolvedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string normalizedBase = allowedBase.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (!normalizedPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"The path '{resolvedPath}' is outside the allowed base directory '{allowedBase}'.");
            }

            if (normalizedPath.Length > normalizedBase.Length)
            {
                char next = normalizedPath[normalizedBase.Length];
                if (next != Path.DirectorySeparatorChar && next != Path.AltDirectorySeparatorChar)
                {
                    throw new ArgumentException(
                        $"The path '{resolvedPath}' is outside the allowed base directory '{allowedBase}'.");
                }
            }
        }
    }
}
