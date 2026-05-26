using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Internals;
using java.util;
using org.apache.sshd.server;

namespace ApacheMinaSSHD.NET.Wrapper
{
    /// <summary>
    /// Main .NET-facing SSH server wrapper used to host SFTP and SCP services.
    /// </summary>
    /// <remarks>
    /// This type hides the underlying Apache MINA SSHD server from application code.
    /// Configure authentication, host keys, filesystem policy, and enabled subsystems
    /// before calling <see cref="Start()"/>.
    /// </remarks>
    public sealed class AMNetSshServer : IDisposable
    {
        private readonly SshServer server;

        private AMNetSshServer(SshServer server)
        {
            this.server = server;
            Config = new AMNetSshServerConfig(server);
        }

        internal SshServer JavaServer => server;

        /// <summary>
        /// Gets server configuration helpers for resource limits, timeouts, and cryptographic algorithms.
        /// </summary>
        public AMNetSshServerConfig Config { get; }

        /// <summary>
        /// Gets or sets the TCP port the SSH server listens on.
        /// </summary>
        public int Port
        {
            get => server.getPort();
            set => server.setPort(value);
        }

        /// <summary>
        /// Gets or sets the bind address. Use <c>null</c> to use the server default.
        /// </summary>
        public string? Host
        {
            get => server.getHost();
            set => server.setHost(value);
        }

        /// <summary>
        /// Creates a server with the default SSH server factories.
        /// </summary>
        public static AMNetSshServer setUpDefaultServer()
        {
            return new AMNetSshServer(SshServer.setUpDefaultServer());
        }

        /// <summary>
        /// Creates a server with the default SSH server factories.
        /// </summary>
        public static AMNetSshServer SetUpDefaultServer() => setUpDefaultServer();

        /// <summary>
        /// Sets the TCP port the SSH server listens on.
        /// </summary>
        /// <param name="port">The TCP port number.</param>
        public void setPort(int port) => Port = port;

        /// <summary>
        /// Gets the TCP port the SSH server listens on.
        /// </summary>
        public int getPort() => Port;

        /// <summary>
        /// Sets the bind address. Use <c>null</c> to use the server default.
        /// </summary>
        /// <param name="host">The IP address or host name to bind.</param>
        public void setHost(string? host) => Host = host;

        /// <summary>
        /// Gets the configured bind address.
        /// </summary>
        public string? getHost() => Host;

        /// <summary>
        /// Starts accepting SSH connections.
        /// </summary>
        public void start() => server.start();

        /// <summary>
        /// Starts accepting SSH connections.
        /// </summary>
        public void Start() => start();

        /// <summary>
        /// Stops the server and closes active resources using the server default shutdown behavior.
        /// </summary>
        public void stop() => server.stop();

        /// <summary>
        /// Stops the server and closes active resources using the server default shutdown behavior.
        /// </summary>
        public void Stop() => stop();

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <param name="immediately">When <c>true</c>, closes active sessions immediately.</param>
        public void stop(bool immediately) => server.stop(immediately);

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <param name="immediately">When <c>true</c>, closes active sessions immediately.</param>
        public void Stop(bool immediately) => stop(immediately);

        /// <summary>
        /// Returns whether the server has been started.
        /// </summary>
        public bool isStarted() => server.isStarted();

        /// <summary>
        /// Returns whether the server has been started.
        /// </summary>
        public bool IsStarted() => isStarted();

        /// <summary>
        /// Returns whether the server has been closed.
        /// </summary>
        public bool isClosed() => server.isClosed();

        /// <summary>
        /// Returns whether the server has been closed.
        /// </summary>
        public bool IsClosed() => isClosed();

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Dispose() => stop();

        /// <summary>
        /// Sets the host key provider used to identify this SSH server to clients.
        /// </summary>
        /// <param name="keyProvider">The host key provider configuration.</param>
        public void setKeyPairProvider(AMNetSimpleGeneratorHostKeyProvider keyProvider)
        {
            ArgumentNullException.ThrowIfNull(keyProvider);
            server.setKeyPairProvider(keyProvider.ToJavaKeyPairProvider());
        }

        /// <summary>
        /// Sets the virtual filesystem factory used to map users to server-side home directories.
        /// </summary>
        /// <param name="fileSystemFactory">The filesystem factory configuration.</param>
        public void setFileSystemFactory(AMNetVirtualFileSystemFactory fileSystemFactory)
        {
            ArgumentNullException.ThrowIfNull(fileSystemFactory);
            server.setFileSystemFactory(fileSystemFactory.ToJavaFileSystemFactory());
        }

        /// <summary>
        /// Enables keyboard-interactive authentication.
        /// </summary>
        /// <param name="keyboardInteractiveAuthenticator">The application authenticator.</param>
        public void setKeyboardInteractiveAuthenticator(IAMNetKeyboardInteractiveAuthenticator keyboardInteractiveAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(keyboardInteractiveAuthenticator);
            server.setKeyboardInteractiveAuthenticator(new InternalKeyboardInteractiveAuthenticator(keyboardInteractiveAuthenticator));
        }

        /// <summary>
        /// Enables keyboard-interactive authentication.
        /// </summary>
        /// <param name="keyboardInteractiveAuthenticator">The application authenticator.</param>
        public void SetKeyboardInteractiveAuthenticator(IAMNetKeyboardInteractiveAuthenticator keyboardInteractiveAuthenticator)
        {
            setKeyboardInteractiveAuthenticator(keyboardInteractiveAuthenticator);
        }

