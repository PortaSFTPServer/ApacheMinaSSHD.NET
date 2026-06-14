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
    /// Provides .NET-facing hooks for SCP path resolution, filtering, metadata,
    /// and stream lifecycle events.
    /// </summary>
    /// <remarks>
    /// Implement this interface to enforce application-specific SCP authorization
    /// and storage rules without importing Apache MINA or Java types.
    /// </remarks>
    public interface IAMNetScpFileOpener
    {
        /// <summary>
        /// Allows the application to rewrite a client path into a local path.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="resolvedPath">The default resolved path.</param>
        /// <returns>The local path to use.</returns>
        string ResolveLocalPath(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <summary>
        /// Allows the application to rewrite the destination path for an incoming file.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="resolvedPath">The default resolved path.</param>
        /// <returns>The local path to use for the incoming file.</returns>
        string ResolveIncomingFilePath(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <summary>
        /// Allows the application to rewrite the receive location for incoming SCP data.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="resolvedPath">The default resolved path.</param>
        /// <returns>The local receive location to use.</returns>
        string ResolveIncomingReceiveLocation(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <summary>
        /// Allows the application to rewrite the source path for an outgoing file.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="resolvedPath">The default resolved path.</param>
        /// <returns>The local path to send.</returns>
        string ResolveOutgoingFilePath(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <summary>
        /// Filters or rewrites files that match an outgoing SCP pattern.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="resolvedPaths">The default matching local paths.</param>
        /// <returns>The local paths that may be sent.</returns>
        IReadOnlyList<string> GetMatchingFilesToSend(
            ISshScpFileAccess access,
            IReadOnlyList<string> resolvedPaths) => resolvedPaths;

        /// <summary>
        /// Returns whether the requested SCP path or operation is allowed.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <returns><c>true</c> to allow the operation; otherwise <c>false</c>.</returns>
        bool IsPathAllowed(ISshScpFileAccess access) => true;

        /// <summary>
        /// Returns whether the path should be sent as a regular file.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="defaultValue">The default server decision.</param>
        /// <returns>The decision to use.</returns>
        bool ShouldSendAsRegularFile(ISshScpFileAccess access, bool defaultValue) => defaultValue;

        /// <summary>
        /// Returns whether the path should be sent as a directory.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="defaultValue">The default server decision.</param>
        /// <returns>The decision to use.</returns>
        bool ShouldSendAsDirectory(ISshScpFileAccess access, bool defaultValue) => defaultValue;

        /// <summary>
        /// Returns whether a directory entry should be visible during recursive SCP.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <returns><c>true</c> to include the entry; otherwise <c>false</c>.</returns>
        bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access) => true;

        /// <summary>
        /// Allows the application to filter or rewrite file attributes reported to SCP clients.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="attributes">The default attributes.</param>
        /// <returns>The attributes to use.</returns>
        IReadOnlyDictionary<string, object> ReadLocalBasicFileAttributes(
            ISshScpFileAccess access,
            IReadOnlyDictionary<string, object> attributes) => attributes;

        /// <summary>
        /// Allows the application to filter or rewrite local file permissions reported to SCP clients.
        /// </summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        /// <param name="permissions">The default permission names.</param>
        /// <returns>The permission names to use.</returns>
        IReadOnlyList<string> GetLocalFilePermissions(
            ISshScpFileAccess access,
            IReadOnlyList<string> permissions) => permissions;

        /// <summary>Called before a local file is opened for SCP read/send.</summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        void OpenRead(ISshScpFileAccess access) { }

        /// <summary>Called after a local file read/send handle is closed.</summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        void CloseRead(ISshScpFileAccess access) { }

        /// <summary>Called before a local file is opened for SCP write/receive.</summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        void OpenWrite(ISshScpFileAccess access) { }

        /// <summary>Called after a local file write/receive handle is closed.</summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        void CloseWrite(ISshScpFileAccess access) { }

        /// <summary>Called when the server creates an outgoing SCP source stream resolver.</summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        void CreateSourceStreamResolver(ISshScpFileAccess access) { }

        /// <summary>Called when the server creates an incoming SCP target stream resolver.</summary>
        /// <param name="access">Operation metadata for the SCP request.</param>
        void CreateTargetStreamResolver(ISshScpFileAccess access) { }
    }
}
