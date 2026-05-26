using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default .NET-facing SCP filesystem policy hook. Override this class to implement
    /// application-specific SCP path, stream, and permission behavior without using
    /// Apache MINA or Java types in application code.
    /// </summary>
    public class AMNetScpFileOpener : IAMNetScpFileOpener
    {
        /// <summary>
        /// Creates a default SCP policy hook.
        /// </summary>
        /// <param name="rootPath">Optional root path used by the default path allow-list behavior.</param>
        public AMNetScpFileOpener(string? rootPath = null)
        {
            RootPath = string.IsNullOrWhiteSpace(rootPath)
                ? null
                : Path.GetFullPath(rootPath);
        }

        /// <summary>
        /// Gets the optional local root path used by the default path allow-list behavior.
        /// </summary>
        public string? RootPath { get; }

        /// <inheritdoc />
        public virtual string ResolveLocalPath(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <inheritdoc />
        public virtual string ResolveIncomingFilePath(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <inheritdoc />
        public virtual string ResolveIncomingReceiveLocation(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <inheritdoc />
        public virtual string ResolveOutgoingFilePath(ISshScpFileAccess access, string resolvedPath) => resolvedPath;

        /// <inheritdoc />
        public virtual IReadOnlyList<string> GetMatchingFilesToSend(
            ISshScpFileAccess access,
            IReadOnlyList<string> resolvedPaths) => resolvedPaths;

        /// <inheritdoc />
        public virtual bool IsPathAllowed(ISshScpFileAccess access)
        {
            if (string.IsNullOrWhiteSpace(access.LocalPath))
            {
                return true;
            }

            string path = ResolvePolicyPath(access.LocalPath);
            if (!IsVisibleByDefault(path))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(RootPath))
            {
                return true;
            }

            string root = EnsureTrailingSeparator(Path.GetFullPath(RootPath));

            return path.Equals(root.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public virtual bool ShouldSendAsRegularFile(ISshScpFileAccess access, bool defaultValue) => defaultValue;

        /// <inheritdoc />
        public virtual bool ShouldSendAsDirectory(ISshScpFileAccess access, bool defaultValue) => defaultValue;

        /// <inheritdoc />
        public virtual bool ShouldIncludeDirectoryEntry(ISshScpFileAccess access)
        {
            return IsVisibleByDefault(access.LocalPath);
        }

        /// <inheritdoc />
        public virtual IReadOnlyDictionary<string, object> ReadLocalBasicFileAttributes(
            ISshScpFileAccess access,
            IReadOnlyDictionary<string, object> attributes) => attributes;

        /// <inheritdoc />
        public virtual IReadOnlyList<string> GetLocalFilePermissions(
            ISshScpFileAccess access,
            IReadOnlyList<string> permissions) => permissions;

        /// <inheritdoc />
        public virtual void OpenRead(ISshScpFileAccess access) { }

        /// <inheritdoc />
        public virtual void CloseRead(ISshScpFileAccess access) { }

        /// <inheritdoc />
        public virtual void OpenWrite(ISshScpFileAccess access) { }

        /// <inheritdoc />
        public virtual void CloseWrite(ISshScpFileAccess access) { }

        /// <inheritdoc />
        public virtual void CreateSourceStreamResolver(ISshScpFileAccess access) { }

        /// <inheritdoc />
        public virtual void CreateTargetStreamResolver(ISshScpFileAccess access) { }

        private static bool IsVisibleByDefault(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return true;
            }

            string fileName = Path.GetFileName(path);
            if (fileName.StartsWith(".", StringComparison.Ordinal) ||
                fileName.Contains("secret_data", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                return !File.GetAttributes(path).HasFlag(FileAttributes.Hidden);
            }
            catch
            {
                return true;
            }
        }

        private static string EnsureTrailingSeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        private string ResolvePolicyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(RootPath) || Path.IsPathFullyQualified(path))
            {
                return Path.GetFullPath(path);
            }

            string relativePath = path
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return Path.GetFullPath(Path.Combine(RootPath, relativePath));
        }
    }
}
