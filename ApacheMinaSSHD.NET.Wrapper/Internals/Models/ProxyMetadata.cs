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

using System.Text;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Infrastructure;
using java.net;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    /// <summary>
    /// Encapsulates the network buffer state and session metadata for active MINA SSHD proxy evaluation.
    /// </summary>
    internal class ProxyMetadata : IProxyMetadata
    {
        private readonly ServerSession _session;
        private readonly org.apache.sshd.common.util.buffer.Buffer _buffer;

        public ProxyMetadata(ServerSession session, org.apache.sshd.common.util.buffer.Buffer buffer)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public string RemoteAddress => _session.getIoSession().getRemoteAddress()?.ToString() ?? "Unknown";
        public string LocalAddress => _session.getIoSession().getLocalAddress()?.ToString() ?? "Unknown";
        public int AvailableBytes => _buffer.available();
        public int CurrentReadPosition => _buffer.rpos();
        public ServerSession UnderlyingSession => _session;

        /// <summary>
        /// Safely peeks at the entire available stream as a string without permanently altering the underlying MINA stream pointer.
        /// </summary>
        public string ReadRawString()
        {
            int initialPos = _buffer.rpos();
            int available = _buffer.available();
            if (available <= 0) return string.Empty;

            try
            {
                byte[] data = new byte[available];
                _buffer.getRawBytes(data);
                return Encoding.UTF8.GetString(data);
            }
            finally
            {
                // CRITICAL CORRECTION: Restores the pointer position to preserve data stability
                _buffer.rpos(initialPos);
            }
        }

        public void Seek(int pos) => _buffer.rpos(pos);

        public void StoreAttribute(string key, object? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Attribute key cannot be empty.", nameof(key));
            }

            _session.setAttribute(ProxyAttributes.GetOrCreate(key), (java.lang.Object)value!);
        }

        public void ForceDisconnect(string reason)
        {
            // Closes the physical socket interface safely within the MINA network core loop
            _session.close(true);
        }

        public string GetHostname()
        {
            try
            {
                var remoteAddress = _session.getIoSession().getRemoteAddress() as InetSocketAddress;
                var inet = remoteAddress?.getAddress();
                return inet?.getHostName() ?? "Unknown";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(ProxyMetadata)}] GetHostname failed: {ex.Message}");
                return "Unknown";
            }
        }

        /// <summary>
        /// Isolates and consumes exactly one text header line, safely handling TCP fragmentation conditions.
        /// </summary>
        public string? GetHeader()
        {
            int initialPos = _buffer.rpos();
            int available = _buffer.available();
            if (available <= 0) return null;

            try
            {
                byte[] data = new byte[available];
                _buffer.getRawBytes(data);
                string content = Encoding.UTF8.GetString(data);

                // Look for the standard PROXY protocol line terminator
                int eol = content.IndexOf("\r\n");
                if (eol != -1)
                {
                    // Advance the buffer position permanently past the header payload boundary
                    _buffer.rpos(initialPos + eol + 2);
                    return content.Substring(0, eol);
                }

                // If incomplete, reset rpos so MINA retains existing stream segments while waiting for more data
                _buffer.rpos(initialPos);
                return null;
            }
            catch
            {
                _buffer.rpos(initialPos);
                throw;
            }
        }

        /// <summary>
        /// Non-destructive extraction tool to evaluate data chunks without triggering pointer advancement anomalies.
        /// </summary>
        public byte[] GetRawBytes()
        {
            int initialPos = _buffer.rpos();
            int available = _buffer.available();
            if (available <= 0) return Array.Empty<byte>();

            try
            {
                byte[] data = new byte[available];
                _buffer.getRawBytes(data);
                return data;
            }
            finally
            {
                _buffer.rpos(initialPos); // Clean snapshot rollback
            }
        }

        public void SetRealClientAddressAndPort(string address, int port)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Parsed address layout cannot be blank.", nameof(address));

            InetSocketAddress realClientAddress = new InetSocketAddress(address, port);
            _session.setAttribute(ProxyAttributes.PROXY_REMOTE_ADDRESS, realClientAddress);
        }
    }
}
