using System.Security.Cryptography;
using System.Text;
using ApacheMinaSSHD.NET.Wrapper.Helpers;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class PuttyKeyConverterTests
{
    private static readonly string FixturePath = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "Fixtures", "putty_test_key.ppk");

    private const string KnownPassphrase = "Jk_%zOaAnKpA1ioXtT";

    private const string ExpectedMac = "551f36cbf7690c642f73e2b4f50dbbb7d74cffd6";

    private const string ExpectedSha256 = "c14bf69751838db45e310f2648eb9a47c785332fe4d4d2df9768066a5f40a136";

    [Fact]
    public void Parse_putty_key_file_headers()
    {
        var content = File.ReadAllText(FixturePath);
        Assert.StartsWith("PuTTY-User-Key-File-2:", content);
    }

    [Fact]
    public void Verify_passphrase_sha256()
    {
        byte[] passphrase = Encoding.UTF8.GetBytes(KnownPassphrase);
        byte[] hash = SHA256.HashData(passphrase);
        string hex = Convert.ToHexString(hash).ToLowerInvariant();
        Assert.Equal(ExpectedSha256, hex);
    }

    [Fact]
    public void Decrypt_with_ppk_v2_kdf_and_verify_mac()
    {
        var parsed = ParsePuttyFile(FixturePath);
        byte[] passphrase = Encoding.UTF8.GetBytes(KnownPassphrase);

        // PPK v2 KDF: key[i*20..] = SHA1(uint32(i) || passphrase), IV = all zeros
        byte[] aesKey = DerivePuttyV2Key(passphrase, 32);
        byte[] iv = new byte[16];

        byte[] plaintext = DecryptAes(parsed.encryptedBlob, aesKey, iv);

        // Verify decrypted blob starts with valid mpint length for 1024-bit RSA
        // First mpint is d (private exponent), typically ~128 bytes
        int dLen = (plaintext[0] << 24) | (plaintext[1] << 16) | (plaintext[2] << 8) | plaintext[3];
        Assert.Equal(128, dLen);

        // Verify MAC with SSH-style string format
        byte[] macKey = DerivePuttyHmacKey(passphrase);
        byte[] algoBytes = Encoding.UTF8.GetBytes(parsed.algorithm);
        byte[] encBytes = Encoding.UTF8.GetBytes(parsed.encryption);
        byte[] commentBytes = Encoding.UTF8.GetBytes(parsed.comment);
        byte[] macInput = BuildMacInput(algoBytes, encBytes, commentBytes, parsed.publicBlob, plaintext);
        string computedMac = ComputeHmacSha1Hex(macKey, macInput);

        Assert.Equal(ExpectedMac, computedMac);
    }

    [Fact]
    public void Convert_via_try_convert_to_pem()
    {
        // Test the full production pipeline: decrypt + parse + PEM output
        string? pemPath = PuttyKeyConverter.TryConvertToPem(FixturePath, KnownPassphrase);

        Assert.NotNull(pemPath);
        Assert.True(File.Exists(pemPath), "PEM file should exist");
        string pemContent = File.ReadAllText(pemPath);
        Assert.StartsWith("-----BEGIN RSA PRIVATE KEY-----", pemContent);
        Assert.Contains("-----END RSA PRIVATE KEY-----", pemContent);

        // Verify it can be parsed by .NET RSA
        using var rsa = RSA.Create();
        rsa.ImportFromPem(pemContent);
        Assert.True(rsa.KeySize >= 1024, "RSA key should be at least 1024 bits");
    }

    private static (byte[] encryptedBlob, byte[] publicBlob, string algorithm, string encryption, string comment)
        ParsePuttyFile(string path)
    {
        string[] lines = File.ReadAllLines(path);
        int idx = 0;

        string algorithm = lines[idx++].Split(": ", 2)[1];
        string encryption = lines[idx++].Split(": ", 2)[1];
        string comment = lines[idx++].Split(": ", 2)[1];
        int pubLines = int.Parse(lines[idx++].Split(": ", 2)[1]);

        var pubB64 = new StringBuilder();
        for (int i = 0; i < pubLines; i++)
            pubB64.Append(lines[idx++].Trim());
        byte[] publicBlob = Convert.FromBase64String(pubB64.ToString());

        int privLines = int.Parse(lines[idx++].Split(": ", 2)[1]);
        var privB64 = new StringBuilder();
        for (int i = 0; i < privLines; i++)
            privB64.Append(lines[idx++].Trim());
        byte[] encryptedBlob = Convert.FromBase64String(privB64.ToString());

        return (encryptedBlob, publicBlob, algorithm, encryption, comment);
    }

    private static byte[] DerivePuttyV2Key(byte[] passphrase, int neededBytes)
    {
        using var sha1 = SHA1.Create();
        var output = new byte[neededBytes];
        int offset = 0;

        for (int i = 0; offset < neededBytes; i++)
        {
            var counter = new byte[4];
            counter[3] = (byte)i;

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
        Array.Copy(magic, combined, magic.Length);
        Array.Copy(passphrase, 0, combined, magic.Length, passphrase.Length);
        return SHA1.HashData(combined);
    }

    private static byte[] BuildMacInput(byte[] algorithm, byte[] encryption, byte[] comment,
        byte[] publicBlob, byte[] privateBlob)
    {
        int total = 0;
        byte[][] strings = [algorithm, encryption, comment, publicBlob, privateBlob];
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

    private static string ComputeHmacSha1Hex(byte[] key, byte[] data)
    {
        using var hmac = new HMACSHA1(key);
        byte[] hash = hmac.ComputeHash(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static byte[] DecryptAes(byte[] ciphertext, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }

}
