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
    /// Provides safe session metadata for authentication, event, and file operation callbacks.
    /// </summary>
    public interface ISshSession
    {
        /// <summary>Gets the remote client address.</summary>
        string RemoteAddress { get; }
        /// <summary>Gets the unique session identifier assigned by the wrapper.</summary>
        Guid SessionId { get; }
        /// <summary>Gets the negotiated session cipher when available.</summary>
        string? SessionCipher => null;
        /// <summary>Gets the SSH algorithm identifier (e.g., "ssh-rsa", "ssh-ed25519") when available.</summary>
        string? KeyAlgorithmId => null;
        /// <summary>Gets the key algorithm name (e.g., "RSA", "ECDSA") when available.</summary>
        string? KeyAlgorithm => null;
        /// <summary>Gets the key size in bits when available.</summary>
        int KeySize => 0;
        /// <summary>Gets the client software version string (e.g., "SSH-2.0-OpenSSH_8.9p1").</summary>
        string? ClientSoftwareIdentifier => null;
        /// <summary>Forces the session to disconnect immediately.</summary>
        void Disconnect();
    }
}
