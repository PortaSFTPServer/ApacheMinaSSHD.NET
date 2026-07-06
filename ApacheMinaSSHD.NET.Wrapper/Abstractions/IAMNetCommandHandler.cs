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

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Defines a contract for handling SSH shell and exec requests from connected clients.
    /// Implement this interface to process remote command execution or interactive shell sessions.
    /// </summary>
    public interface IAMNetCommandHandler
    {
        /// <summary>
        /// Handles an "exec" request — a single remote command to be run.
        /// </summary>
        /// <param name="command">The command string received from the client.</param>
        /// <param name="session">Metadata for the current SSH session.</param>
        /// <returns>An exit code indicating the result of the command execution (0 for success).</returns>
        int ExecuteCommand(string command, ISshSession session);

        /// <summary>
        /// Handles a "shell" request — an interactive shell session.
        /// </summary>
        /// <param name="session">Metadata for the current SSH session.</param>
        /// <returns>An exit code indicating the result of the shell session (0 for success).</returns>
        int ExecuteShell(ISshSession session);
    }
}
