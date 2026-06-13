// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Provides .NET-facing hooks for SFTP path resolution, filtering, attributes,
    /// and filesystem operations.
    /// </summary>
    /// <remarks>
    /// Implement this interface when the application needs full control over SFTP
    /// authorization and storage policy. Return values are already translated to
    /// .NET types; application code does not need Java or Apache MINA imports.
    /// </remarks>
    public interface IAMNetSftpFileSystemAccessor
    {
        /// <summary>
        /// Allows the application to rewrite the resolved local path for an SFTP request.
        /// </summary>
        /// <param name="context">Operation metadata, including remote and resolved local paths.</param>
        /// <param name="resolvedLocalPath">The default resolved local path.</param>
        /// <returns>The local path to use for the operation.</returns>
        string ResolveLocalFilePath(ISshFileSystemAccess context, string resolvedLocalPath) => resolvedLocalPath;
        /// <summary>
        /// Returns whether the requested SFTP path or operation is allowed.
        /// </summary>
        /// <param name="context">Operation metadata for the request being evaluated.</param>
        /// <returns><c>true</c> to allow the operation; otherwise <c>false</c>.</returns>
        bool IsPathAllowed(ISshFileSystemAccess context) => true;
        /// <summary>
        /// Allows the application to adjust link-following options used for file access.
        /// </summary>
        /// <param name="context">Operation metadata for the request.</param>
        /// <param name="resolvedOptions">The default option names.</param>
        /// <returns>The option names to apply.</returns>
        IReadOnlyList<string> ResolveFileAccessLinkOptions(ISshFileSystemAccess context, IReadOnlyList<string> resolvedOptions) => resolvedOptions;
        /// <summary>
        /// Allows the application to filter or rewrite attributes reported to clients.
        /// </summary>
        /// <param name="context">Operation metadata for the request.</param>
        /// <param name="resolvedAttributes">The default attributes.</param>
        /// <returns>The attributes to report to the client.</returns>
        IReadOnlyDictionary<string, object> ResolveReportedFileAttributes(ISshFileSystemAccess context, IReadOnlyDictionary<string, object> resolvedAttributes) => resolvedAttributes;
        /// <summary>Called when extension attributes are being applied to a file.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void ApplyExtensionFileAttributes(ISshFileSystemAccess context) { }
        /// <summary>Called when the server is preparing a remote filename response.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void PutRemoteFileName(ISshFileSystemAccess context) { }
        /// <summary>Called before or during SFTP file open handling.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void OpenFile(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to lock a file region.</summary>
        /// <param name="context">Operation metadata for the lock request.</param>
        void TryLock(ISshFileSystemAccess context) { }
        /// <summary>Called when a client requests file data synchronization.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void SyncFileData(ISshFileSystemAccess context) { }
        /// <summary>Called when an SFTP file handle is closed.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void CloseFile(ISshFileSystemAccess context) { }
        /// <summary>Called when an SFTP directory handle is opened.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void OpenDirectory(ISshFileSystemAccess context) { }
        /// <summary>Called when an SFTP directory handle is closed.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void CloseDirectory(ISshFileSystemAccess context) { }
        /// <summary>
        /// Returns whether a directory entry should be visible to the client.
        /// </summary>
        /// <param name="context">Operation metadata for the directory entry.</param>
        /// <returns><c>true</c> to include the entry; otherwise <c>false</c>.</returns>
        bool ShouldIncludeDirectoryEntry(ISshFileSystemAccess context) => true;
        /// <summary>
        /// Allows the application to filter or rewrite attributes read from a file.
        /// </summary>
        /// <param name="context">Operation metadata for the request.</param>
        /// <param name="resolvedAttributes">The default attributes.</param>
        /// <returns>The attributes to return to the client.</returns>
        IReadOnlyDictionary<string, object> ReadFileAttributes(ISshFileSystemAccess context, IReadOnlyDictionary<string, object> resolvedAttributes) => resolvedAttributes;
        /// <summary>Called when a client sets a file attribute.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void SetFileAttribute(ISshFileSystemAccess context) { }
        /// <summary>Called when file owner information is resolved.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void ResolveFileOwner(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to set file owner information.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void SetFileOwner(ISshFileSystemAccess context) { }
        /// <summary>Called when group owner information is resolved.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void ResolveGroupOwner(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to set group owner information.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void SetGroupOwner(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to set file permissions.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void SetFilePermissions(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to set file access control information.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void SetFileAccessControl(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to create a directory.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void CreateDirectory(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to create a hard link or symbolic link.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void CreateLink(ISshFileSystemAccess context) { }
        /// <summary>
        /// Allows the application to rewrite a symbolic link target reported to the client.
        /// </summary>
        /// <param name="context">Operation metadata for the request.</param>
        /// <param name="resolvedTarget">The default resolved link target.</param>
        /// <returns>The link target to report to the client.</returns>
        string ResolveLinkTarget(ISshFileSystemAccess context, string resolvedTarget) => resolvedTarget;
        /// <summary>Called when a client attempts to rename or move a file.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void RenameFile(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to copy a file.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void CopyFile(ISshFileSystemAccess context) { }
        /// <summary>Called when a client attempts to remove a file or directory.</summary>
        /// <param name="context">Operation metadata for the request.</param>
        void RemoveFile(ISshFileSystemAccess context) { }
        /// <summary>
        /// Returns whether symlinks should be treated as no-follow for the current request.
        /// </summary>
        /// <param name="context">Operation metadata for the request.</param>
        /// <param name="defaultNoFollow">The default no-follow decision.</param>
        /// <returns>The no-follow decision to use.</returns>
        bool NoFollow(ISshFileSystemAccess context, bool defaultNoFollow) => defaultNoFollow;
    }
}
