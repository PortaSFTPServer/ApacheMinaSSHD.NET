using java.nio.file;
using org.apache.sshd.server.keyprovider;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    /// <summary>
    /// Configures a generated host key provider for the SSH server identity.
    /// </summary>
    public class AMNetSimpleGeneratorHostKeyProvider
    {
        /// <summary>
        /// Creates a generated host key provider.
        /// </summary>
        /// <param name="keyPath">Optional path where the generated host key is stored and reused.</param>
        public AMNetSimpleGeneratorHostKeyProvider(string keyPath = "")
        {
            KeyPath = ValidateKeyPath(keyPath);
            Algorithm = "RSA";
            KeySize = 3072;
        }

        /// <summary>
        /// Gets the optional path where the generated host key is stored and reused.
        /// </summary>
        public string KeyPath { get; }

        /// <summary>
        /// Gets the resolved full path of the host key file, or empty if no path is set.
        /// This is the canonical path after removing any directory traversal sequences.
        /// </summary>
        public string ResolvedKeyPath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the host key generation algorithm.
        /// </summary>
        public string Algorithm { get; private set; }

        /// <summary>
        /// Gets the host key size in bits.
        /// </summary>
        public int KeySize { get; private set; }

        /// <summary>
        /// Gets whether strict host key file permission checks are enabled.
        /// </summary>
        public bool StrictFilePermissions { get; private set; } = true;

        /// <summary>
        /// Sets the host key generation algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm name, such as <see cref="AMNetSshAlgorithms.HostKeyAlgorithms.Rsa"/>.</param>
        public void setAlgorithm(string algorithm)
        {
            Algorithm = string.IsNullOrWhiteSpace(algorithm)
                ? throw new ArgumentException("Algorithm is required.", nameof(algorithm))
                : algorithm;
        }

        /// <summary>
        /// Gets the host key generation algorithm.
        /// </summary>
        public string getAlgorithm() => Algorithm;

        /// <summary>
        /// Sets the host key size in bits.
        /// </summary>
        /// <param name="keySize">The key size in bits.</param>
        public void setKeySize(int keySize)
        {
            if (keySize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(keySize), "Key size must be greater than zero.");
            }

            KeySize = keySize;
        }

        /// <summary>
        /// Gets the host key size in bits.
        /// </summary>
        public int getKeySize() => KeySize;

        /// <summary>
        /// Enables or disables strict host key file permission checks.
        /// </summary>
        /// <param name="strictFilePermissions">Whether strict file permission checks are enabled.</param>
        public void setStrictFilePermissions(bool strictFilePermissions)
        {
            StrictFilePermissions = strictFilePermissions;
        }

        /// <summary>
        /// Returns whether strict host key file permission checks are enabled.
        /// </summary>
        public bool hasStrictFilePermissions() => StrictFilePermissions;

        /// <summary>
        /// Validates that the key file path does not contain directory traversal sequences.
        /// Returns the resolved canonical path.
        /// </summary>
        private static string ValidateKeyPath(string keyPath)
        {
            if (string.IsNullOrWhiteSpace(keyPath))
            {
                return keyPath;
            }

            // Check for directory traversal BEFORE resolving — Path.GetFullPath on
            // Windows resolves ".." away, making post-resolution detection unreliable.
            string normalized = keyPath.Replace('\\', '/');
            string[] parts = normalized.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "..")
                {
                    throw new ArgumentException(
                        "Host key file path contains directory traversal sequences ('..'). " +
                        "Use a direct path to prevent symlink or traversal attacks.",
                        nameof(keyPath));
                }
            }

            try
            {
                System.IO.Path.GetFullPath(keyPath);
                return keyPath;
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(
                    "Host key file path is invalid or contains unsupported characters.",
                    nameof(keyPath), ex);
            }
        }

        /// <summary>
        /// Resolves and stores the canonical key path without creating the Java provider.
        /// </summary>
        public void ResolveKeyPath()
        {
            if (!string.IsNullOrWhiteSpace(KeyPath))
            {
                ResolvedKeyPath = System.IO.Path.GetFullPath(KeyPath);
            }
        }

        internal SimpleGeneratorHostKeyProvider ToJavaKeyPairProvider()
        {
            var provider = new SimpleGeneratorHostKeyProvider();
            ResolveKeyPath();
            if (!string.IsNullOrWhiteSpace(ResolvedKeyPath))
            {
                provider.setPath(Paths.get(ResolvedKeyPath));
            }

            provider.setAlgorithm(Algorithm);
            provider.setKeySize(KeySize);
            provider.setStrictFilePermissions(StrictFilePermissions);

            return provider;
        }
    }
}
