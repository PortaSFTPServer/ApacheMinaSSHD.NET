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

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Helpers;
using java.nio.file;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
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
        private string? _fallbackPassword;

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
        /// Gets the password provider used to resolve the host key passphrase dynamically.
        /// </summary>
        public IAMNetFilePasswordProvider? PasswordProvider { get; private set; }

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
        /// Sets a dynamic password provider for the host key file.
        /// <para>
        /// When set, <see cref="getPassword"/> and <see cref="setPassword"/> are ignored.
        /// The provider is called during key loading to obtain the decryption passphrase.
        /// </para>
        /// </summary>
        /// <param name="provider">The password provider, or <c>null</c> to clear.</param>
        public void setPasswordProvider(IAMNetFilePasswordProvider? provider)
        {
            PasswordProvider = provider;
        }

        /// <summary>
        /// Gets the dynamic password provider, if set.
        /// </summary>
        public IAMNetFilePasswordProvider? getPasswordProvider() => PasswordProvider;

        /// <summary>
        /// Sets a dynamic password provider for the host key file.
        /// </summary>
        public void SetPasswordProvider(IAMNetFilePasswordProvider? provider)
            => setPasswordProvider(provider);

        /// <summary>
        /// Sets a fallback passphrase to try if the primary <see cref="Password"/> fails
        /// (e.g., when the database-provided passphrase doesn't match the key's encryption).
        /// </summary>
        /// <param name="password">The fallback passphrase, or <c>null</c> to disable fallback.</param>
        public void setFallbackPassword(string? password)
        {
            _fallbackPassword = password;
        }

        /// <summary>
        /// Gets the fallback passphrase, if any.
        /// </summary>
        /// <returns>The fallback passphrase, or <c>null</c> if none set.</returns>
        public string? getFallbackPassword() => _fallbackPassword;

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

            string? primaryPassword = PasswordProvider != null
                ? PasswordProvider.GetPassword(ResolvedKeyPath, 0)
                : Password;

            if (!string.IsNullOrWhiteSpace(ResolvedKeyPath))
            {
                string? decryptedPath = null;

                if (System.IO.File.Exists(ResolvedKeyPath))
                {
                    if (!string.IsNullOrEmpty(primaryPassword))
                    {
                        decryptedPath = DecryptWithBouncyCastle(ResolvedKeyPath, primaryPassword);
                    }

                    if (decryptedPath == null && DetectPuttyFormat(ResolvedKeyPath))
                    {
                        if (!string.IsNullOrEmpty(primaryPassword))
                        {
                            string? puttyPem = PuttyKeyConverter.TryConvertToPem(ResolvedKeyPath, primaryPassword);
                            if (puttyPem != null)
                            {
                                decryptedPath = DecryptWithBouncyCastle(puttyPem, primaryPassword);
                                if (decryptedPath == null)
                                    decryptedPath = puttyPem;
                            }
                        }

                        if (decryptedPath == null && !string.IsNullOrEmpty(_fallbackPassword))
                        {
                            System.Console.Error.WriteLine(
                                $"[AMNetSimpleGeneratorHostKeyProvider] Primary password failed for PuTTY key, trying fallback...");
                            string? puttyPem = PuttyKeyConverter.TryConvertToPem(ResolvedKeyPath, _fallbackPassword);
                            if (puttyPem != null)
                            {
                                decryptedPath = DecryptWithBouncyCastle(puttyPem, _fallbackPassword);
                                if (decryptedPath == null)
                                    decryptedPath = puttyPem;
                            }
                        }

                        if (decryptedPath == null)
                        {
                            string diversionPath = System.IO.Path.Combine(
                                System.IO.Path.GetTempPath(),
                                "Porta_SSHD_generated_key_" + Guid.NewGuid() + ".openssh");
                            System.Console.Error.WriteLine(
                                $"[AMNetSimpleGeneratorHostKeyProvider] WARNING: PuTTY key could not be decrypted. " +
                                $"SSHD will generate a new key at '{diversionPath}'.");
                            decryptedPath = diversionPath;
                        }
                    }

                    if (decryptedPath != null)
                    {
                        ResolvedKeyPath = decryptedPath;
                    }
                }
                else if (!string.IsNullOrEmpty(primaryPassword))
                {
                    // Generate a new encrypted key
                    GenerateEncryptedKey(ResolvedKeyPath, primaryPassword);
                    // Decrypt to temp for the Java provider which has no setPassword access
                    string? tempPath = DecryptWithBouncyCastle(ResolvedKeyPath, primaryPassword);
                    if (tempPath != null)
                        ResolvedKeyPath = tempPath;
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

        private static string? DecryptWithBouncyCastle(string keyPath, string password)
        {
            try
            {
                if (!System.IO.File.Exists(keyPath))
                    return null;

                // First try reading as unencrypted PEM
                bool isEncrypted = false;
                try
                {
                    using var reader = new System.IO.StreamReader(keyPath);
                    var pemReader = new PemReader(reader);
                    var obj = pemReader.ReadObject();
                    if (obj != null)
                        return keyPath; // Unencrypted, use directly
                }
                catch
                {
                    isEncrypted = true;
                }

                if (!isEncrypted)
                    return keyPath;

                // Read with password
                var passwordFinder = new FixedPasswordFinder(password);
                using var encryptedReader = new System.IO.StreamReader(keyPath);
                var encryptedPemReader = new PemReader(encryptedReader, passwordFinder);
                var keyObj = encryptedPemReader.ReadObject();

                AsymmetricKeyParameter? privateKey = null;
                if (keyObj is AsymmetricCipherKeyPair akp)
                    privateKey = akp.Private;
                else if (keyObj is AsymmetricKeyParameter akp2)
                    privateKey = akp2;

                if (privateKey == null)
                    return null;

                // Write to temp unencrypted PEM
                string tempFile = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    TempFilePrefix + Guid.NewGuid() + ".pem");

                using (var writer = new System.IO.StreamWriter(tempFile))
                {
                    var pemWriter = new PemWriter(writer);
                    pemWriter.WriteObject(privateKey);
                }

                RegisterTempFileCleanup(tempFile);
                return tempFile;
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine(
                    $"[AMNetSimpleGeneratorHostKeyProvider] Failed to decrypt key '{keyPath}': {ex.Message}");
                return null;
            }
        }

        private static void GenerateEncryptedKey(string keyPath, string password)
        {
            string algorithm = "RSA";
            int keySize = 3072;

            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
            var keyPair = keyGen.GenerateKeyPair();

            using var writer = new System.IO.StreamWriter(keyPath);
            var pemWriter = new PemWriter(writer);
            pemWriter.WriteObject(keyPair.Private, "AES-256-CBC", password.ToCharArray(), new SecureRandom());
        }

        private static bool DetectPuttyFormat(string path)
        {
            try
            {
                using var reader = new System.IO.StreamReader(path);
                string? firstLine = reader.ReadLine();
                return firstLine != null && firstLine.StartsWith("PuTTY-User-Key-File-", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private sealed class FixedPasswordFinder : IPasswordFinder
        {
            private readonly char[] _password;
            public FixedPasswordFinder(string password) => _password = password.ToCharArray();
            public char[] GetPassword() => _password;
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
