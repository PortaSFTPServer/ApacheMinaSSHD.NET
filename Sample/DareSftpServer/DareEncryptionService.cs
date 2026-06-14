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

using System.Security.Cryptography;

namespace DareSftpServer;

public sealed class DareEncryptionService : IDisposable
{
    public const int DefaultChunkSize = 65536;
    private const int HeaderSize = 32;
    private static readonly byte[] Magic = [0x43, 0x45, 0x4E, 0x43]; // "CENC"
    private static readonly short FormatVersion = 1;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int AesKeySize = 32;

    private readonly AesGcm _aes;
    private readonly int _chunkSize;

    public DareEncryptionService(byte[] key, int chunkSize = DefaultChunkSize)
    {
        if (key.Length != AesKeySize)
            throw new ArgumentException($"Key must be {AesKeySize} bytes", nameof(key));
        if (chunkSize <= 0 || chunkSize > 16 * 1024 * 1024)
            throw new ArgumentException("Chunk size must be between 1 and 16 MiB", nameof(chunkSize));

        _aes = new AesGcm(key, TagSize);
        _chunkSize = chunkSize;
    }

    public int ChunkSize => _chunkSize;

    public void Dispose() => _aes.Dispose();

    public async Task EncryptFileAsync(string sourcePath, string outputPath, string filename, CancellationToken ct = default)
    {
        var plaintext = await File.ReadAllBytesAsync(sourcePath, ct);
        var encrypted = EncryptAll(plaintext, filename);
        await File.WriteAllBytesAsync(outputPath, encrypted, ct);
    }

    public async Task DecryptFileAsync(string sourcePath, string outputPath, string filename, CancellationToken ct = default)
    {
        var encrypted = await File.ReadAllBytesAsync(sourcePath, ct);
        var plaintext = DecryptAll(encrypted, filename);
        await File.WriteAllBytesAsync(outputPath, plaintext, ct);
    }

    public byte[] EncryptAll(byte[] plaintext, string filename)
    {
        var totalChunks = (int)Math.Ceiling((double)plaintext.Length / _chunkSize);
        using var ms = new MemoryStream();

        WriteHeader(ms, _chunkSize, plaintext.Length);

        for (int i = 0; i < totalChunks; i++)
        {
            var offset = i * _chunkSize;
            var count = Math.Min(_chunkSize, plaintext.Length - offset);
            var chunk = plaintext.AsSpan(offset, count);
            var encrypted = EncryptChunk(chunk, filename, i);
            WriteChunk(ms, encrypted);
        }

        return ms.ToArray();
    }

    public byte[] DecryptAll(byte[] encrypted, string filename)
    {
        var header = ParseHeader(encrypted);
        using var ms = new MemoryStream((int)Math.Min(header.originalSize, int.MaxValue));

        Span<byte> data = encrypted.AsSpan(HeaderSize);
        for (int i = 0; i < header.totalChunks; i++)
        {
            var chunkSize = ReadInt(data);
            data = data[4..];

            var decrypted = DecryptChunk(data[..chunkSize], filename, i);
            ms.Write(decrypted);
            data = data[chunkSize..];
        }

        return ms.ToArray();
    }

    public byte[] EncryptStream(Stream plaintext, string filename)
    {
        using var ms = new MemoryStream();
        WriteHeader(ms, _chunkSize, 0);

        int chunkIndex = 0;
        byte[] buffer = new byte[_chunkSize];
        int bytesRead;
        long totalWritten = 0;

        while ((bytesRead = plaintext.Read(buffer, 0, _chunkSize)) > 0)
        {
            var encrypted = EncryptChunk(buffer.AsSpan(0, bytesRead), filename, chunkIndex);
            WriteChunk(ms, encrypted);
            totalWritten += bytesRead;
            chunkIndex++;
        }

        UpdateOriginalSize(ms, totalWritten);
        return ms.ToArray();
    }

    public byte[] DecryptToArray(Stream encryptedStream, string filename)
    {
        using var ms = new MemoryStream();
        var header = ParseHeaderFromStream(encryptedStream, out var totalChunks, out var stream);

        byte[] chunkSizeBuf = new byte[4];
        for (int i = 0; i < totalChunks; i++)
        {
            stream.ReadExactly(chunkSizeBuf);
            var chunkLen = BitConverter.ToInt32(chunkSizeBuf);
            var encryptedChunk = new byte[chunkLen];
            stream.ReadExactly(encryptedChunk);

            var decrypted = DecryptChunk(encryptedChunk, filename, i);
            ms.Write(decrypted);
        }

        return ms.ToArray();
    }

