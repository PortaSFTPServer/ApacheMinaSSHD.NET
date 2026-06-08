using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Directory-backed public key authenticator that compares incoming key fingerprints
    /// against key files in an Authorized_Keys directory.
    /// </summary>
    /// <remarks>
    /// Supports RSA, ECDSA (NIST P-256, P-384, P-521), and Ed25519 keys in PEM,
    /// OpenSSH public key, and SSH2 public key formats.
    /// For new applications, prefer <see cref="AMNetAuthorizedKeysAuthenticator"/>
    /// for OpenSSH authorized_keys files or <see cref="AMNetFingerprintPublickeyAuthenticator"/>
    /// when fingerprints are stored in an application database.
    /// </remarks>
    public class AMNetPublickeyAuthenticator : IAMNetPublickeyAuthenticator
    {
        private readonly string authKeysPath;
        private const string AuthorizedKeysDirectory = "Authorized_Keys";

        /// <summary>
        /// Creates a public key authenticator.
        /// </summary>
        /// <param name="authKeysPath">
        /// Base path that contains the Authorized_Keys directory. When empty, a default
        /// application data path is used.
        /// </param>
        public AMNetPublickeyAuthenticator(string authKeysPath = "")
        {
            string basePath = string.IsNullOrWhiteSpace(authKeysPath)
                ? GetDefaultAuthorizedKeysBasePath()
                : authKeysPath;

            this.authKeysPath = System.IO.Path.Combine(basePath, AuthorizedKeysDirectory);
        }

        /// <inheritdoc />
        public virtual bool Authenticate(string username, string incomingFingerprint, ISshSession session)
        {
            return AuthenticateValidUserKeyFingerprint(username, incomingFingerprint);
        }

        private bool AuthenticateValidUserKeyFingerprint(string username, string incomingFingerprint)
        {
            if (!Directory.Exists(authKeysPath))
            {
                return false;
            }

            string escapedUsername = username
                .Replace("*", "[*]")
                .Replace("?", "[?]")
                .Replace("[", "[[]");
            var authKeys = Directory.GetFiles(authKeysPath, $"*{escapedUsername}*", SearchOption.TopDirectoryOnly);

            foreach (var keyPath in authKeys)
            {
                var authKeyFingerprint = GetSecureFingerprint(keyPath);

                if (ConstantTimeEquals(incomingFingerprint, authKeyFingerprint))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            byte[] ab = Encoding.UTF8.GetBytes(a);
            byte[] bb = Encoding.UTF8.GetBytes(b);
            bool result = CryptographicOperations.FixedTimeEquals(ab, bb);
            CryptographicOperations.ZeroMemory(ab);
            CryptographicOperations.ZeroMemory(bb);
            return result;
        }

        /// <summary>
        /// Computes the SHA-256 fingerprint of a public key file that matches
        /// Apache MINA SSHD's KeyUtils.getFingerPrint format.
        /// </summary>
        private static string GetSecureFingerprint(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Path is empty", nameof(filePath));
            }

            string content = File.ReadAllText(filePath).Trim();
            byte[] sshEncoded = EncodeToSshWireFormat(content);
            byte[] hash = SHA256.HashData(sshEncoded);
            return "SHA256:" + Convert.ToBase64String(hash).Replace("=", "");
        }

        /// <summary>
        /// Encodes a public key file to SSH wire format bytes for fingerprinting.
        /// Supports PEM, OpenSSH public key, OpenSSH private key, and SSH2 public key formats.
        /// </summary>
        private static byte[] EncodeToSshWireFormat(string content)
        {
            // SSH2 public key format (---- BEGIN SSH2 PUBLIC KEY ----)
            if (content.Contains("BEGIN SSH2"))
            {
                var lines = content.Split('\n', '\r')
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0 && !l.StartsWith("----") && !l.Contains(":"));
                return Convert.FromBase64String(string.Concat(lines));
            }

            // OpenSSH public key format: "keytype base64data [comment]" — fast path
            string[] parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2 && IsSshKeyType(parts[0]))
            {
                return Convert.FromBase64String(parts[1]);
            }

            // OpenSSH private key format (-----BEGIN OPENSSH PRIVATE KEY-----).
            // Extracts the embedded SSH wire-format public key without needing
            // BouncyCastle PemReader or a password (the public key is stored
            // unencrypted before the private key section).
            if (content.Contains("BEGIN OPENSSH PRIVATE KEY"))
            {
                return DecodeOpenSshPrivateKey(content);
            }

            // PEM format via BouncyCastle PemReader (-----BEGIN ... KEY-----).
            // Handles PUBLIC KEY, PRIVATE KEY, RSA PRIVATE KEY, EC PRIVATE KEY,
            // DSA PRIVATE KEY. ENCRYPTED PRIVATE KEY is rejected below.
            if (IsBouncyCastlePem(content))
            {
                return DecodePemToSshWireFormat(content);
            }

            throw new CryptographicException("Unrecognized public key format. Supported formats: PEM, OpenSSH public key, OpenSSH private key, SSH2 public key.");
        }

        private static bool IsBouncyCastlePem(string content)
        {
            return content.Contains("-----BEGIN ")
                && content.Contains("-----")
                && !content.Contains("ENCRYPTED PRIVATE KEY")
                && !content.Contains("OPENSSH PRIVATE KEY")
                && !content.Contains("BEGIN SSH2");
        }

        private static byte[] DecodePemToSshWireFormat(string content)
        {
            using var reader = new StringReader(content);
            var pemReader = new PemReader(reader);

            object? keyObj;
            try
            {
                keyObj = pemReader.ReadObject();
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException)
            {
                throw new CryptographicException("Failed to read PEM content.", ex);
            }

            if (keyObj is null)
            {
                throw new CryptographicException("PEM content did not contain a recognizable key.");
            }

            AsymmetricKeyParameter pubKey = ExtractPublicKey(keyObj);
            return EncodePublicKeyToSsh(pubKey);
        }

        private static byte[] DecodeOpenSshPrivateKey(string content)
        {
            // Extract base64 lines between the BEGIN/END markers.
            var lines = content.Split('\n', '\r')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("-----"))
                .ToArray();

            byte[] decoded = Convert.FromBase64String(string.Concat(lines));

            // OpenSSH private key wire format (PROTOCOL.key):
            //   AUTH_MAGIC       = "openssh-key-v1\0"  (15 bytes)
            //   ciphername       = string
            //   kdfname          = string
            //   kdfoptions       = string
            //   number_of_keys   = uint32
            //   public_key       = string  (SSH wire format — what we want)
            //   encrypted_...    = ...     (skipped)
            using var ms = new MemoryStream(decoded);
            byte[] magicBytes = ReadBytes(ms, 15);
            string magic = Encoding.ASCII.GetString(magicBytes);
            if (magic != "openssh-key-v1\0")
            {
                throw new CryptographicException("Invalid OpenSSH private key header.");
            }

            ReadSshString(ms); // ciphername
            ReadSshString(ms); // kdfname
            ReadSshBytes(ms);  // kdfoptions
            ReadUint32(ms);    // number_of_keys

            byte[] publicKey = ReadSshBytes(ms);
            return publicKey;
        }

        private static bool IsSshKeyType(string value)
        {
            return value switch
            {
                "ssh-rsa" or "ssh-dss" or "ssh-ed25519"
                    or "ecdsa-sha2-nistp256" or "ecdsa-sha2-nistp384" or "ecdsa-sha2-nistp521"
                    or "sk-ssh-ed25519@openssh.com" or "sk-ecdsa-sha2-nistp256@openssh.com"
                    or "ssh-rsa-cert-v01@openssh.com" or "ssh-dss-cert-v01@openssh.com"
                    or "ssh-ed25519-cert-v01@openssh.com"
                    or "ecdsa-sha2-nistp256-cert-v01@openssh.com"
                    or "ecdsa-sha2-nistp384-cert-v01@openssh.com"
                    or "ecdsa-sha2-nistp521-cert-v01@openssh.com"
                    => true,
                _ => false
            };
        }

        private static AsymmetricKeyParameter ExtractPublicKey(object pemObject)
        {
            return pemObject switch
            {
                AsymmetricCipherKeyPair pair => pair.Public,
                AsymmetricKeyParameter key => key,
                _ => throw new NotSupportedException($"Unrecognized PEM object: {pemObject.GetType().Name}. Expected a public key or a key pair.")
            };
        }

        private static byte[] EncodePublicKeyToSsh(AsymmetricKeyParameter key)
        {
            return key switch
            {
                RsaKeyParameters rsa => EncodeRsaSsh(rsa),
                ECPublicKeyParameters ec => EncodeEcdsaSsh(ec),
                Ed25519PublicKeyParameters ed => EncodeEd25519Ssh(ed),
                _ => throw new NotSupportedException($"Key type {key.GetType().Name} is not supported. Supported types: RSA, ECDSA, Ed25519.")
            };
        }

        private static byte[] EncodeRsaSsh(RsaKeyParameters rsa)
        {
            using var ms = new MemoryStream();
            WriteSshString(ms, "ssh-rsa");
            WriteSshMpint(ms, rsa.Exponent.ToByteArrayUnsigned());
            WriteSshMpint(ms, rsa.Modulus.ToByteArrayUnsigned());
            return ms.ToArray();
        }

        private static byte[] EncodeEcdsaSsh(ECPublicKeyParameters ec)
        {
            string curveName = GetEcdsaCurveName(ec);
            string algorithm = "ecdsa-sha2-" + curveName;

            using var ms = new MemoryStream();
            WriteSshString(ms, algorithm);
            WriteSshString(ms, curveName);
            byte[] encodedPoint = ec.Q.GetEncoded(false);
            WriteSshString(ms, encodedPoint);
            return ms.ToArray();
        }

        private static byte[] EncodeEd25519Ssh(Ed25519PublicKeyParameters ed)
        {
            using var ms = new MemoryStream();
            WriteSshString(ms, "ssh-ed25519");
            WriteSshString(ms, ed.GetEncoded());
            return ms.ToArray();
        }

        private static string GetEcdsaCurveName(ECPublicKeyParameters ec)
        {
            int fieldSize = ec.Parameters.Curve.FieldSize;
            return fieldSize switch
            {
                <= 256 => "nistp256",
                <= 384 => "nistp384",
                _ => "nistp521"
            };
        }

        private static void WriteSshString(MemoryStream ms, string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            WriteUint32(ms, bytes.Length);
            ms.Write(bytes);
        }

        private static void WriteSshString(MemoryStream ms, byte[] value)
        {
            WriteUint32(ms, value.Length);
            ms.Write(value);
        }

        private static void WriteSshMpint(MemoryStream ms, byte[] value)
        {
            if (value.Length > 0 && (value[0] & 0x80) != 0)
            {
                WriteUint32(ms, value.Length + 1);
                ms.WriteByte(0);
                ms.Write(value);
            }
            else
            {
                WriteUint32(ms, value.Length);
                ms.Write(value);
            }
        }

        private static void WriteUint32(MemoryStream ms, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            ms.Write(bytes);
        }

        private static string ReadSshString(MemoryStream ms)
        {
            int length = ReadUint32(ms);
            return Encoding.ASCII.GetString(ReadBytes(ms, length));
        }

        private static byte[] ReadSshBytes(MemoryStream ms)
        {
            int length = ReadUint32(ms);
            return ReadBytes(ms, length);
        }

        private static int ReadUint32(MemoryStream ms)
        {
            byte[] bytes = ReadBytes(ms, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes);
        }

        private static byte[] ReadBytes(MemoryStream ms, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = ms.Read(buffer, offset, count - offset);
                if (read == 0)
                {
                    throw new EndOfStreamException("Unexpected end of SSH key data.");
                }
                offset += read;
            }
            return buffer;
        }

        private static string GetDefaultAuthorizedKeysBasePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return string.IsNullOrWhiteSpace(appDataPath)
                ? AppContext.BaseDirectory
                : System.IO.Path.Combine(appDataPath, "ApacheMinaSSHD.NET");
        }
    }
}
