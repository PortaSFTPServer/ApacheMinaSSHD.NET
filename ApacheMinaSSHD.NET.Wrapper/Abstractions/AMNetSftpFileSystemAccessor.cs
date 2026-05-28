using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default .NET-facing SFTP filesystem policy hook. Override this class to implement
    /// application-specific path resolution, filtering, and filesystem operation
    /// validation without using Apache MINA or Java types in application code.
    /// </summary>
    public class AMNetSftpFileSystemAccessor : IAMNetSftpFileSystemAccessor
    {
        /// <summary>
        /// Creates a default SFTP filesystem accessor.
        /// </summary>
        public AMNetSftpFileSystemAccessor()
        {
        }

        /// <inheritdoc />
        public virtual string ResolveLocalFilePath(ISshFileSystemAccess context, string resolvedLocalPath)
        {
            return resolvedLocalPath;
        }

        /// <inheritdoc />
        public virtual bool IsPathAllowed(ISshFileSystemAccess context)
        {
            string? localPath = context.LocalPath;
            string? rootPath = context.RootPath;

            // Allow root and current-directory queries through without jail checks.
            if (context.Operation == SshFileSystemOperation.ResolveLocalFilePath &&
                (string.IsNullOrWhiteSpace(context.RemotePath) ||
                 context.RemotePath == "." ||
                 context.RemotePath == "/"))
            {
                return true;
            }

            if (!IsVisibleByDefault(localPath))
            {
                return false;
            }

            // Jail containment: ensure the resolved real path is within the root.
            if (!string.IsNullOrWhiteSpace(localPath) &&
                !string.IsNullOrWhiteSpace(rootPath))
            {
                return IsWithinRoot(localPath, rootPath);
            }

            return true;
        }

        private static bool IsWithinRoot(string localPath, string rootPath)
        {
            string realPath = ResolveFinalTarget(localPath);
            string normalizedRoot = Path.GetFullPath(rootPath);

            return realPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveFinalTarget(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);
                var target = File.ResolveLinkTarget(fullPath, true);
                return target?.FullName ?? fullPath;
            }
            catch
            {
                return Path.GetFullPath(path);
            }
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<string> ResolveFileAccessLinkOptions(
            ISshFileSystemAccess context,
            IReadOnlyList<string> resolvedOptions)
        {
            return resolvedOptions;
        }

        /// <inheritdoc />
        public virtual IReadOnlyDictionary<string, object> ResolveReportedFileAttributes(
            ISshFileSystemAccess context,
            IReadOnlyDictionary<string, object> resolvedAttributes)
        {
            return resolvedAttributes;
        }

        /// <inheritdoc />
        public virtual void ApplyExtensionFileAttributes(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void PutRemoteFileName(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void OpenFile(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void TryLock(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void SyncFileData(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void CloseFile(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void OpenDirectory(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void CloseDirectory(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual bool ShouldIncludeDirectoryEntry(ISshFileSystemAccess context)
        {
            return IsVisibleByDefault(context.LocalPath);
        }

        /// <inheritdoc />
        public virtual IReadOnlyDictionary<string, object> ReadFileAttributes(
            ISshFileSystemAccess context,
            IReadOnlyDictionary<string, object> resolvedAttributes)
        {
            return resolvedAttributes;
        }

        /// <inheritdoc />
        public virtual void SetFileAttribute(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void ResolveFileOwner(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void SetFileOwner(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void ResolveGroupOwner(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void SetGroupOwner(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void SetFilePermissions(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void SetFileAccessControl(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void CreateDirectory(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void CreateLink(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual string ResolveLinkTarget(ISshFileSystemAccess context, string resolvedTarget)
        {
            return resolvedTarget;
        }

        /// <inheritdoc />
        public virtual void RenameFile(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void CopyFile(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual void RemoveFile(ISshFileSystemAccess context)
        {
        }

        /// <inheritdoc />
        public virtual bool NoFollow(ISshFileSystemAccess context, bool defaultNoFollow)
        {
            return defaultNoFollow;
        }

        /// <summary>
        /// Returns whether a path should be visible using the default hidden-file policy.
        /// </summary>
        /// <param name="localPath">The local path to evaluate.</param>
        /// <returns><c>true</c> when the path should be visible; otherwise <c>false</c>.</returns>
        protected virtual bool IsVisibleByDefault(string? localPath)
        {
            if (string.IsNullOrWhiteSpace(localPath))
            {
                return true;
            }

            string? fileName = Path.GetFileName(localPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return true;
            }

            if (fileName.StartsWith(".", StringComparison.Ordinal) &&
                fileName != "." &&
                fileName != "..")
            {
                return false;
            }

            if (fileName.Contains("secret_data", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                if (File.Exists(localPath) || Directory.Exists(localPath))
                {
                    return (File.GetAttributes(localPath) & FileAttributes.Hidden) == 0;
                }
            }
            catch
            {
                return true;
            }

            return true;
        }
    }
}
