using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
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

            var authKeys = Directory.GetFiles(authKeysPath, $"*{username}*", SearchOption.TopDirectoryOnly);

            foreach (var keyPath in authKeys)
            {
                var authKeyFingerprint = GetSecureFingerprint(keyPath);

                if (string.Equals(incomingFingerprint, authKeyFingerprint, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
        /// Supports PEM, OpenSSH public key, and SSH2 public key formats.
        /// </summary>
        private static byte[] EncodeToSshWireFormat(string content)
        {
            // PEM format (e.g., -----BEGIN PUBLIC KEY-----)
            if (content.Contains("BEGIN ") && content.Contains(" KEY"))
            {
                using var reader = new StringReader(content);
                var pemReader = new PemReader(reader);
                object keyObj = pemReader.ReadObject();
                AsymmetricKeyParameter pubKey = ExtractPublicKey(keyObj);
                return EncodePublicKeyToSsh(pubKey);
            }

            // SSH2 public key format (---- BEGIN SSH2 PUBLIC KEY ----)
            if (content.Contains("BEGIN SSH2") || content.Contains("BEGIN SSH2 PUBLIC KEY"))
            {
                var lines = content.Split('\n', '\r')
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0 && !l.StartsWith("----") && !l.Contains(":"));
                return Convert.FromBase64String(string.Concat(lines));
            }

            // OpenSSH public key format: "keytype base64data [comment]"
            string[] parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2 && IsSshKeyType(parts[0]))
            {
                return Convert.FromBase64String(parts[1]);
            }

            throw new CryptographicException("Unrecognized public key format. Supported formats: PEM, OpenSSH public key, SSH2 public key.");
        }

        private static bool IsSshKeyType(string value)
        {
            return value switch
            {
                "ssh-rsa" or "ssh-dss" or "ssh-ed25519"
                    or "ecdsa-sha2-nistp256" or "ecdsa-sha2-nistp384" or "ecdsa-sha2-nistp521"
                    or "sk-ssh-ed25519@openssh.com" or "sk-ecdsa-sha2-nistp256@openssh.com"
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

        private static string GetDefaultAuthorizedKeysBasePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return string.IsNullOrWhiteSpace(appDataPath)
                ? AppContext.BaseDirectory
                : System.IO.Path.Combine(appDataPath, "ApacheMinaSSHD.NET");
        }
    }
}
