// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using org.slf4j;

namespace ApacheMinaSSHD.NET.Wrapper.Logging
{



    /// <summary>
    /// Default logger that writes through the SSH runtime logging backend.
    /// </summary>
    public class AMNetLogger : IAMNetLogger
    {

        private readonly Logger slf4JLogger;

        /// <summary>
        /// Log levels supported by <see cref="AMNetLogger"/>.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>Informational logging.</summary>
            Info,
            /// <summary>Warning logging.</summary>
            Warn,
            /// <summary>Error logging.</summary>
            Error,
            /// <summary>Debug logging.</summary>
            Debug,
            /// <summary>Trace logging.</summary>
            Trace
        }

        private static readonly object slf4jLock = new();
        private static bool slf4jConfigured;

        private static void EnsureSlf4jConfigured(LogLevel logLevel)
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

                java.lang.System.setProperty("org.slf4j.simpleLogger.defaultLogLevel", logLevel.ToString().ToLowerInvariant());
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
        /// <param name="logLevel">The default log level.</param>
        public AMNetLogger(Type type, LogLevel logLevel = LogLevel.Info)
        {
            EnsureSlf4jConfigured(logLevel);

            slf4JLogger = LoggerFactory.getLogger(type.FullName);

            // Force load the binding assembly to prevent "Failed to load class"
            //_ = org.slf4j.impl.StaticLoggerBinder.getSingleton();

        }
        /// <inheritdoc />
        public void Info(string message)
        {
            slf4JLogger.info(message);
        }

        /// <inheritdoc />
        public void Error(string message, Exception? ex = null)
        {
            if (ex == null)
            {
                slf4JLogger.error(message);
            }
            else
            {
                // IKVM allows passing .NET Exceptions directly to Java methods in many cases
                slf4JLogger.error(message, ikvm.runtime.Util.mapException(ex));
            }
        }

        /// <inheritdoc />
        public void Warn(string message, Exception? ex = null)
        {
            if (ex == null)
            {
                slf4JLogger.warn(message);
            }
            else
            {
                // IKVM allows passing .NET Exceptions directly to Java methods in many cases
                slf4JLogger.warn(message, ikvm.runtime.Util.mapException(ex));
            }
        }

        /// <inheritdoc />
        public void Debug(string message, Exception? ex = null)
        {
            if (ex == null)
            {
                slf4JLogger.debug(message);
            }
            else
            {
                slf4JLogger.debug(message, ikvm.runtime.Util.mapException(ex));
            }
        }

        /// <inheritdoc />
        public void Trace(string message, Exception? ex = null)
        {
            if (ex == null)
            {
                slf4JLogger.trace(message);
            }
            else
            {
                slf4JLogger.trace(message, ikvm.runtime.Util.mapException(ex));
            }
        }

    }
}
