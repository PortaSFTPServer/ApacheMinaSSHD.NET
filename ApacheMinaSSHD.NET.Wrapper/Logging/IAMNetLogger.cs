// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Logging
{
    /// <summary>
    /// Minimal logger abstraction used by default wrapper listeners.
    /// </summary>
    public interface IAMNetLogger
    {
        /// <summary>Writes an informational log message.</summary>
        /// <param name="message">The message to write.</param>
        void Info(string message);
        /// <summary>Writes an error log message.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="ex">Optional exception details.</param>
        void Error(string message, Exception? ex = null);
        /// <summary>Writes a warning log message.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="ex">Optional exception details.</param>
        void Warn(string message, Exception? ex = null);
        /// <summary>Writes a debug log message.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="ex">Optional exception details.</param>
        void Debug(string message, Exception? ex = null);
        /// <summary>Writes a trace log message.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="ex">Optional exception details.</param>
        void Trace(string message, Exception? ex = null);
    }
}
