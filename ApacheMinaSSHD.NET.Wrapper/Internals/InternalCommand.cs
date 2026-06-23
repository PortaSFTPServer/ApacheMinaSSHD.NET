using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.io;
using org.apache.sshd.server.channel;
using org.apache.sshd.server.command;
using org.apache.sshd.server.session;
using SshdEnvironment = org.apache.sshd.server.Environment;
using SshdExitCallback = org.apache.sshd.server.ExitCallback;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalCommand : java.lang.Object, Command
    {
        private readonly string? _commandString;
        private readonly IAMNetCommandHandler _handler;
        private InputStream? _inputStream;
        private OutputStream? _outputStream;
        private OutputStream? _errorStream;
        private SshdExitCallback? _exitCallback;
        private ServerSession? _session;
        private bool _started;

        public InternalCommand(string? commandString, IAMNetCommandHandler handler)
        {
            _commandString = commandString;
            _handler = handler;
        }

        public void setInputStream(InputStream inputStream)
        {
            _inputStream = inputStream;
        }

        public void setOutputStream(OutputStream outputStream)
        {
            _outputStream = outputStream;
        }

        public void setErrorStream(OutputStream errorStream)
        {
            _errorStream = errorStream;
        }

        public void setExitCallback(SshdExitCallback callback)
        {
            _exitCallback = callback;
        }

        public void start(ChannelSession channelSession, SshdEnvironment env)
        {
            if (_started) return;
            _started = true;
            _session = channelSession.getSession();

            var thread = new Thread(() =>
            {
                try
                {
                    var sshSession = new SshSession(_session);
                    int exitCode;
                    if (!string.IsNullOrEmpty(_commandString))
                    {
                        exitCode = _handler.ExecuteCommand(_commandString, sshSession);
                    }
                    else
                    {
                        exitCode = _handler.ExecuteShell(sshSession);
                    }
                    _exitCallback?.onExit(exitCode);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[InternalCommand] Error: {ex.Message}");
                    _exitCallback?.onExit(1, ex.Message);
                }
            });
            thread.Name = "command-" + (_commandString ?? "shell");
            thread.IsBackground = true;
            thread.Start();
        }

        public void destroy(ChannelSession channelSession)
        {
            _started = false;
            try
            {
                _inputStream?.close();
            }
            catch { }
            try
            {
                _outputStream?.close();
            }
            catch { }
            try
            {
                _errorStream?.close();
            }
            catch { }
        }
    }
}
