using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default PROXY protocol acceptor for direct SSH, PROXY v1, and PROXY v2 connections.
    /// </summary>
    public class AMNetServerProxyAcceptor : IAMNetServerProxyAcceptor
    {
        /// <summary>
        /// Creates a default PROXY protocol acceptor.
        /// </summary>
        public AMNetServerProxyAcceptor()
        {
        }

        /// <inheritdoc />
        public virtual bool acceptServerProxyMetadata(IProxyMetadata proxyMetadata)
        {
            int readPosition = proxyMetadata.CurrentReadPosition;

            // 1. Fetch a safe, non-destructive snapshot of the initial bytes
            byte[] rawBytes = proxyMetadata.GetRawBytes();
            if (rawBytes.Length < 5)
            {
                return false; // Wait for the network card to finish buffering
            }

            // =========================================================================
            // DIRECT BYPASS (No Load Balancer / Local Dev Mode)
            // If it starts with "SSH-", it is a direct connection bypassing the proxy.
            // =========================================================================
            if (rawBytes[0] == 0x53 && rawBytes[1] == 0x53 && rawBytes[2] == 0x48 && rawBytes[3] == 0x2D) // 'S','S','H','-'
            {
                proxyMetadata.Seek(readPosition);
                return true; // Handshake proceeds natively
            }

            // =========================================================================
            // PROXY PROTOCOL V2 (Modern Cloud Balancers - Binary Structure)
            // Immutable 12-byte signature check block: \r\n\r\n\0\r\nQUIT\n
            // =========================================================================
            bool isV2Binary = rawBytes.Length >= 16 &&
                              rawBytes[0] == 0x0D && rawBytes[1] == 0x0A &&
                              rawBytes[2] == 0x0D && rawBytes[3] == 0x0A &&
                              rawBytes[4] == 0x00 && rawBytes[5] == 0x0D &&
                              rawBytes[6] == 0x0A && rawBytes[7] == 0x51 &&
                              rawBytes[8] == 0x55 && rawBytes[9] == 0x49 &&
                              rawBytes[10] == 0x54 && rawBytes[11] == 0x0A;

            if (isV2Binary)
            {
                return ProcessV2BinaryFormat(proxyMetadata, rawBytes, readPosition);
            }

            // =========================================================================
            // PROXY PROTOCOL V1 (Traditional Balancers - Human-Readable Text)
            // Enforces strict text protocol specifications: "PROXY TCP4 " or "PROXY TCP6 "
            // =========================================================================
            string header = proxyMetadata.ReadRawString();
            if (header.StartsWith("PROXY TCP4 ") || header.StartsWith("PROXY TCP6 "))
            {
                int endOfLine = header.IndexOf("\r\n");
                if (endOfLine == -1)
                {
                    proxyMetadata.Seek(readPosition); // Text line is incomplete; retry on next packet
                    return false;
                }

                string proxyLine = header.Substring(0, endOfLine);
                proxyMetadata.Seek(readPosition + endOfLine + 2); // Permanently advance past the proxy string

                string[] parts = proxyLine.Split(' ');
                if (parts.Length < 6)
                {
                    proxyMetadata.ForceDisconnect("Incomplete text proxy parameters.");
                    throw new InvalidOperationException("Invalid text PROXY parameters.");
                }

                string clientIp = parts[2];
                int clientSourcePort = int.Parse(parts[4]);

                proxyMetadata.SetRealClientAddressAndPort(clientIp, clientSourcePort);
                return true;
            }

            // =========================================================================
            // PROTECTION FOR UNKNOWN / MALICIOUS TRAFFIC
            // If it reaches here, it violates the expected transport standards.
            // =========================================================================
            proxyMetadata.ForceDisconnect("Protocol violation. Unrecognized initial payload.");
            throw new InvalidOperationException("Malformed transport layout protocol.");
        }
        private bool ProcessV2BinaryFormat(IProxyMetadata proxyMetadata, byte[] rawBytes, int startPos)
        {
            // Byte offset 12 dictates protocol command, byte 13 dictates address family profiles
            byte commandByte = rawBytes[12];
            byte familyByte = rawBytes[13];

            // Read the exact payload data size declared by the binary frame (byte offset 14-15)
            int payloadLength = (rawBytes[14] << 8) | rawBytes[15];
            int absoluteHeaderSize = 16 + payloadLength;

            if (rawBytes.Length < absoluteHeaderSize)
            {
                proxyMetadata.Seek(startPos); // Fragmented binary frame; wait for next TCP packets
                return false;
            }

            // Advance the underlying MINA pointer cleanly past the binary structure block
            proxyMetadata.Seek(startPos + absoluteHeaderSize);

            // Check if this connection is a LOCAL HEALTH-CHECK PROBE (Command 0x00 / LOCAL)
            // AWS NLB, HAProxy, and Azure LB send these empty pings to see if your server port is open.
            if ((commandByte & 0x0F) == 0x00)
            {
                // Bind to localhost securely so it satisfies MINA without polling data metrics
                proxyMetadata.SetRealClientAddressAndPort("127.0.0.1", 0);
                return true;
            }

            // Decode protocol profile types (0x10 = IPv4 over TCP, 0x20 = IPv6 over TCP)
            bool isIPv4 = (familyByte & 0xF0) == 0x10;
            bool isIPv6 = (familyByte & 0xF0) == 0x20;

            string clientIp;
            int clientSourcePort;

            if (isIPv4)
            {
                // IPv4 data blocks sit at fixed byte offsets starting at index 16
                clientIp = $"{rawBytes[16]}.{rawBytes[17]}.{rawBytes[18]}.{rawBytes[19]}";
                clientSourcePort = (rawBytes[24] << 8) | rawBytes[25]; // Big-endian deserialization
            }
            else if (isIPv6)
            {
                // IPv6 chunks occupy exactly 16 bytes (offsets 16 through 31)
                var ipBytes = new byte[16];
                Array.Copy(rawBytes, 16, ipBytes, 0, 16);

                // System.Net.IPAddress converts the raw bytes cleanly to standard text format
                clientIp = new System.Net.IPAddress(ipBytes).ToString();

                // Port sits immediately after source IP (16 bytes) and destination IP (16 bytes) -> index 48-49
                clientSourcePort = (rawBytes[48] << 8) | rawBytes[49];
            }
            else
            {
                // Fallback for unexpected or unsupported address blocks (e.g., Unix Domain Sockets)
                proxyMetadata.SetRealClientAddressAndPort("127.0.0.1", 0);
                return true;
            }

            proxyMetadata.SetRealClientAddressAndPort(clientIp, clientSourcePort);
            return true;
        }


    }
}

