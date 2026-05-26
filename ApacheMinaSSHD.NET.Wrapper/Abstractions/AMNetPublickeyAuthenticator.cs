using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Directory-backed public key authenticator that compares incoming key fingerprints
    /// against key files in an Authorized_Keys directory.
    /// </summary>
    /// <remarks>
    /// This authenticator exists for compatibility with the original directory
    /// pattern. For new applications, prefer <see cref="AMNetAuthorizedKeysAuthenticator"/>
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


        /// <summary>
        /// let users to have multiple keys and select the valid one
        /// </summary>
        /// <param name="username"></param>
        /// <param name="incomingFingerprint"></param>
        /// <returns></returns>
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
        /// Generates a fingerprint that matches Apache MINA SSHD's KeyUtils exactly.
        /// This method is O(1) in performance and cross-platform.
        /// </summary>
        private static string GetSecureFingerprint(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Path is empty", nameof(filePath));

            // load to memory
            string content = File.ReadAllText(filePath).Trim();
            using var rsa = RSA.Create();

            try
            {
                if (content.Contains("BEGIN PUBLIC KEY")) ImportFromPem(content);
                else rsa.ImportParameters(ParseSshBlob(GetRawBytes(content)));
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Failed to securely parse key format.", ex);
            }

            return GenerateMinaCompatibleHash(rsa);
        }


        /// <summary>
        /// Imports an RSA key from PEM text.
        /// </summary>
        /// <param name="pemText">The PEM-encoded key text.</param>
        /// <returns>An RSA instance initialized from the PEM data.</returns>
        public static RSA ImportFromPem(string pemText)
        {
            using (var reader = new StringReader(pemText))
            {
                var pemReader = new PemReader(reader);
                object keyObject = pemReader.ReadObject();

                // Convert BouncyCastle RSA parameters to standard .NET RSA parameters
                RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyObject);

                RSA rsa = RSA.Create();
                rsa.ImportParameters(rsaParams);
                return rsa;
            }
        }

        private static string GenerateMinaCompatibleHash(RSA rsa)
        {
            var p = rsa.ExportParameters(false);
            byte[] exponent = p.Exponent
                ?? throw new CryptographicException("RSA public exponent is missing.");
            byte[] modulus = p.Modulus
                ?? throw new CryptographicException("RSA public modulus is missing.");
            if (modulus.Length == 0)
            {
                throw new CryptographicException("RSA public modulus is empty.");
            }

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

             void WriteSsh(byte[] d)
            {
                var len = BitConverter.GetBytes(d.Length);
                if (BitConverter.IsLittleEndian) Array.Reverse(len);
                writer.Write(len); writer.Write(d);
            }

            WriteSsh(Encoding.ASCII.GetBytes("ssh-rsa"));
            WriteSsh(exponent);

            // Securely handle signed integers for SSH/Java compatibility
            byte[] mod = modulus;
            if ((mod[0] & 0x80) != 0)
            {
                byte[] padded = new byte[mod.Length + 1];
                Buffer.BlockCopy(mod, 0, padded, 1, mod.Length);
                mod = padded;
            }
            WriteSsh(mod);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(ms.ToArray());

                return "SHA256:" + Convert.ToBase64String(hash).Replace("=", "");

            }

            //// SHA256 is the modern secure standard for SSH fingerprints
            //byte[] hash = SHA256.HashData(ms.ToArray());
            //return "SHA256:" + Convert.ToBase64String(hash).Replace("=", "");
        }

        private static byte[] GetRawBytes(string c)
        {
            if (c.Contains("BEGIN SSH2"))
            {
                var lines = c.Split('\n', '\r').Select(l => l.Trim());
                return Convert.FromBase64String(string.Join("", lines.Where(l => !l.StartsWith("-") && !l.Contains(":"))));
            }
            return Convert.FromBase64String(c.Split(' ')[1]);
        }

        private static RSAParameters ParseSshBlob(byte[] d)
        {
            using var ms = new MemoryStream(d);
            using var r = new BinaryReader(ms);
            byte[] Next()
            {
                var lb = r.ReadBytes(4); if (BitConverter.IsLittleEndian) Array.Reverse(lb);
                return r.ReadBytes(BitConverter.ToInt32(lb, 0));
            }
            Next(); // Skip "ssh-rsa"
            byte[] e = Next(); byte[] m = Next();
            if (m[0] == 0x00) m = m.Skip(1).ToArray(); // Strip SSH padding
            return new RSAParameters { Exponent = e, Modulus = m };
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
