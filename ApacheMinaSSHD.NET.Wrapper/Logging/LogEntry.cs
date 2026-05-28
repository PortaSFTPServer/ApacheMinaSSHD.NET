namespace ApacheMinaSSHD.NET.Wrapper.Logging
{
    /// <summary>
    /// Represents a UI-friendly log message with an optional display color name.
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

        /// <summary>Gets or sets the preferred display color name (e.g. "Red", "Green").</summary>
        public string? ColorName { get; set; }
    }
}
