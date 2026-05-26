namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Identifies the SFTP filesystem operation being evaluated.
    /// </summary>
    public enum SshFileSystemOperation
    {
        /// <summary>Resolve a remote SFTP path to a local filesystem path.</summary>
        ResolveLocalFilePath,
        /// <summary>Resolve link-following options for file access.</summary>
        ResolveFileAccessLinkOptions,
        /// <summary>Resolve attributes reported to the client.</summary>
        ResolveReportedFileAttributes,
        /// <summary>Apply extension-provided file attributes.</summary>
        ApplyExtensionFileAttributes,
        /// <summary>Prepare a remote file name response.</summary>
        PutRemoteFileName,
        /// <summary>Open a file handle.</summary>
        OpenFile,
        /// <summary>Attempt to lock a file region.</summary>
        TryLock,
        /// <summary>Synchronize file data.</summary>
        SyncFileData,
        /// <summary>Close a file handle.</summary>
        CloseFile,
        /// <summary>Open a directory handle.</summary>
        OpenDirectory,
        /// <summary>Close a directory handle.</summary>
        CloseDirectory,
        /// <summary>Evaluate a directory entry before returning it to the client.</summary>
        DirectoryEntry,
        /// <summary>Read file attributes.</summary>
        ReadFileAttributes,
        /// <summary>Set a file attribute.</summary>
        SetFileAttribute,
        /// <summary>Resolve a file owner.</summary>
        ResolveFileOwner,
        /// <summary>Set a file owner.</summary>
        SetFileOwner,
        /// <summary>Resolve a group owner.</summary>
        ResolveGroupOwner,
        /// <summary>Set a group owner.</summary>
        SetGroupOwner,
        /// <summary>Set file permissions.</summary>
        SetFilePermissions,
        /// <summary>Set file access control information.</summary>
        SetFileAccessControl,
        /// <summary>Create a directory.</summary>
        CreateDirectory,
        /// <summary>Create a hard link or symbolic link.</summary>
        CreateLink,
        /// <summary>Resolve a symbolic link target.</summary>
        ResolveLinkTarget,
        /// <summary>Rename or move a file.</summary>
        RenameFile,
        /// <summary>Copy a file.</summary>
        CopyFile,
        /// <summary>Remove a file or directory.</summary>
        RemoveFile,
        /// <summary>Evaluate whether links should be treated as no-follow.</summary>
        NoFollow
    }

    /// <summary>
    /// Provides metadata for SFTP filesystem policy callbacks.
    /// </summary>
    public interface ISshFileSystemAccess
    {
        /// <summary>Gets the filesystem operation currently being evaluated.</summary>
        SshFileSystemOperation Operation { get; }
        /// <summary>Gets the session associated with the operation when available.</summary>
        ISshSession? Session { get; }
        /// <summary>Gets the configured filesystem root path when available.</summary>
        string? RootPath { get; }
        /// <summary>Gets the client-requested remote path when available.</summary>
        string? RemotePath { get; }
        /// <summary>Gets the resolved local path when available.</summary>
        string? LocalPath { get; }
        /// <summary>Gets the operation source path when available.</summary>
        string? SourcePath { get; }
        /// <summary>Gets the operation destination path when available.</summary>
        string? DestinationPath { get; }
        /// <summary>Gets the remote handle identifier when available.</summary>
        string? RemoteHandle { get; }
        /// <summary>Gets the remote name being reported or manipulated when available.</summary>
        string? RemoteName { get; }
        /// <summary>Gets the SFTP extension name when available.</summary>
        string? Extension { get; }
        /// <summary>Gets the file attribute view name when available.</summary>
        string? FileAttributeView { get; }
        /// <summary>Gets the file attribute name when available.</summary>
        string? FileAttributeName { get; }
        /// <summary>Gets the owner name when available.</summary>
        string? Owner { get; }
        /// <summary>Gets the group name when available.</summary>
        string? Group { get; }
        /// <summary>Gets an operation-specific value when available.</summary>
        object? Value { get; }
        /// <summary>Gets whether the current target is a directory.</summary>
        bool IsDirectory { get; }
        /// <summary>Gets whether the current target is a symbolic link.</summary>
        bool IsSymbolicLink { get; }
        /// <summary>Gets whether short-name reporting was requested.</summary>
        bool ShortName { get; }
        /// <summary>Gets whether links should be followed for the current operation.</summary>
        bool FollowLinks { get; }
        /// <summary>Gets whether a requested lock is shared.</summary>
        bool SharedLock { get; }
        /// <summary>Gets an operation-specific command value when available.</summary>
        int Command { get; }
        /// <summary>Gets an operation-specific file offset when available.</summary>
        long Offset { get; }
        /// <summary>Gets an operation-specific length when available.</summary>
        long Length { get; }
        /// <summary>Gets option names associated with the operation.</summary>
        IReadOnlyList<string> Options { get; }
        /// <summary>Gets attributes associated with the operation.</summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
    }
}
