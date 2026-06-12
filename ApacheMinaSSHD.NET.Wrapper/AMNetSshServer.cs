using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Internals;
using java.util;
using org.apache.sshd.server;
using System.Net;

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
        private IAMNetIoServiceEventListener? _userIoServiceEventListener;
        private IAmNetConnectionRateLimiter? _rateLimiter;

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
        /// Gets the configured authentication method chains in evaluation order.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> getConfiguredAuthenticationMethods()
        {
            return Config.GetConfiguredAuthenticationMethods();
        }

        /// <summary>
        /// Gets the configured authentication method chains in evaluation order.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> GetConfiguredAuthenticationMethods()
        {
            return getConfiguredAuthenticationMethods();
        }

        /// <summary>
        /// Sets the authentication method policy using one or more pre-built method chains.
        /// </summary>
        /// <param name="authenticationChains">
        /// Authentication chains such as <see cref="AMNetSshAuthenticationMethods.PublicKey"/>
        /// or values returned by <see cref="AMNetSshAuthenticationMethods.RequireAll(string[])"/>.
        /// </param>
        public void setAuthenticationMethods(params string[] authenticationChains)
        {
            Config.SetAuthenticationMethods(authenticationChains);
        }

        /// <summary>
        /// Sets the authentication method policy using one or more pre-built method chains.
        /// </summary>
        /// <param name="authenticationChains">
        /// Authentication chains such as <see cref="AMNetSshAuthenticationMethods.PublicKey"/>
        /// or values returned by <see cref="AMNetSshAuthenticationMethods.RequireAll(string[])"/>.
        /// </param>
        public void SetAuthenticationMethods(params string[] authenticationChains)
        {
            setAuthenticationMethods(authenticationChains);
        }

        /// <summary>
        /// Sets the authentication method policy using one or more required method groups.
        /// </summary>
        /// <param name="authenticationMethodGroups">
        /// Each group contains methods that must all succeed in order. The outer set
        /// represents alternative groups.
        /// </param>
        public void setAuthenticationMethodGroups(params IEnumerable<string>[] authenticationMethodGroups)
        {
            Config.SetAuthenticationMethodGroups(authenticationMethodGroups);
        }

        /// <summary>
        /// Sets the authentication method policy using one or more required method groups.
        /// </summary>
        /// <param name="authenticationMethodGroups">
        /// Each group contains methods that must all succeed in order. The outer set
        /// represents alternative groups.
        /// </param>
        public void SetAuthenticationMethodGroups(params IEnumerable<string>[] authenticationMethodGroups)
        {
            setAuthenticationMethodGroups(authenticationMethodGroups);
        }

        /// <summary>
        /// Gets or sets the TCP port the SSH server listens on.
        /// </summary>
        public int Port
        {
            get => server.getPort();
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"Port must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}.");
                }

                server.setPort(value);
            }
        }

        /// <summary>
        /// Gets or sets the bind address. Use <c>null</c> to use the server default.
        /// Accepts IPv4, IPv6, or valid hostname. Rejects empty/whitespace-only values.
        /// </summary>
        public string? Host
        {
            get => server.getHost();
            set
            {
                if (value != null && string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Host cannot be empty or whitespace. Use null to bind to all interfaces.", nameof(value));
                }

                if (value != null && value.Length > 255)
                {
                    throw new ArgumentException("Host value exceeds maximum length of 255 characters.", nameof(value));
                }

                server.setHost(value);
            }
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

        private bool disposed;

        /// <summary>
        /// Stops the server if it was started.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                if (isStarted())
                {
                    stop();
                }

                disposed = true;
            }
        }

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
        /// Enables keyboard-interactive authentication using .NET callbacks.
        /// </summary>
        /// <param name="generateChallenge">Callback that populates prompts sent to the client.</param>
        /// <param name="authenticate">Callback that validates the client responses.</param>
        public void setDelegateKeyboardInteractiveAuthenticator(
            Action<string, ISshChallenge> generateChallenge,
            Func<ISshSession, string, IResponseList, bool> authenticate)
        {
            setKeyboardInteractiveAuthenticator(
                new AMNetDelegateKeyboardInteractiveAuthenticator(generateChallenge, authenticate));
        }

        /// <summary>
        /// Enables keyboard-interactive authentication using .NET callbacks.
        /// </summary>
        /// <param name="generateChallenge">Callback that populates prompts sent to the client.</param>
        /// <param name="authenticate">Callback that validates the client responses.</param>
        public void SetDelegateKeyboardInteractiveAuthenticator(
            Action<string, ISshChallenge> generateChallenge,
            Func<ISshSession, string, IResponseList, bool> authenticate)
        {
            setDelegateKeyboardInteractiveAuthenticator(generateChallenge, authenticate);
        }

        /// <summary>
        /// Enables keyboard-interactive authentication with a single fixed response.
        /// </summary>
        /// <param name="expectedResponse">The exact response to accept.</param>
        /// <param name="username">Optional exact username to accept.</param>
        /// <param name="prompt">Prompt text shown to the client.</param>
        /// <param name="interactionName">Challenge name shown to the client.</param>
        /// <param name="instruction">Instruction text shown with the challenge.</param>
        public void setFixedKeyboardInteractiveAuthenticator(
            string expectedResponse,
            string? username = null,
            string prompt = "Verification code",
            string interactionName = "Authentication",
            string instruction = "Enter the verification code.")
        {
            setKeyboardInteractiveAuthenticator(
                new AMNetFixedKeyboardInteractiveAuthenticator(
                    expectedResponse,
                    username,
                    prompt,
                    interactionName,
                    instruction));
        }

        /// <summary>
        /// Enables keyboard-interactive authentication with a single fixed response.
        /// </summary>
        /// <param name="expectedResponse">The exact response to accept.</param>
        /// <param name="username">Optional exact username to accept.</param>
        /// <param name="prompt">Prompt text shown to the client.</param>
        /// <param name="interactionName">Challenge name shown to the client.</param>
        /// <param name="instruction">Instruction text shown with the challenge.</param>
        public void SetFixedKeyboardInteractiveAuthenticator(
            string expectedResponse,
            string? username = null,
            string prompt = "Verification code",
            string interactionName = "Authentication",
            string instruction = "Enter the verification code.")
        {
            setFixedKeyboardInteractiveAuthenticator(
                expectedResponse,
                username,
                prompt,
                interactionName,
                instruction);
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
        /// Enables username/password authentication using multiple modules evaluated in order.
        /// </summary>
        /// <param name="authenticators">The password authenticators to try.</param>
        public void setCompositePasswordAuthenticator(params IAMNetPasswordAuthenticator[] authenticators)
        {
            setPasswordAuthenticator(new AMNetCompositePasswordAuthenticator(authenticators));
        }

        /// <summary>
        /// Enables username/password authentication using multiple modules evaluated in order.
        /// </summary>
        /// <param name="authenticators">The password authenticators to try.</param>
        public void SetCompositePasswordAuthenticator(params IAMNetPasswordAuthenticator[] authenticators)
        {
            setCompositePasswordAuthenticator(authenticators);
        }

        /// <summary>
        /// Enables username/password authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, password, and session metadata and returns
        /// whether the credentials should be accepted.
        /// </param>
        public void setDelegatePasswordAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            setPasswordAuthenticator(new AMNetDelegatePasswordAuthenticator(authenticate));
        }

        /// <summary>
        /// Enables username/password authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, password, and session metadata and returns
        /// whether the credentials should be accepted.
        /// </param>
        public void SetDelegatePasswordAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            setDelegatePasswordAuthenticator(authenticate);
        }

        /// <summary>
        /// Enables a single fixed username/password pair.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="password">The exact password to accept.</param>
        public void setFixedPasswordAuthenticator(string username, string password)
        {
            setPasswordAuthenticator(new AMNetFixedPasswordAuthenticator(username, password));
        }

        /// <summary>
        /// Enables a single fixed username/password pair.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="password">The exact password to accept.</param>
        public void SetFixedPasswordAuthenticator(string username, string password)
        {
            setFixedPasswordAuthenticator(username, password);
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
        /// Enables public key authentication using multiple modules evaluated in order.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try.</param>
        public void setCompositePublickeyAuthenticator(params IAMNetPublickeyAuthenticator[] authenticators)
        {
            setPublickeyAuthenticator(new AMNetCompositePublickeyAuthenticator(authenticators));
        }

        /// <summary>
        /// Enables public key authentication using multiple modules evaluated in order.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try.</param>
        public void SetCompositePublickeyAuthenticator(params IAMNetPublickeyAuthenticator[] authenticators)
        {
            setCompositePublickeyAuthenticator(authenticators);
        }

        /// <summary>
        /// Enables public key authentication using multiple modules evaluated in order.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try.</param>
        public void SetCompositePublicKeyAuthenticator(params IAMNetPublickeyAuthenticator[] authenticators)
        {
            setCompositePublickeyAuthenticator(authenticators);
        }

        /// <summary>
        /// Enables public key authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, public key fingerprint, and session metadata
        /// and returns whether the key should be accepted.
        /// </param>
        public void setDelegatePublickeyAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            setPublickeyAuthenticator(new AMNetDelegatePublickeyAuthenticator(authenticate));
        }

        /// <summary>
        /// Enables public key authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, public key fingerprint, and session metadata
        /// and returns whether the key should be accepted.
        /// </param>
        public void SetDelegatePublickeyAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            setDelegatePublickeyAuthenticator(authenticate);
        }

        /// <summary>
        /// Enables public key authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, public key fingerprint, and session metadata
        /// and returns whether the key should be accepted.
        /// </param>
        public void SetDelegatePublicKeyAuthenticator(Func<string, string, ISshSession, bool> authenticate)
        {
            setDelegatePublickeyAuthenticator(authenticate);
        }

        /// <summary>
        /// Enables public key authentication for one username and one or more accepted fingerprints.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="acceptedFingerprints">One or more accepted public key fingerprints.</param>
        public void setFingerprintPublickeyAuthenticator(string username, params string[] acceptedFingerprints)
        {
            ArgumentNullException.ThrowIfNull(acceptedFingerprints);
            if (acceptedFingerprints.Length == 0)
            {
                throw new ArgumentException("At least one accepted fingerprint is required.", nameof(acceptedFingerprints));
            }

            var authenticator = new AMNetFingerprintPublickeyAuthenticator();
            foreach (string acceptedFingerprint in acceptedFingerprints)
            {
                authenticator.AddFingerprint(username, acceptedFingerprint);
            }

            setPublickeyAuthenticator(authenticator);
        }

        /// <summary>
        /// Enables public key authentication for one username and one or more accepted fingerprints.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="acceptedFingerprints">One or more accepted public key fingerprints.</param>
        public void SetFingerprintPublickeyAuthenticator(string username, params string[] acceptedFingerprints)
        {
            setFingerprintPublickeyAuthenticator(username, acceptedFingerprints);
        }

        /// <summary>
        /// Enables public key authentication for one username and one or more accepted fingerprints.
        /// </summary>
        /// <param name="username">The exact username to accept.</param>
        /// <param name="acceptedFingerprints">One or more accepted public key fingerprints.</param>
        public void SetFingerprintPublicKeyAuthenticator(string username, params string[] acceptedFingerprints)
        {
            setFingerprintPublickeyAuthenticator(username, acceptedFingerprints);
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
        /// Enables public key authentication backed by an authorized_keys file path.
        /// </summary>
        /// <param name="path">The authorized_keys file path.</param>
        public void setAuthorizedkeyAuthenticator(string path)
        {
            setAuthorizedkeyAuthenticator(new AMNetAuthorizedKeysAuthenticator(path));
        }

        /// <summary>
        /// Enables public key authentication backed by an authorized_keys file path.
        /// </summary>
        /// <param name="path">The authorized_keys file path.</param>
        public void SetAuthorizedkeyAuthenticator(string path)
        {
            setAuthorizedkeyAuthenticator(path);
        }

        /// <summary>
        /// Enables public key authentication backed by an authorized_keys file path.
        /// </summary>
        /// <param name="path">The authorized_keys file path.</param>
        public void SetAuthorizedKeysAuthenticator(string path)
        {
            setAuthorizedkeyAuthenticator(path);
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
            _userIoServiceEventListener = serverIoServiceEventListener;
            ApplyIoServiceEventListener();
        }

        /// <summary>
        /// Sets a per-IP connection rate limiter that is evaluated before any
        /// registered <see cref="IAMNetIoServiceEventListener"/>.
        /// Pass <c>null</c> to disable rate limiting.
        /// </summary>
        public void SetRateLimiter(IAmNetConnectionRateLimiter? rateLimiter)
        {
            _rateLimiter = rateLimiter;
            ApplyIoServiceEventListener();
        }

        private void ApplyIoServiceEventListener()
        {
            var effective = _userIoServiceEventListener ?? new AMNetIoServiceEventListener();
            if (_rateLimiter != null)
            {
                effective = new RateLimitingIoServiceEventListener(effective, _rateLimiter);
            }

            server.setIoServiceEventListener(new InternalIoServiceEventListener(effective));
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
