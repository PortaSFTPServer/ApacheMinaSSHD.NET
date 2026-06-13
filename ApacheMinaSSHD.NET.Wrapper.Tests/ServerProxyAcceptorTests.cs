// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class ServerProxyAcceptorTests
{
    private sealed class MockProxyMetadata : IProxyMetadata
    {
        public byte[] RawBytes { get; set; } = [];
        public string RawString { get; set; } = "";
        public int CurrentReadPositionValue { get; set; }
        public int SeekPosition { get; private set; } = -1;
        public bool SeekCalled { get; private set; }
        public string? SetAddress { get; private set; }
        public int SetPort { get; private set; }
        public bool SetRealClientCalled { get; private set; }
        public string? ForceDisconnectReason { get; private set; }
        public bool ForceDisconnectCalled { get; private set; }

        public string? GetHeader() => null;
        public string RemoteAddress => "0.0.0.0";
        public string LocalAddress => "127.0.0.1";
        public int AvailableBytes => RawBytes.Length;
        public int CurrentReadPosition => CurrentReadPositionValue;
        public string ReadRawString() => RawString;
        public void Seek(int pos) { SeekCalled = true; SeekPosition = pos; }
        public string GetHostname() => "";
        public void StoreAttribute(string key, object? value) { }
        public void ForceDisconnect(string reason) { ForceDisconnectCalled = true; ForceDisconnectReason = reason; }
        public byte[] GetRawBytes() => RawBytes;
        public void SetRealClientAddressAndPort(string address, int port) { SetRealClientCalled = true; SetAddress = address; SetPort = port; }
    }

    private static byte[] StringToBytes(string s) => System.Text.Encoding.ASCII.GetBytes(s);
    private static byte[] HexToBytes(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    private readonly AMNetServerProxyAcceptor _acceptor = new();

    [Fact]
    public void Accept_direct_SSH_connection()
    {
        var meta = new MockProxyMetadata
        {
            RawBytes = StringToBytes("SSH-2.0-OpenSSH_9.0"),
            RawString = "SSH-2.0-OpenSSH_9.0",
            CurrentReadPositionValue = 0
        };
        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SeekCalled);
        Assert.Equal(0, meta.SeekPosition);
    }

    [Fact]
    public void Accept_less_than_5_bytes_returns_false()
    {
        var meta = new MockProxyMetadata
        {
            RawBytes = [0x53, 0x53, 0x48],
            CurrentReadPositionValue = 0
        };
        Assert.False(_acceptor.acceptServerProxyMetadata(meta));
    }

    [Fact]
    public void Accept_empty_bytes_returns_false()
    {
        var meta = new MockProxyMetadata
        {
            RawBytes = [],
            CurrentReadPositionValue = 0
        };
        Assert.False(_acceptor.acceptServerProxyMetadata(meta));
    }

    [Fact]
    public void Accept_PROXY_v1_TCP4()
    {
        string proxyLine = "PROXY TCP4 192.168.1.10 10.0.0.1 12345 22\r\n";
        var meta = new MockProxyMetadata
        {
            RawBytes = StringToBytes(proxyLine),
            RawString = proxyLine,
            CurrentReadPositionValue = 0
        };
        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SetRealClientCalled);
        Assert.Equal("192.168.1.10", meta.SetAddress);
        Assert.Equal(12345, meta.SetPort);
    }

    [Fact]
    public void Accept_PROXY_v1_TCP6()
    {
        string proxyLine = "PROXY TCP6 ::1 ::1 54321 22\r\n";
        var meta = new MockProxyMetadata
        {
            RawBytes = StringToBytes(proxyLine),
            RawString = proxyLine,
            CurrentReadPositionValue = 0
        };
        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SetRealClientCalled);
        Assert.Equal("::1", meta.SetAddress);
        Assert.Equal(54321, meta.SetPort);
    }

    [Fact]
    public void Accept_PROXY_v1_incomplete_line_returns_false()
    {
        string partialLine = "PROXY TCP4 192.168.1";
        var meta = new MockProxyMetadata
        {
            RawBytes = StringToBytes(partialLine),
            RawString = partialLine,
            CurrentReadPositionValue = 0
        };
        Assert.False(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SeekCalled);
        Assert.Equal(0, meta.SeekPosition);
    }

    [Fact]
    public void Accept_PROXY_v1_missing_parameters_throws()
    {
        string proxyLine = "PROXY TCP4 192.168.1.10 10.0.0.1\r\n";
        var meta = new MockProxyMetadata
        {
            RawBytes = StringToBytes(proxyLine),
            RawString = proxyLine,
            CurrentReadPositionValue = 0
        };
        var ex = Assert.Throws<InvalidOperationException>(() => _acceptor.acceptServerProxyMetadata(meta));
        Assert.Contains("Invalid text PROXY parameters", ex.Message);
        Assert.True(meta.ForceDisconnectCalled);
        Assert.Contains("Incomplete text proxy parameters", meta.ForceDisconnectReason);
    }

    [Fact]
    public void Accept_PROXY_v2_IPv4()
    {
        var raw = new byte[16 + 12]; // header 16 + payload 12
        // V2 signature
        raw[0] = 0x0D; raw[1] = 0x0A; raw[2] = 0x0D; raw[3] = 0x0A;
        raw[4] = 0x00; raw[5] = 0x0D; raw[6] = 0x0A; raw[7] = 0x51;
        raw[8] = 0x55; raw[9] = 0x49; raw[10] = 0x54; raw[11] = 0x0A;
        raw[12] = 0x21; // command: PROXY
        raw[13] = 0x11; // family: IPv4 + TCP (0x10 | 0x01)
        raw[14] = 0x00; raw[15] = 0x0C; // payload length = 12
        // IPv4 src: 10.0.0.1
        raw[16] = 10; raw[17] = 0; raw[18] = 0; raw[19] = 1;
        // IPv4 dst: 10.0.0.2
        raw[20] = 10; raw[21] = 0; raw[22] = 0; raw[23] = 2;
        // src port: 50000
        raw[24] = 0xC3; raw[25] = 0x50;
        // dst port: 22
        raw[26] = 0x00; raw[27] = 0x16;

        var meta = new MockProxyMetadata
        {
            RawBytes = raw,
            RawString = System.Text.Encoding.ASCII.GetString(raw, 0, raw.Length),
            CurrentReadPositionValue = 0
        };

        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SetRealClientCalled);
        Assert.Equal("10.0.0.1", meta.SetAddress);
        Assert.Equal(50000, meta.SetPort);
        Assert.True(meta.SeekCalled);
        Assert.Equal(28, meta.SeekPosition); // 16 + 12
    }

    [Fact]
    public void Accept_PROXY_v2_IPv6()
    {
        var raw = new byte[16 + 36]; // header 16 + payload 36
        raw[0] = 0x0D; raw[1] = 0x0A; raw[2] = 0x0D; raw[3] = 0x0A;
        raw[4] = 0x00; raw[5] = 0x0D; raw[6] = 0x0A; raw[7] = 0x51;
        raw[8] = 0x55; raw[9] = 0x49; raw[10] = 0x54; raw[11] = 0x0A;
        raw[12] = 0x21; // command: PROXY
        raw[13] = 0x21; // family: IPv6 + TCP (0x20 | 0x01)
        raw[14] = 0x00; raw[15] = 0x24; // payload length = 36
        // IPv6 src: ::1
        raw[16] = 0; raw[17] = 0; raw[18] = 0; raw[19] = 0;
        raw[20] = 0; raw[21] = 0; raw[22] = 0; raw[23] = 0;
        raw[24] = 0; raw[25] = 0; raw[26] = 0; raw[27] = 0;
        raw[28] = 0; raw[29] = 0; raw[30] = 0; raw[31] = 1;
        // IPv6 dst: ::2 (16 bytes start at 32)
        raw[32] = 0; raw[33] = 0; raw[34] = 0; raw[35] = 0;
        raw[36] = 0; raw[37] = 0; raw[38] = 0; raw[39] = 0;
        raw[40] = 0; raw[41] = 0; raw[42] = 0; raw[43] = 0;
        raw[44] = 0; raw[45] = 0; raw[46] = 0; raw[47] = 2;
        // src port: 60000
        raw[48] = 0xEA; raw[49] = 0x60;
        // dst port: 22
        raw[50] = 0x00; raw[51] = 0x16;

        var meta = new MockProxyMetadata
        {
            RawBytes = raw,
            CurrentReadPositionValue = 0
        };

        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SetRealClientCalled);
        Assert.Equal("::1", meta.SetAddress);
        Assert.Equal(60000, meta.SetPort);
    }

    [Fact]
    public void Accept_PROXY_v2_LOCAL_health_check()
    {
        var raw = new byte[16]; // LOCAL command, no payload
        raw[0] = 0x0D; raw[1] = 0x0A; raw[2] = 0x0D; raw[3] = 0x0A;
        raw[4] = 0x00; raw[5] = 0x0D; raw[6] = 0x0A; raw[7] = 0x51;
        raw[8] = 0x55; raw[9] = 0x49; raw[10] = 0x54; raw[11] = 0x0A;
        raw[12] = 0x00; // command: LOCAL
        raw[13] = 0x00;
        raw[14] = 0x00; raw[15] = 0x00; // payload length = 0

        var meta = new MockProxyMetadata
        {
            RawBytes = raw,
            CurrentReadPositionValue = 0
        };

        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SetRealClientCalled);
        Assert.Equal("127.0.0.1", meta.SetAddress);
        Assert.Equal(0, meta.SetPort);
    }

    [Fact]
    public void Accept_PROXY_v2_fragmented_returns_false()
    {
        var raw = new byte[16 + 4]; // 16 header + 4 payload bytes declared, but need 12 for actual payload
        raw[0] = 0x0D; raw[1] = 0x0A; raw[2] = 0x0D; raw[3] = 0x0A;
        raw[4] = 0x00; raw[5] = 0x0D; raw[6] = 0x0A; raw[7] = 0x51;
        raw[8] = 0x55; raw[9] = 0x49; raw[10] = 0x54; raw[11] = 0x0A;
        raw[12] = 0x21; // command: PROXY
        raw[13] = 0x11; // family: IPv4 + TCP
        raw[14] = 0x00; raw[15] = 0x0C; // payload length = 12, but we only have 20 bytes total (16+4)

        var meta = new MockProxyMetadata
        {
            RawBytes = raw,
            CurrentReadPositionValue = 5
        };

        Assert.False(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SeekCalled);
        Assert.Equal(5, meta.SeekPosition);
    }

    [Fact]
    public void Accept_PROXY_v2_unknown_family_falls_back_to_localhost()
    {
        var raw = new byte[20];
        raw[0] = 0x0D; raw[1] = 0x0A; raw[2] = 0x0D; raw[3] = 0x0A;
        raw[4] = 0x00; raw[5] = 0x0D; raw[6] = 0x0A; raw[7] = 0x51;
        raw[8] = 0x55; raw[9] = 0x49; raw[10] = 0x54; raw[11] = 0x0A;
        raw[12] = 0x21;
        raw[13] = 0x31; // unknown family (0x30 = Unix socket)
        raw[14] = 0x00; raw[15] = 0x04; // 4 bytes
        raw[16] = 0; raw[17] = 0; raw[18] = 0; raw[19] = 0;

        var meta = new MockProxyMetadata
        {
            RawBytes = raw,
            CurrentReadPositionValue = 0
        };

        Assert.True(_acceptor.acceptServerProxyMetadata(meta));
        Assert.True(meta.SetRealClientCalled);
        Assert.Equal("127.0.0.1", meta.SetAddress);
        Assert.Equal(0, meta.SetPort);
    }

    [Fact]
    public void Accept_malformed_traffic_throws()
    {
        var meta = new MockProxyMetadata
        {
            RawBytes = StringToBytes("GET / HTTP/1.1\r\n"),
            RawString = "GET / HTTP/1.1\r\n",
            CurrentReadPositionValue = 0
        };

        var ex = Assert.Throws<InvalidOperationException>(() => _acceptor.acceptServerProxyMetadata(meta));
        Assert.Contains("Malformed transport layout protocol", ex.Message);
        Assert.True(meta.ForceDisconnectCalled);
    }
}
