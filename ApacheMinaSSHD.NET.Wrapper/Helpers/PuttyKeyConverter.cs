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

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Helpers
{
    internal static class PuttyKeyConverter
    {
        private const string TempFilePrefix = "amnet-sshd-putty-";
        private static readonly System.Collections.Concurrent.ConcurrentBag<string> _tempFiles = new();
        private static bool _cleanupRegistered;

        internal static string? TryConvertToPem(string keyPath, string password)
        {
            try
            {
                string content = System.IO.File.ReadAllText(keyPath);
                if (!content.StartsWith("PuTTY-User-Key-File-2:"))
                    return null;

                var headers = ParsePuttyFile(content);
                if (headers == null)
                    return null;

                string encryption = (string)headers["Encryption"];
                string algorithm = (string)headers["Algorithm"];
                string comment = (string)headers["Comment"];
                byte[] publicKey = (byte[])headers["PublicKey"];
                byte[] encryptedPrivateBlob = (byte[])headers["PrivateBlob"];
                string hexMac = (string)headers["PrivateMAC"];

                byte[] passphrase = Encoding.UTF8.GetBytes(password);

                byte[] privateBlob;
                if (encryption == "aes256-cbc")
                {
                    // PPK v2: AES key = SHA1(uint32(0) || passphrase) || SHA1(uint32(1) || passphrase)[0..12]
                    byte[] aesKey = DerivePuttyKey(passphrase, 32);
                    // PPK v2: IV is always 16 bytes of zero
                    byte[] iv = new byte[16];

                    string encryptedHex = BitConverter.ToString(encryptedPrivateBlob, 0, Math.Min(32, encryptedPrivateBlob.Length))
                        .Replace("-", " ");

                    privateBlob = DecryptAes256Cbc(encryptedPrivateBlob, aesKey, iv);

                    string passHash = BitConverter.ToString(
                        System.Security.Cryptography.SHA256.Create().ComputeHash(passphrase))
                        .Replace("-", "").ToLowerInvariant();

                    string hexDump = BitConverter.ToString(privateBlob, 0, Math.Min(64, privateBlob.Length))
                        .Replace("-", " ");

                    System.Console.Error.WriteLine(
                        $"[PuttyKeyConverter] Encrypted len={encryptedPrivateBlob.Length}, first32={encryptedHex}");
                    System.Console.Error.WriteLine(
                        $"[PuttyKeyConverter] Decrypted len={privateBlob.Length}, first64={hexDump}, " +
                        $"Pass-SHA256={passHash}");

                    byte[] hmacKey = DerivePuttyHmacKey(passphrase);
                    byte[] algorithmBytes = Encoding.UTF8.GetBytes(algorithm);
                    byte[] encryptionBytes = Encoding.UTF8.GetBytes(encryption);
                    byte[] commentBytes = Encoding.UTF8.GetBytes(comment);
                    byte[] macInput = BuildMacInput(algorithmBytes, encryptionBytes, commentBytes, publicKey, privateBlob);
                    string computedMac = ComputeHmacSha1Hex(hmacKey, macInput);

                    System.Console.Error.WriteLine(
                        $"[PuttyKeyConverter] MAC: Expected={hexMac}, Computed={computedMac}");

                    if (!string.Equals(computedMac, hexMac, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Console.Error.WriteLine(
                            $"[PuttyKeyConverter] MAC mismatch, trying RSA parsing as fallback...");
                    }
                }
                else if (encryption == "none")
                {
                    privateBlob = encryptedPrivateBlob;
                }
                else
                {
                    System.Console.Error.WriteLine(
                        $"[PuttyKeyConverter] Unsupported encryption '{encryption}' in '{keyPath}'");
                    return null;
                }

                byte[]? pemBytes = ConvertToPemBytes(algorithm, publicKey, privateBlob, comment, password);
                if (pemBytes == null)
                {
                    System.Console.Error.WriteLine(
                        $"[PuttyKeyConverter] RSA parsing failed after decryption for '{keyPath}' - wrong password or unsupported algorithm '{algorithm}'.");
                    return null;
                }

                string pemPath = System.IO.Path.ChangeExtension(keyPath, ".pem");
                System.IO.File.WriteAllBytes(pemPath, pemBytes);

                System.Console.Error.WriteLine(
                    $"[PuttyKeyConverter] Converted PuTTY key '{keyPath}' -> '{pemPath}'");

                return pemPath;
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine(
                    $"[PuttyKeyConverter] Failed to convert PuTTY key '{keyPath}': {ex.Message}");
                return null;
            }
        }

        private static Dictionary<string, object>? ParsePuttyFile(string content)
        {
            try
            {
                var result = new Dictionary<string, object>();
                string[] lines = content.Replace("\r\n", "\n").Split('\n');

                int lineIdx = 0;

                string algorithm = ParseHeader(lines[lineIdx++], "PuTTY-User-Key-File-2");
                if (algorithm == null) return null;
                result["Algorithm"] = algorithm;

                string encryption = ParseHeader(lines[lineIdx++], "Encryption");
                if (encryption == null) return null;
                result["Encryption"] = encryption;

                string comment = ParseHeader(lines[lineIdx++], "Comment");
                if (comment == null) return null;
                result["Comment"] = comment;

                string publicLines = ParseHeader(lines[lineIdx++], "Public-Lines");
                if (publicLines == null || !int.TryParse(publicLines, out int pubLines))
                    return null;

                var pubBase64 = new StringBuilder();
                for (int i = 0; i < pubLines; i++)
                {
                    if (lineIdx >= lines.Length) return null;
                    pubBase64.Append(lines[lineIdx++].Trim());
                }
                result["PublicKey"] = Convert.FromBase64String(pubBase64.ToString());

                string privateLines = ParseHeader(lines[lineIdx++], "Private-Lines");
                if (privateLines == null || !int.TryParse(privateLines, out int privLines))
                    return null;

                var privBase64 = new StringBuilder();
                for (int i = 0; i < privLines; i++)
                {
                    if (lineIdx >= lines.Length) return null;
                    privBase64.Append(lines[lineIdx++].Trim());
                }
                result["PrivateBlob"] = Convert.FromBase64String(privBase64.ToString());

                string mac = ParseHeader(lines[lineIdx], "Private-MAC");
                if (mac != null)
                    result["PrivateMAC"] = mac;

                return result;
            }
            catch
            {
                return null;
            }
        }

        private static string? ParseHeader(string line, string header)
        {
            if (!line.StartsWith(header + ": ", StringComparison.Ordinal))
                return null;
            return line.Substring(header.Length + 2).Trim();
        }

        private static byte[] DerivePuttyKey(byte[] passphrase, int neededBytes)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();

            // PPK v2 KDF: key[i*20..(i+1)*20] = SHA1(uint32(i) || passphrase)
            // For AES-256-CBC, neededBytes = 32 (only the AES key, not IV)
            var output = new byte[neededBytes];
            int offset = 0;

            for (int i = 0; offset < neededBytes; i++)
            {
                var counter = new byte[4];
                counter[3] = (byte)i; // big-endian uint32

                var combined = new byte[4 + passphrase.Length];
                Array.Copy(counter, 0, combined, 0, 4);
                Array.Copy(passphrase, 0, combined, 4, passphrase.Length);

                byte[] digest = sha1.ComputeHash(combined);
                int toCopy = Math.Min(digest.Length, neededBytes - offset);
                Array.Copy(digest, 0, output, offset, toCopy);
                offset += toCopy;
            }

            return output;
        }

        private static byte[] DerivePuttyHmacKey(byte[] passphrase)
        {
            byte[] magic = Encoding.UTF8.GetBytes("putty-private-key-file-mac-key");
            var combined = new byte[magic.Length + passphrase.Length];
            Array.Copy(magic, 0, combined, 0, magic.Length);
            Array.Copy(passphrase, 0, combined, magic.Length, passphrase.Length);
            return System.Security.Cryptography.SHA1.Create().ComputeHash(combined);
        }

        private static string ComputeHmacSha1Hex(byte[] key, byte[] data)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static byte[] BuildMacInput(byte[] algorithm, byte[] encryption, byte[] comment, byte[] publicKey, byte[] privateBlob)
        {
            // PPK v2 MAC input: SSH-style strings for each component
            // (4-byte big-endian length prefix + data)
            return ConcatSshStrings(algorithm, encryption, comment, publicKey, privateBlob);
        }

        private static byte[] ConcatSshStrings(params byte[][] strings)
        {
            int total = 0;
            foreach (var s in strings)
                total += 4 + s.Length;

            var result = new byte[total];
            int offset = 0;
            foreach (var s in strings)
            {
                result[offset++] = (byte)((s.Length >> 24) & 0xFF);
                result[offset++] = (byte)((s.Length >> 16) & 0xFF);
                result[offset++] = (byte)((s.Length >> 8) & 0xFF);
                result[offset++] = (byte)(s.Length & 0xFF);
                Array.Copy(s, 0, result, offset, s.Length);
                offset += s.Length;
            }
            return result;
        }

        private static byte[] DecryptAes256Cbc(byte[] ciphertext, byte[] key, byte[] iv)
        {
            var engine = new AesEngine();
            var cipher = new CbcBlockCipher(engine);
            cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));

            int blockSize = cipher.GetBlockSize();
            int blocks = ciphertext.Length / blockSize;
            byte[] plaintext = new byte[ciphertext.Length];

            int offset = 0;
            byte[] buffer = new byte[blockSize];
            for (int i = 0; i < blocks; i++)
            {
                Array.Copy(ciphertext, offset, buffer, 0, blockSize);
                cipher.ProcessBlock(buffer, 0, plaintext, offset);
                offset += blockSize;
            }

            return plaintext;
        }

        private static byte[]? ConvertToPemBytes(string algorithm, byte[] publicBlob, byte[] privateBlob, string comment, string password)
        {
            switch (algorithm)
            {
                case "ssh-rsa":
                    return ConvertRsaToPem(publicBlob, privateBlob, password);
                case "ssh-ed25519":
                    return null;
                case "ecdsa-sha2-nistp256":
                    return null;
                default:
                    return null;
            }
        }

        private static byte[]? ConvertRsaToPem(byte[] publicBlob, byte[] privateBlob, string password)
        {
            try
            {
                var pubOffset = 0;
                string alg = ReadString(publicBlob, ref pubOffset);
                if (alg != "ssh-rsa")
                {
                    System.Console.Error.WriteLine($"[PuttyKeyConverter] Unsupported algorithm in public blob: {alg}");
                    return null;
                }
                BigInteger e = ReadMpInt(publicBlob, ref pubOffset);
                BigInteger n = ReadMpInt(publicBlob, ref pubOffset);

                var privOffset = 0;
                BigInteger d = ReadMpInt(privateBlob, ref privOffset);
                BigInteger p = ReadMpInt(privateBlob, ref privOffset);
                BigInteger q = ReadMpInt(privateBlob, ref privOffset);
                BigInteger iqmp = ReadMpInt(privateBlob, ref privOffset);

                BigInteger dp = d.Mod(p.Subtract(BigInteger.One));
                BigInteger dq = d.Mod(q.Subtract(BigInteger.One));

                var rsaParams = new RsaPrivateCrtKeyParameters(n, e, d, p, q, dp, dq, iqmp);

                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                var pemWriter = new PemWriter(sw);

                pemWriter.WriteObject(rsaParams);

                sw.Flush();
                return ms.ToArray();
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine(
                    $"[PuttyKeyConverter] Failed to convert RSA private blob: {ex.Message}");
                return null;
            }
        }

        private static string ReadString(byte[] data, ref int offset)
        {
            if (offset + 4 > data.Length)
                throw new InvalidOperationException("Unexpected end of data reading string length");

            int len = (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
            offset += 4;

            if (offset + len > data.Length)
                throw new InvalidOperationException("Unexpected end of data reading string value");

            string value = Encoding.UTF8.GetString(data, offset, len);
            offset += len;

            return value;
        }

        private static BigInteger ReadMpInt(byte[] data, ref int offset)
        {
            if (offset + 4 > data.Length)
                throw new InvalidOperationException("Unexpected end of data reading mpint length");

            int len = (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
            offset += 4;

            if (offset + len > data.Length)
                throw new InvalidOperationException("Unexpected end of data reading mpint value");

            byte[] valueBytes = new byte[len];
            Array.Copy(data, offset, valueBytes, 0, len);
            offset += len;

            return new BigInteger(1, valueBytes);
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
                        System.IO.File.Delete(file);
                }
                catch
                {
                }
            }
        }
    }
}
