using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Represents an open SFTP directory handle.
    /// </summary>
    public interface ISshDirectoryHandle : IDisposable
    {
        /// <summary>Gets whether another directory entry is available.</summary>
        bool HasNext { get; }
        /// <summary>Gets whether the directory stream has completed.</summary>
        bool IsDone { get; }
        /// <summary>Gets whether the dot entry should still be sent.</summary>
        bool ShouldSendDot { get; }
        /// <summary>Gets whether the dot-dot entry should still be sent.</summary>
        bool ShouldSendDotDot { get; }
        /// <summary>Gets whether dot entries are included for this handle.</summary>
        bool IsWithDots { get; }
        /// <summary>Gets the local physical path associated with the handle.</summary>
        public string PhysicalPath { get; }
        /// <summary>Gets the next directory entry path as a string.</summary>
        string Next();
        /// <summary>Marks the directory stream as done.</summary>
        void MarkDone();
        /// <summary>Marks the dot entry as sent.</summary>
        void MarkDotSent();
        /// <summary>Marks the dot-dot entry as sent.</summary>
        void MarkDotDotSent();
        /// <summary>Removes the current directory entry when supported.</summary>
        void Remove();
        /// <summary>Closes the directory handle.</summary>
        void Close();
    }
}
