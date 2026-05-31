using ApacheMinaSSHD.NET.Wrapper.Logging;
using System.Collections.Concurrent;
using System.Drawing; // Make sure this is included for Color
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms; // Make sure this is included for RichTextBox, MethodInvoker, Timer

namespace SimpleSSHDSever
{
    public class SshdLoggerStream
    {
        private readonly RichTextBox _box;
        private readonly AMNetOutputStream _outputStream;
        private readonly ConcurrentQueue<LogEntry> _logQueue = new();
        private readonly System.Windows.Forms.Timer _flushTimer;
        private bool _isMufflingNoise = false;

        public SshdLoggerStream(RichTextBox box)
        {
            _box = box ?? throw new ArgumentNullException(nameof(box)); // Added defensive check
            _box.ReadOnly = true;
            _box.WordWrap = false; // Mandatory for performance
            _box.DetectUrls = false;

            _outputStream = new AMNetOutputStream(line => ParseToQueue(line + Environment.NewLine));
            _outputStream.RedirectStandardError();

            // Timer-based batching (Drains the queue every 200ms)
            _flushTimer = new System.Windows.Forms.Timer { Interval = 200 };
            _flushTimer.Tick += (s, e) => FlushToRtf();
            _flushTimer.Start();
        }


        private void ParseToQueue(string raw)
        {
            var lines = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string cleanLine = line.Trim();

                // Detect the start of the harmless NIO2 shutdown error
                if (cleanLine.Contains("java.lang.IllegalStateException: Executor has been shut down"))
                {
                    _isMufflingNoise = true;
                    continue;
                }

                if (_isMufflingNoise)
                {
                    if (cleanLine.StartsWith("at ") || cleanLine.Contains("org.apache.sshd") || cleanLine.Contains("sun.nio"))
                    {
                        continue;
                    }
                    else
                    {
                        _isMufflingNoise = false;
                    }
                }

                _logQueue.Enqueue(new LogEntry
                {
                    Message = line.TrimEnd('\r'),
                    Color = GetColorFast(line)
                });
            }
        }



        private void FlushToRtf()
        {
            // Defensive check if the RichTextBox has been disposed
            if (_box.IsDisposed)
            {
                _flushTimer.Stop(); // Stop timer if the box is gone
                return;
            }

            if (_logQueue.IsEmpty) return;

            StringBuilder rtfBuilder = new StringBuilder();
            while (_logQueue.TryDequeue(out var entry))
            {
                string colorIndex = GetRtfColorIndex(entry.Color);
                rtfBuilder.Append($@"\cf{colorIndex} {EscapeRtf(entry.Message)}\line ");
            }

            // Perform UI update on the main thread
            if (_box.IsDisposed) return; // Final check before BeginInvoke

            _box.BeginInvoke((MethodInvoker)delegate {
                if (_box.IsDisposed) return; // Final check inside BeginInvoke

                SendMessage(_box.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                try
                {
                    // Truncate old logs if history is too large ---
                    // This is the problematic part from your ORIGINAL code.
                    // We need to change this because it's a major source of RTF corruption.
                    // The safest way is to clear and add a new message.
                    if (_box.TextLength > 100000)
                    {
                        _box.Clear(); // Clears all existing RTF content cleanly
                        _box.AppendText("[... History Truncated ...]" + Environment.NewLine); // Add plain text message
                    }

                    _box.SelectionStart = _box.TextLength;
                    _box.SelectionLength = 0; // Ensure nothing is selected

                    // Updated \colortbl to include all used colors ---
                    // This is the primary fix for "Cannot load the text" when using Color.IndianRed.
                    _box.SelectedRtf = $@"{{\rtf1\ansi\deff0{{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;\red255\green165\blue0;\red128\green128\blue128;\red205\green92\blue92;\red0\green0\blue139;}}{rtfBuilder}}}";

                    _box.ScrollToCaret();
                }
                finally
                {
                    SendMessage(_box.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                    _box.Invalidate();
                }
            });
        }

        // --- GetRtfColorIndex aligned with the new \colortbl ---
        private string GetRtfColorIndex(Color c)
        {
            if (c == Color.Red) return "2";        // ERROR
            if (c == Color.OrangeRed) return "3";  // WARN
            if (c == Color.LightSlateGray) return "4";   // DEBUG
            if (c == Color.IndianRed) return "5";  // TRACE
            if (c == Color.DarkBlue) return "6";   // ADD THIS LINE FOR DarkBlue/INFO
            return "1";                            // INFO (Black/Default) - this would be a fallback
        }
        private string EscapeRtf(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            StringBuilder sb = new StringBuilder(text.Length + 10); // Pre-allocate with some buffer
            foreach (char c in text)
            {
                if (c == '\\')
                    sb.Append(@"\\");
                else if (c == '{')
                    sb.Append(@"\{");
                else if (c == '}')
                    sb.Append(@"\}");
                else if (c == '\0') // Handle null bytes
                    sb.Append("[NUL]"); // Or continue; to remove them
                                        // Optional: Handle other control characters like tabs, but `\line` covers newlines.
                                        // else if (c == '\t') sb.Append(@"\tab");
                                        // else if (c < 32) sb.AppendFormat("\\'{0:x2}", (int)c); // RTF hex escape for other control chars
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
        private Color GetColorFast(string line)
        {
            if (line.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0) return Color.Red;
            if (line.IndexOf("WARN", StringComparison.OrdinalIgnoreCase) >= 0) return Color.OrangeRed;
            if (line.IndexOf("DEBUG", StringComparison.OrdinalIgnoreCase) >= 0) return Color.LightSlateGray;
            if (line.IndexOf("TRACE", StringComparison.OrdinalIgnoreCase) >= 0) return Color.IndianRed;
            return Color.DarkBlue; //info
        }

        #region Win32 API
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0x000B;
        #endregion

        private class LogEntry
        {
            public string Message { get; set; } = string.Empty;
            public Color Color { get; set; }
        }
    }
}
