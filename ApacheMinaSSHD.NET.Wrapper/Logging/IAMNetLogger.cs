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