    private (long originalSize, int totalChunks, int chunkSize) ParseHeader(byte[] data)
    {
        if (data.Length < HeaderSize)
            throw new InvalidDataException("File too small for DARE header");

        var magic = data.AsSpan(0, 4);
        if (!magic.SequenceEqual(Magic))
            throw new InvalidDataException("Invalid DARE magic bytes");

        var version = BitConverter.ToInt16(data, 4);
        if (version != FormatVersion)
            throw new InvalidDataException($"Unsupported DARE version: {version}");

        var chunkSize = BitConverter.ToInt32(data, 6);
        var originalSize = BitConverter.ToInt64(data, 10);
        var totalChunks = (int)Math.Ceiling((double)originalSize / chunkSize);

        return (originalSize, totalChunks, chunkSize);
    }

    private DareHeader ParseHeaderFromStream(Stream stream, out int totalChunks, out Stream remaining)
    {
        byte[] header = new byte[HeaderSize];
        stream.ReadExactly(header);

        var magic = header.AsSpan(0, 4);
        if (!magic.SequenceEqual(Magic))
            throw new InvalidDataException("Invalid DARE magic bytes");

        var version = BitConverter.ToInt16(header, 4);
        if (version != FormatVersion)
            throw new InvalidDataException($"Unsupported DARE version: {version}");

        var chunkSize = BitConverter.ToInt32(header, 6);
        var originalSize = BitConverter.ToInt64(header, 10);
        totalChunks = (int)Math.Ceiling((double)originalSize / chunkSize);
        remaining = stream;

        return new DareHeader(version, chunkSize, originalSize);
    }

    private byte[] EncryptChunk(ReadOnlySpan<byte> plaintext, string filename, int chunkIndex)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] ciphertext = new byte[plaintext.Length + TagSize];
        byte[] aad = GetAssociatedData(filename, chunkIndex);

        _aes.Encrypt(nonce, plaintext, ciphertext, aad);

        var result = new byte[NonceSize + ciphertext.Length];
        nonce.CopyTo(result.AsSpan(0, NonceSize));
        ciphertext.CopyTo(result.AsSpan(NonceSize));
        return result;
    }

    private byte[] DecryptChunk(ReadOnlySpan<byte> encryptedChunk, string filename, int chunkIndex)
    {
        var nonce = encryptedChunk[..NonceSize];
        var ciphertext = encryptedChunk[NonceSize..];
        var plaintext = new byte[ciphertext.Length - TagSize];
        var aad = GetAssociatedData(filename, chunkIndex);

        _aes.Decrypt(nonce, ciphertext, aad, plaintext);
        return plaintext;
    }

    private static byte[] GetAssociatedData(string filename, int chunkIndex)
        => System.Text.Encoding.UTF8.GetBytes($"{filename}:chunk:{chunkIndex}");

    private void WriteHeader(MemoryStream ms, int chunkSize, long originalSize)
    {
        ms.Write(Magic);
        ms.Write(BitConverter.GetBytes(FormatVersion));
        ms.Write(BitConverter.GetBytes(chunkSize));
        ms.Write(BitConverter.GetBytes(originalSize));
        ms.Write(new byte[14]); // reserved
    }

    private static void UpdateOriginalSize(MemoryStream ms, long originalSize)
    {
        var buf = BitConverter.GetBytes(originalSize);
        ms.Position = 10;
        ms.Write(buf, 0, buf.Length);
    }

    private static void WriteChunk(MemoryStream ms, byte[] encrypted)
    {
        ms.Write(BitConverter.GetBytes(encrypted.Length));
        ms.Write(encrypted);
    }

    private static int ReadInt(ReadOnlySpan<byte> data) => BitConverter.ToInt32(data);

    private readonly record struct DareHeader(short Version, int ChunkSize, long OriginalSize)
    {
        public readonly int TotalChunks => (int)Math.Ceiling((double)OriginalSize / ChunkSize);
    }
}
