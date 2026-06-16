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

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal sealed class SuppressShutdownExceptionHandler : java.lang.Object, java.lang.Thread.UncaughtExceptionHandler
    {
        private readonly java.lang.Thread.UncaughtExceptionHandler? _previous;

        public SuppressShutdownExceptionHandler(java.lang.Thread.UncaughtExceptionHandler? previous)
        {
            _previous = previous;
        }

        public void uncaughtException(java.lang.Thread t, System.Exception e)
        {
            if (e is java.lang.IllegalStateException ise
                && ise.getMessage() is string msg
                && msg.IndexOf("shut down", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AMNetSshServer] Suppressed uncaught exception on thread '{t?.getName()}': {msg}");
                return;
            }

            _previous?.uncaughtException(t, e);
        }
    }
}
