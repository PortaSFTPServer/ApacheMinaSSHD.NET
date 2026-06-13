// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides access to connection metadata sent before the SSH handshake, such as PROXY protocol data.
    /// </summary>
    public interface IProxyMetadata
    {
        /// <summary>Returns a text header snapshot when one is available.</summary>
        string? GetHeader();
        /// <summary>Gets the currently known remote address.</summary>
        string RemoteAddress { get; }
        /// <summary>Gets the local server address for the connection.</summary>
        string LocalAddress { get; }
        /// <summary>Gets the number of bytes currently available in the pre-handshake buffer.</summary>
        int AvailableBytes { get; }
        /// <summary>Gets the current read position in the pre-handshake buffer.</summary>
        int CurrentReadPosition { get; }
        /// <summary>Reads the available pre-handshake bytes as text.</summary>
        string ReadRawString();
        /// <summary>Moves the read position in the pre-handshake buffer.</summary>
        /// <param name="pos">The absolute buffer position to move to.</param>
        void Seek(int pos);
        /// <summary>Gets the current remote host name when available.</summary>
        string GetHostname();
        /// <summary>Stores application metadata on the connection.</summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        void StoreAttribute(string key, object? value);
        /// <summary>Forces the connection to disconnect with a reason.</summary>
        /// <param name="reason">The disconnect reason.</param>
        void ForceDisconnect(string reason);
        /// <summary>Returns the raw pre-handshake bytes currently available.</summary>
        byte[] GetRawBytes();
        /// <summary>Overrides the client address and port after proxy metadata is validated.</summary>
        /// <param name="address">The real client address.</param>
        /// <param name="port">The real client source port.</param>
        void SetRealClientAddressAndPort(string address, int port);
    }
}
