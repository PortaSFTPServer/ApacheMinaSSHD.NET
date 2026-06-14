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

using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Logging
{
    /// <summary>
    /// Default output sink for redirecting Java stderr into .NET code.
    /// </summary>
    public class AMNetOutputStream
    {
        private readonly Action<string> writeLine;

        /// <summary>
        /// Creates a stderr redirector.
        /// </summary>
        /// <param name="writeLine">Optional callback that receives each redirected line.</param>
        public AMNetOutputStream(Action<string>? writeLine = null)
        {
            this.writeLine = writeLine ?? Console.Error.WriteLine;
        }

        /// <summary>
        /// Redirects the SSH runtime standard error stream to the configured callback.
        /// </summary>
        public void RedirectStandardError()
        {
            java.lang.System.setErr(new java.io.PrintStream(new InternalOutputStream(writeLine), true));
        }

        private sealed class InternalOutputStream : java.io.OutputStream
        {
            private readonly Action<string> writeLine;
            private readonly StringBuilder buffer = new();

            public InternalOutputStream(Action<string> writeLine)
            {
                this.writeLine = writeLine;
            }

            public override void write(int b)
            {
                var current = (char)b;
                if (current == '\n')
                {
                    FlushBuffer();
                    return;
                }

                if (current != '\r')
                {
                    buffer.Append(current);
                }
            }

            public override void flush()
            {
                FlushBuffer();
            }

            private void FlushBuffer()
            {
                if (buffer.Length == 0)
                {
                    return;
                }

                writeLine(buffer.ToString());
                buffer.Clear();
            }
        }
    }
}