        /// <summary>
        /// Enables username/password authentication.
        /// </summary>
        /// <param name="passwordAuthenticator">The application authenticator.</param>
        public void setPasswordAuthenticator(IAMNetPasswordAuthenticator passwordAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(passwordAuthenticator);
            server.setPasswordAuthenticator(new InternalPasswordAuthenticator(passwordAuthenticator));
        }

        /// <summary>
        /// Enables username/password authentication.
        /// </summary>
        /// <param name="passwordAuthenticator">The application authenticator.</param>
        public void SetPasswordAuthenticator(IAMNetPasswordAuthenticator passwordAuthenticator)
        {
            setPasswordAuthenticator(passwordAuthenticator);
        }

        /// <summary>
        /// Enables public key authentication.
        /// </summary>
        /// <param name="publickeyAuthenticator">The application authenticator.</param>
        public void setPublickeyAuthenticator(IAMNetPublickeyAuthenticator publickeyAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(publickeyAuthenticator);
            server.setPublickeyAuthenticator(new InternalPublickeyAuthenticator(publickeyAuthenticator));
        }

        /// <summary>
        /// Enables public key authentication.
        /// </summary>
        /// <param name="publickeyAuthenticator">The application authenticator.</param>
        public void SetPublickeyAuthenticator(IAMNetPublickeyAuthenticator publickeyAuthenticator)
        {
            setPublickeyAuthenticator(publickeyAuthenticator);
        }

        /// <summary>
        /// Enables public key authentication.
        /// </summary>
        /// <param name="publickeyAuthenticator">The application authenticator.</param>
        public void SetPublicKeyAuthenticator(IAMNetPublickeyAuthenticator publickeyAuthenticator)
        {
            setPublickeyAuthenticator(publickeyAuthenticator);
        }

        /// <summary>
        /// Enables public key authentication backed by an authorized_keys file.
        /// </summary>
        /// <param name="authorizedKeysAuthenticator">The authorized keys configuration.</param>
        public void setAuthorizedkeyAuthenticator(IAMNetAuthorizedKeysAuthenticator authorizedKeysAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(authorizedKeysAuthenticator);
            server.setPublickeyAuthenticator(new InternalAuthorizedKeysAuthenticator(authorizedKeysAuthenticator));
        }

        /// <summary>
        /// Enables public key authentication backed by an authorized_keys file.
        /// </summary>
        /// <param name="authorizedKeysAuthenticator">The authorized keys configuration.</param>
        public void SetAuthorizedkeyAuthenticator(IAMNetAuthorizedKeysAuthenticator authorizedKeysAuthenticator)
        {
            setAuthorizedkeyAuthenticator(authorizedKeysAuthenticator);
        }

        /// <summary>
        /// Enables public key authentication backed by an authorized_keys file.
        /// </summary>
        /// <param name="authorizedKeysAuthenticator">The authorized keys configuration.</param>
        public void SetAuthorizedKeysAuthenticator(IAMNetAuthorizedKeysAuthenticator authorizedKeysAuthenticator)
        {
            setAuthorizedkeyAuthenticator(authorizedKeysAuthenticator);
        }

        /// <summary>
        /// Enables PROXY protocol metadata handling before the SSH handshake.
        /// </summary>
        /// <param name="serverProxyAcceptor">The acceptor that parses and validates proxy metadata.</param>
        public void setServerProxyAcceptor(IAMNetServerProxyAcceptor serverProxyAcceptor)
        {
            ArgumentNullException.ThrowIfNull(serverProxyAcceptor);
            server.setServerProxyAcceptor(new InternalServerProxyAcceptor(serverProxyAcceptor));
        }

        /// <summary>
        /// Registers low-level I/O service callbacks.
        /// </summary>
        /// <param name="serverIoServiceEventListener">The low-level connection listener.</param>
        public void setIoServiceEventListener(IAMNetIoServiceEventListener serverIoServiceEventListener)
        {
            ArgumentNullException.ThrowIfNull(serverIoServiceEventListener);
            server.setIoServiceEventListener(new InternalIoServiceEventListener(serverIoServiceEventListener));
        }

        /// <summary>
        /// Registers session lifecycle callbacks.
        /// </summary>
        /// <param name="sessionListener">The session listener to add.</param>
        public void addSessionListener(IAMNetSessionListener sessionListener)
        {
            ArgumentNullException.ThrowIfNull(sessionListener);
            server.addSessionListener(new InternalSessionListener(sessionListener));
        }

        /// <summary>
        /// Enables the SFTP subsystem for incoming SSH sessions.
        /// </summary>
        /// <param name="sftpFactory">The SFTP subsystem factory.</param>
        public void setSubsystemFactories(AMNetSftpSubsystemFactory sftpFactory)
        {
            ArgumentNullException.ThrowIfNull(sftpFactory);
            server.setSubsystemFactories(Collections.singletonList(sftpFactory.JavaFactory));
        }

        /// <summary>
        /// Enables SCP command handling for incoming SSH sessions.
        /// </summary>
        /// <param name="scpFactory">The SCP command factory.</param>
        public void setCommandFactory(AMNetScpCommandFactory scpFactory)
        {
            ArgumentNullException.ThrowIfNull(scpFactory);
            server.setCommandFactory(scpFactory.JavaFactory);
        }
    }
}
