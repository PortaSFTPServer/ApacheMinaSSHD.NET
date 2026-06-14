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
    /// Identifies the SCP file operation being evaluated.
    /// </summary>
    public enum SshScpFileOperation
    {
        /// <summary>Resolve a client SCP path to a local path.</summary>
        ResolveLocalPath,
        /// <summary>Resolve the local path for an incoming file.</summary>
        ResolveIncomingFilePath,
        /// <summary>Resolve the receive location for incoming SCP data.</summary>
        ResolveIncomingReceiveLocation,
        /// <summary>Resolve the local path for an outgoing file.</summary>
        ResolveOutgoingFilePath,
        /// <summary>Resolve files that match an outgoing SCP pattern.</summary>
        GetMatchingFilesToSend,
        /// <summary>Evaluate whether a path should be sent as a regular file.</summary>
        SendAsRegularFile,
        /// <summary>Evaluate whether a path should be sent as a directory.</summary>
        SendAsDirectory,
        /// <summary>Evaluate local folder children for recursive SCP.</summary>
        GetLocalFolderChildren,
        /// <summary>Read local basic file attributes.</summary>
        GetLocalBasicFileAttributes,
        /// <summary>Read local file permissions.</summary>
        GetLocalFilePermissions,
        /// <summary>Open a file for SCP read/send.</summary>
        OpenRead,
        /// <summary>Close a file after SCP read/send.</summary>
        CloseRead,
        /// <summary>Open a file for SCP write/receive.</summary>
        OpenWrite,
        /// <summary>Close a file after SCP write/receive.</summary>
        CloseWrite,
        /// <summary>Create an outgoing SCP source stream resolver.</summary>
        CreateSourceStreamResolver,
        /// <summary>Create an incoming SCP target stream resolver.</summary>
        CreateTargetStreamResolver
    }

    /// <summary>
    /// Provides metadata for SCP filesystem policy callbacks.
    /// </summary>
    public interface ISshScpFileAccess
    {
        /// <summary>Gets the SCP operation currently being evaluated.</summary>
        SshScpFileOperation Operation { get; }
        /// <summary>Gets the session associated with the operation when available.</summary>
        ISshSession? Session { get; }
        /// <summary>Gets the configured SCP root path when available.</summary>
        string? RootPath { get; }
        /// <summary>Gets the resolved local path when available.</summary>
        string? LocalPath { get; }
        /// <summary>Gets the client-requested path when available.</summary>
        string? RequestedPath { get; }
        /// <summary>Gets the file name associated with the operation when available.</summary>
        string? FileName { get; }
        /// <summary>Gets the outgoing file match pattern when available.</summary>
        string? Pattern { get; }
        /// <summary>Gets the SCP command text when available.</summary>
        string? Command { get; }
        /// <summary>Gets whether the SCP command is recursive.</summary>
        bool Recursive { get; }
        /// <summary>Gets whether the target should be a directory.</summary>
        bool ShouldBeDirectory { get; }
        /// <summary>Gets whether timestamps should be preserved.</summary>
        bool PreserveTimestamp { get; }
        /// <summary>Gets whether the target is a directory.</summary>
        bool IsDirectory { get; }
        /// <summary>Gets the file length when available.</summary>
        long Length { get; }
        /// <summary>Gets permission names associated with the operation.</summary>
        IReadOnlyList<string> Permissions { get; }
        /// <summary>Gets option names associated with the operation.</summary>
        IReadOnlyList<string> Options { get; }
        /// <summary>Gets attributes associated with the operation.</summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
    }
}
