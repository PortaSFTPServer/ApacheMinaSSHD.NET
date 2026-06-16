// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
        /// Gets the optional passphrase used to protect the host key file.
        /// </summary>
        public string? Password { get; private set; }

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
        /// <returns>The algorithm name (e.g. <c>RSA</c>, <c>ECDSA</c>, <c>Ed25519</c>).</returns>
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
        /// <returns>The key size in bits.</returns>
        public int getKeySize() => KeySize;

        /// <summary>
        /// Sets an optional passphrase used to protect the host key file.
        /// </summary>
        /// <param name="password">The passphrase, or <c>null</c> to disable password protection.</param>
        public void setPassword(string? password)
        {
            Password = password;
        }

        /// <summary>
        /// Gets the optional passphrase used to protect the host key file.
        /// </summary>
        /// <returns>The passphrase, or <c>null</c> if no password is set.</returns>
        public string? getPassword() => Password;

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
        /// <returns><c>true</c> if strict permission checks are enabled; otherwise <c>false</c>.</returns>
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

            // Note: Password/passphrase protection for host keys is not supported
            // by the upstream SimpleGeneratorHostKeyProvider in this SSHD version.
            // If provided, the password is stored for future compatibility when/whether
            // the upstream adds this capability.

            return provider;
        }
    }
}
