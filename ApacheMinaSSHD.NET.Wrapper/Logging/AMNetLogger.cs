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

using org.slf4j;

namespace ApacheMinaSSHD.NET.Wrapper.Logging
{



    /// <summary>
    /// Default logger that writes through the SSH runtime logging backend.
    /// </summary>
    public class AMNetLogger : IAMNetLogger
    {

        private readonly Logger slf4JLogger;
        private readonly LogLevel _level;

        /// <summary>Global minimum log level across all wrapper loggers. Default <see cref="LogLevel.Info"/>.</summary>
        public static LogLevel GlobalLevel { get; set; } = LogLevel.Info;

        /// <summary>Optional callback invoked for every log message that passes level filtering.</summary>
        public static Action<LogLevel, string, Exception?>? LogEvent { get; set; }

        /// <summary>
        /// Log levels supported by <see cref="AMNetLogger"/>.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>No logging.</summary>
            Off,
            /// <summary>Error logging.</summary>
            Error,
            /// <summary>Warning logging.</summary>
            Warn,
            /// <summary>Informational logging.</summary>
            Info,
            /// <summary>Debug logging.</summary>
            Debug,
            /// <summary>Trace logging.</summary>
            Trace
        }

        private static readonly object slf4jLock = new();
        private static bool slf4jConfigured;

        private static void EnsureSlf4jConfigured()
        {
            if (slf4jConfigured)
            {
                return;
            }

            lock (slf4jLock)
            {
                if (slf4jConfigured)
                {
                    return;
                }

                // Set SLF4J Simple to the most verbose level so AMNetLogger's own
                // per-instance level filtering is the sole gate for wrapper-originated messages.
                // MINA SSHD internal logs also pass through at this level; consumers that
                // don't want them should set a root-level threshold when switching backends.
                java.lang.System.setProperty("org.slf4j.simpleLogger.defaultLogLevel", "trace");
                java.lang.System.setProperty("org.slf4j.simpleLogger.showDateTime", "true");
                java.lang.System.setProperty("org.slf4j.simpleLogger.dateTimeFormat", "yyyy-MM-dd HH:mm:ss.SSS |");
                java.lang.System.setProperty("org.slf4j.simpleLogger.showThreadName", "false");
                java.lang.System.setProperty("org.slf4j.simpleLogger.showThreadId", "false");

                slf4jConfigured = true;
            }
        }

        /// <summary>
        /// Creates a logger for the supplied source type.
        /// </summary>
        /// <param name="type">The source type used as the logger name.</param>
        /// <param name="logLevel">The minimum log level for this logger instance.</param>
        public AMNetLogger(Type type, LogLevel logLevel = LogLevel.Info)
        {
            EnsureSlf4jConfigured();

            slf4JLogger = LoggerFactory.getLogger(type.FullName);
            _level = logLevel;
        }

        private bool IsEnabled(LogLevel level) => level <= GlobalLevel;

        /// <inheritdoc />
        public void Info(string message)
        {
            if (IsEnabled(LogLevel.Info))
            {
                slf4JLogger.info(message);
                LogEvent?.Invoke(LogLevel.Info, message, null);
            }
        }

        /// <inheritdoc />
        public void Error(string message, Exception? ex = null)
        {
            if (IsEnabled(LogLevel.Error))
            {
                if (ex == null)
                {
                    slf4JLogger.error(message);
                }
                else
                {
                    slf4JLogger.error(message, ikvm.runtime.Util.mapException(ex));
                }
                LogEvent?.Invoke(LogLevel.Error, message, ex);
            }
        }

        /// <inheritdoc />
        public void Warn(string message, Exception? ex = null)
        {
            if (IsEnabled(LogLevel.Warn))
            {
                if (ex == null)
                {
                    slf4JLogger.warn(message);
                }
                else
                {
                    slf4JLogger.warn(message, ikvm.runtime.Util.mapException(ex));
                }
                LogEvent?.Invoke(LogLevel.Warn, message, ex);
            }
        }

        /// <inheritdoc />
        public void Debug(string message, Exception? ex = null)
        {
            if (IsEnabled(LogLevel.Debug))
            {
                if (ex == null)
                {
                    slf4JLogger.debug(message);
                }
                else
                {
                    slf4JLogger.debug(message, ikvm.runtime.Util.mapException(ex));
                }
                LogEvent?.Invoke(LogLevel.Debug, message, ex);
            }
        }

        /// <inheritdoc />
        public void Trace(string message, Exception? ex = null)
        {
            if (IsEnabled(LogLevel.Trace))
            {
                if (ex == null)
                {
                    slf4JLogger.trace(message);
                }
                else
                {
                    slf4JLogger.trace(message, ikvm.runtime.Util.mapException(ex));
                }
                LogEvent?.Invoke(LogLevel.Trace, message, ex);
            }
        }

    }
}
