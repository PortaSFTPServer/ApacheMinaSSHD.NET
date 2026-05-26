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
            KeyPath = keyPath;
            Algorithm = "RSA";
            KeySize = 2048;
        }

        /// <summary>
        /// Gets the optional path where the generated host key is stored and reused.
        /// </summary>
        public string KeyPath { get; }

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

        internal SimpleGeneratorHostKeyProvider ToJavaKeyPairProvider()
        {
            var provider = new SimpleGeneratorHostKeyProvider();
            if (!string.IsNullOrWhiteSpace(KeyPath))
            {
                provider.setPath(Paths.get(KeyPath));
            }

            provider.setAlgorithm(Algorithm);
            provider.setKeySize(KeySize);
            provider.setStrictFilePermissions(StrictFilePermissions);

            return provider;
        }
    }
}
