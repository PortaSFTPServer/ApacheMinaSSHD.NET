using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Logging
{
    /// <summary>
    /// Represents a UI-friendly log message with an optional display color.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Creates an empty log entry.
        /// </summary>
        public LogEntry()
        {
        }

        /// <summary>Gets or sets the log message text.</summary>
        public string Message = string.Empty;
        /// <summary>Gets or sets the preferred display color.</summary>
        public System.Drawing.Color Color = System.Drawing.Color.Empty;
    }
}
