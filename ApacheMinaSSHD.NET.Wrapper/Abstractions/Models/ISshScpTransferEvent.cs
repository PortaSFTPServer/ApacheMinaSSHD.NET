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

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides metadata for an SCP transfer event.
    /// </summary>
    public interface ISshScpTransferEvent
    {
        /// <summary>Gets the session associated with the transfer.</summary>
        ISshSession Session { get; }

        /// <summary>Gets the transfer operation name.</summary>
        string Operation { get; }

        /// <summary>Gets the local or remote path associated with the transfer.</summary>
        string Path { get; }

        /// <summary>Gets the file length when available.</summary>
        long Length { get; }

        /// <summary>Gets permission names associated with the transfer.</summary>
        IReadOnlyList<string> Permissions { get; }

        /// <summary>Gets the SCP acknowledgement status code when available.</summary>
        int? AckStatusCode { get; }

        /// <summary>Gets the SCP acknowledgement line when available.</summary>
        string? AckLine { get; }

        /// <summary>Gets the SCP command text when available.</summary>
        string? Command { get; }

        /// <summary>Gets the exception associated with the transfer when available.</summary>
        Exception? Exception { get; }
    }
}
