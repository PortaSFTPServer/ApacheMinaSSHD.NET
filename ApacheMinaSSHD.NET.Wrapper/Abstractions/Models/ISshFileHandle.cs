// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Represents an open SFTP file handle for direct data operations.
    /// </summary>
    public interface ISshFileHandle
    {
        /// <summary>
        /// Gets the SFTP access mask used to open the file.
        /// </summary>
        int AccessMask { get; }
        /// <summary>Gets whether the file is open in append mode.</summary>
        bool IsOpenAppend { get; }
        /// <summary>Gets file attributes captured for the handle.</summary>
        IReadOnlyDictionary<string, object> Attributes { get; }

        /// <summary>
        /// Appends data to the file.
        /// </summary>
        /// <param name="data">The source buffer.</param>
        /// <param name="offset">The source buffer offset.</param>
        /// <param name="length">The number of bytes to append, or <c>null</c> for the remaining buffer.</param>
        void Append(byte[] data, int offset = 0, int? length = null);
        /// <summary>Reads data from the file.</summary>
        /// <param name="data">The destination buffer.</param>
        /// <param name="fileOffset">The file offset to read from.</param>
        /// <param name="dataOffset">The destination buffer offset.</param>
        /// <param name="length">The number of bytes to read, or <c>null</c> for the remaining buffer.</param>
        /// <returns>The number of bytes read.</returns>
        int Read(byte[] data, long fileOffset, int dataOffset = 0, int? length = null);
        /// <summary>Reads data from the file and reports end-of-file state.</summary>
        /// <param name="data">The destination buffer.</param>
        /// <param name="fileOffset">The file offset to read from.</param>
        /// <param name="isEof">Set to <c>true</c> when the read reaches end-of-file.</param>
        /// <param name="dataOffset">The destination buffer offset.</param>
        /// <param name="length">The number of bytes to read, or <c>null</c> for the remaining buffer.</param>
        /// <returns>The number of bytes read.</returns>
        int Read(byte[] data, long fileOffset, out bool isEof, int dataOffset = 0, int? length = null);
        /// <summary>Writes data to the file.</summary>
        /// <param name="data">The source buffer.</param>
        /// <param name="fileOffset">The file offset to write to.</param>
        /// <param name="dataOffset">The source buffer offset.</param>
        /// <param name="length">The number of bytes to write, or <c>null</c> for the remaining buffer.</param>
        void Write(byte[] data, long fileOffset, int dataOffset = 0, int? length = null);

        /// <summary>
        /// Locks a file region.
        /// </summary>
        /// <param name="offset">The file region offset.</param>
        /// <param name="length">The file region length.</param>
        /// <param name="mask">The lock mask supplied by the client.</param>
        void Lock(long offset, long length, int mask);
        /// <summary>Unlocks a file region.</summary>
        /// <param name="offset">The file region offset.</param>
        /// <param name="length">The file region length.</param>
        void Unlock(long offset, long length);

        /// <summary>Gets the underlying safe handle metadata.</summary>
        ISshHandle SshHandle { get; }

        /// <summary>
        /// Returns a .NET stream wrapper over this file handle.
        /// </summary>
        /// <returns>A stream backed by this SFTP file handle.</returns>
        Stream AsStream();
        /// <summary>Closes the file handle.</summary>
        void Close();
    }
}
