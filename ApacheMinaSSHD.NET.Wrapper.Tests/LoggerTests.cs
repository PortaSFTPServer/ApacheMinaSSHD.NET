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

using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class LoggerTests
{
    [Fact]
    public void Create_logger_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests));
        Assert.NotNull(logger);
    }

    [Fact]
    public void LogInfo_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests));
        logger.Info("test info message");
    }

    [Fact]
    public void LogError_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests));
        logger.Error("test error message");
    }

    [Fact]
    public void LogError_with_exception_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests));
        logger.Error("test error", new InvalidOperationException("test"));
    }

    [Fact]
    public void LogWarn_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests));
        logger.Warn("test warn message");
    }

    [Fact]
    public void LogWarn_with_exception_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests));
        logger.Warn("test warn", new InvalidOperationException("test"));
    }

    [Fact]
    public void LogDebug_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests), AMNetLogger.LogLevel.Debug);
        logger.Debug("test debug message");
    }

    [Fact]
    public void LogDebug_with_exception_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests), AMNetLogger.LogLevel.Debug);
        logger.Debug("test debug", new InvalidOperationException("test"));
    }

    [Fact]
    public void LogTrace_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests), AMNetLogger.LogLevel.Trace);
        logger.Trace("test trace message");
    }

    [Fact]
    public void LogTrace_with_exception_does_not_throw()
    {
        var logger = new AMNetLogger(typeof(LoggerTests), AMNetLogger.LogLevel.Trace);
        logger.Trace("test trace", new InvalidOperationException("test"));
    }

    [Fact]
    public void Create_AMNetOutputStream_does_not_throw()
    {
        var stream = new AMNetOutputStream();
        Assert.NotNull(stream);
    }

    [Fact]
    public void RedirectStandardError_does_not_throw()
    {
        var stream = new AMNetOutputStream();
        stream.RedirectStandardError();
    }

    [Fact]
    public void RedirectStandardError_with_callback_does_not_throw()
    {
        var lines = new List<string>();
        var stream = new AMNetOutputStream(msg => lines.Add(msg));
        stream.RedirectStandardError();
    }
}
