using ApacheMinaSSHD.NET.Wrapper.Internals;
using org.apache.sshd.common.file;

namespace ApacheMinaSSHD.NET.Wrapper.Factories
{
    /// <summary>
    /// Maps authenticated users to local virtual filesystem roots.
    /// </summary>
    public class AMNetVirtualFileSystemFactory
    {
        /// <summary>
        /// Creates a virtual filesystem factory that creates per-user directories under <paramref name="basePath"/>.
        /// </summary>
        /// <param name="basePath">The base directory that contains user home directories.</param>
        public AMNetVirtualFileSystemFactory(string basePath)
            : this(basePath, createUserDirectory: true)
        {
        }

        /// <summary>
        /// Creates a virtual filesystem factory.
        /// </summary>
        /// <param name="basePath">The base directory used to resolve user home directories.</param>
        /// <param name="createUserDirectory">Whether missing user home directories may be created automatically.</param>
        public AMNetVirtualFileSystemFactory(string basePath, bool createUserDirectory)
        {
            BasePath = string.IsNullOrWhiteSpace(basePath)
                ? throw new ArgumentException("Base path is required.", nameof(basePath))
                : basePath;
            CreateUserDirectory = createUserDirectory;
        }

        /// <summary>
        /// Gets the base directory used to resolve user home directories.
        /// </summary>
        public string BasePath { get; }

        /// <summary>
        /// Gets whether missing user home directories may be created automatically.
        /// </summary>
        public bool CreateUserDirectory { get; }

        /// <summary>
        /// Resolves the local home directory for an authenticated username.
        /// </summary>
        /// <param name="username">The authenticated username.</param>
        /// <returns>The local directory path to use as the user's home.</returns>
        public virtual string ResolveUserHomeDirectory(string username)
        {
            return Path.Combine(BasePath, username);
        }

        internal FileSystemFactory ToJavaFileSystemFactory()
        {
            return new InternalVirtualFileSystemFactory(this);
        }
    }
}
