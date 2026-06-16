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

using java.io;
using java.nio.file;
using java.security;
using org.apache.sshd.common.config.keys;
using org.apache.sshd.common.config.keys.writer.openssh;
using org.apache.sshd.common.util.io.resource;
using org.apache.sshd.common.util.security;
using org.apache.sshd.server.keyprovider;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    /// <summary>
    /// Configures a generated host key provider for the SSH server identity.
    /// </summary>
    public class AMNetSimpleGeneratorHostKeyProvider
    {
        private static readonly string TempFilePrefix = "amnet-sshd-hostkey-";
        private static readonly System.Collections.Concurrent.ConcurrentBag<string> _tempFiles = new();
        private static bool _cleanupRegistered;

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
            ResolveKeyPath();

            if (!string.IsNullOrWhiteSpace(ResolvedKeyPath)
                && !string.IsNullOrEmpty(Password)
                && System.IO.File.Exists(ResolvedKeyPath))
            {
                string? decryptedPath = TryDecryptHostKey(ResolvedKeyPath, Password);
                if (decryptedPath != null)
                {
                    ResolvedKeyPath = decryptedPath;
                }
            }

            var provider = new SimpleGeneratorHostKeyProvider();
            if (!string.IsNullOrWhiteSpace(ResolvedKeyPath))
            {
                provider.setPath(Paths.get(ResolvedKeyPath));
            }

            provider.setAlgorithm(Algorithm);
            provider.setKeySize(KeySize);
            provider.setStrictFilePermissions(StrictFilePermissions);

            return provider;
        }

        private static string? TryDecryptHostKey(string keyPath, string password)
        {
            try
            {
                var path = Paths.get(keyPath);
                var resourceKey = new PathResource(path);
                var passwordProvider = FilePasswordProvider.of(password);

                java.io.InputStream? inputStream = null;
                java.io.OutputStream? os = null;
                try
                {
                    inputStream = java.nio.file.Files.newInputStream(path);
                    var keyPairs = SecurityUtils.loadKeyPairIdentities(
                        null, resourceKey, inputStream, passwordProvider);

                    if (keyPairs == null || !keyPairs.iterator().hasNext())
                    {
                        return null;
                    }

                    var kp = (KeyPair)keyPairs.iterator().next();
                    string tempFile = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(),
                        TempFilePrefix + Guid.NewGuid() + ".openssh");

                    var tempPath = Paths.get(tempFile);
                    os = java.nio.file.Files.newOutputStream(tempPath);
                    var writer = new OpenSSHKeyPairResourceWriter();
                    writer.writePrivateKey(kp, "host key", null, os);

                    RegisterTempFileCleanup(tempFile);
                    return tempFile;
                }
                finally
                {
                    inputStream?.close();
                    os?.close();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AMNetSimpleGeneratorHostKeyProvider] Failed to decrypt host key '{keyPath}': {ex.Message}");
                return null;
            }
        }

        private static void RegisterTempFileCleanup(string path)
        {
            _tempFiles.Add(path);

            if (!_cleanupRegistered)
            {
                _cleanupRegistered = true;
                AppDomain.CurrentDomain.ProcessExit += (_, _) => CleanupTempFiles();
            }
        }

        private static void CleanupTempFiles()
        {
            foreach (string file in _tempFiles)
            {
                try
                {
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
