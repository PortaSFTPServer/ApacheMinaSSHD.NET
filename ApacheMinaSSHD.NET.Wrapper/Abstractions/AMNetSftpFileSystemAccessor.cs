using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

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
                Console.WriteLine($"[SRV-RFT] ResolveFinalTarget input: {path}");
                Console.WriteLine($"[SRV-RFT] ResolveFinalTarget fullPath: {fullPath}");

                string? finalPath = ResolveViaGetFinalPathByHandle(fullPath);
                if (finalPath != null)
                {
                    Console.WriteLine($"[SRV-RFT] GetFinalPathNameByHandle: {finalPath}");
                    return finalPath;
                }
                Console.WriteLine("[SRV-RFT] GetFinalPathNameByHandle returned null");

                var target = File.ResolveLinkTarget(fullPath, true);
                if (target != null)
                {
                    Console.WriteLine($"[SRV-RFT] File.ResolveLinkTarget(true): {target.FullName}");
                    return target.FullName;
                }
                Console.WriteLine("[SRV-RFT] File.ResolveLinkTarget(true) returned null");

                target = File.ResolveLinkTarget(fullPath, false);
                if (target != null)
                {
                    Console.WriteLine($"[SRV-RFT] File.ResolveLinkTarget(false): {target.FullName}");
                    return target.FullName;
                }
                Console.WriteLine("[SRV-RFT] File.ResolveLinkTarget(false) returned null");

                Console.WriteLine($"[SRV-RFT] Fallback to fullPath: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SRV-RFT] ResolveFinalTarget threw: {ex.GetType().Name}: {ex.Message}");
                return Path.GetFullPath(path);
            }
        }

        private static string? ResolveViaGetFinalPathByHandle(string fullPath)
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            try
            {
                using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.None);
                var sb = new StringBuilder(4096);
                int result = GetFinalPathNameByHandle(fs.SafeFileHandle, sb, sb.Capacity, VOLUME_NAME_DOS);
                if (result <= 0)
                {
                    return null;
                }

                string finalPath = sb.ToString();
                if (finalPath.StartsWith(@"\\?\", StringComparison.Ordinal))
                {
                    finalPath = finalPath.Substring(4);
                }

                return finalPath;
            }
            catch
            {
                return null;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetFinalPathNameByHandle(
            SafeFileHandle hFile,
            StringBuilder lpszFilePath,
            int cchFilePath,
            int dwFlags);

        private const int VOLUME_NAME_DOS = 0;

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
