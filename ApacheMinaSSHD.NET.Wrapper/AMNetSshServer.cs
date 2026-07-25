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
        private IAMNetForwardingFilter? _forwardingFilter;
        private IAMNetTcpForwardingFilter? _tcpForwardingFilter;
        private IAMNetAgentForwardingFilter? _agentForwardingFilter;
        private IAMNetX11ForwardingFilter? _x11ForwardingFilter;
        private AMNetSimpleGeneratorHostKeyProvider? _keyPairProvider;
        private AMNetVirtualFileSystemFactory? _fileSystemFactory;
        private IAMNetPasswordAuthenticator? _passwordAuthenticator;
        private IAMNetPublickeyAuthenticator? _publickeyAuthenticator;
        private IAMNetKeyboardInteractiveAuthenticator? _keyboardInteractiveAuthenticator;
        private IAMNetHostBasedAuthenticator? _hostBasedAuthenticator;
        private IAMNetGssapiAuthenticator? _gssapiAuthenticator;
        private IAMNetAuthorizedKeysAuthenticator? _authorizedKeysAuthenticator;
        private AMNetScpCommandFactory? _scpCommandFactory;
        private IAMNetCommandHandler? _commandHandler;
        private IAMNetServerProxyAcceptor? _serverProxyAcceptor;
        private AMNetSftpSubsystemFactory[]? _subsystemFactories;
        private org.apache.sshd.common.io.IoServiceFactoryFactory? _ioServiceFactoryFactory;
        private global::java.util.concurrent.ScheduledExecutorService? _scheduledExecutorService;

        private AMNetSshServer(SshServer server)
        {
            this.server = server;
            Config = new AMNetSshServerConfig(server);
        }

        internal SshServer JavaServer => server;

        /// <summary>
        /// Gets the Apache MINA SSHD version string of the underlying Java library.
        /// </summary>
        public string Version => server.getVersion();

        /// <summary>
        /// Gets server configuration helpers for resource limits, timeouts, and cryptographic algorithms.
        /// </summary>
        public AMNetSshServerConfig Config { get; }

        /// <summary>
        /// Gets the configured authentication method chains in evaluation order.
        /// </summary>
        /// <returns>Read-only list of authentication method chains, where each inner list is a chain of method names.</returns>
        public IReadOnlyList<IReadOnlyList<string>> getConfiguredAuthenticationMethods()
        {
            return Config.GetConfiguredAuthenticationMethods();
        }

        /// <summary>
        /// Gets the configured authentication method chains in evaluation order.
        /// </summary>
        /// <returns>Read-only list of authentication method chains, where each inner list is a chain of method names.</returns>
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationChains"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationMethodGroups"/> is null.</exception>
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
        /// Creates a server with the default SSH server factories and modern algorithm preferences.
        /// Equivalent to calling <c>SetUpDefaultServer()</c>.
        /// </summary>
        /// <returns>A new configured <see cref="AMNetSshServer"/> instance.</returns>
        public static AMNetSshServer setUpDefaultServer()
        {
            var server = new AMNetSshServer(SshServer.setUpDefaultServer());
            server.Config.ApplyModernAlgorithmDefaults();
            return server;
        }

        /// <summary>
        /// Creates a server with the default SSH server factories.
        /// </summary>
        /// <returns>A new configured <see cref="AMNetSshServer"/> instance.</returns>
        public static AMNetSshServer SetUpDefaultServer() => setUpDefaultServer();

        /// <summary>
        /// Sets the TCP port the SSH server listens on.
        /// </summary>
        /// <param name="port">The TCP port number.</param>
        public void setPort(int port) => Port = port;

        /// <summary>
        /// Gets the TCP port the SSH server listens on.
        /// </summary>
        /// <returns>The TCP port number.</returns>
        public int getPort() => Port;

        /// <summary>
        /// Sets the bind address. Use <c>null</c> to use the server default.
        /// </summary>
        /// <param name="host">The IP address or host name to bind.</param>
        public void setHost(string? host) => Host = host;

        /// <summary>
        /// Gets the configured bind address.
        /// </summary>
        /// <returns>The bind address, or <c>null</c> if the server default is used.</returns>
        public string? getHost() => Host;

        private static volatile bool _shutdownHandlerInstalled;
        private static readonly object _shutdownHandlerLock = new();

        /// <summary>
        /// Starts accepting SSH connections.
        /// </summary>
        public void start()
        {
            if (!_shutdownHandlerInstalled)
            {
                lock (_shutdownHandlerLock)
                {
                    if (!_shutdownHandlerInstalled)
                    {
                        var previous = java.lang.Thread.getDefaultUncaughtExceptionHandler();
                        java.lang.Thread.setDefaultUncaughtExceptionHandler(
                            new Internals.SuppressShutdownExceptionHandler(previous));
                        _shutdownHandlerInstalled = true;
                    }
                }
            }
            server.start();
        }

        /// <summary>
        /// Starts accepting SSH connections.
        /// </summary>
        public void Start() => start();

        /// <summary>
        /// Stops the server and closes active resources using the server default shutdown behavior.
        /// </summary>
        public void stop()
        {
            try
            {
                CloseAcceptorFirst();
                server.stop();
            }
            catch (java.lang.IllegalStateException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AMNetSshServer] Suppressed exception during stop: {ex.Message}");
            }
        }

        private void CloseAcceptorFirst()
        {
            try
            {
                var type = server.GetType();
                System.Reflection.FieldInfo? acceptorField = null;
                while (type != null && acceptorField == null)
                {
                    acceptorField = type.GetField(
                        "acceptor",
                        System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Public);
                    type = type.BaseType;
                }

                if (acceptorField?.GetValue(server) is org.apache.sshd.common.io.IoService acceptor)
                {
                    var closeBool = acceptor.GetType().GetMethod(
                        "close",
                        [typeof(bool)]);
                    closeBool?.Invoke(acceptor, [true]);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < 2000)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                }
            }
            catch (System.Exception ex) when (ex is System.Reflection.TargetInvocationException
                or System.Reflection.TargetParameterCountException
                or System.InvalidOperationException
                or System.NullReferenceException)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AMNetSshServer] Reflection error in CloseAcceptorFirst: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the server and closes active resources using the server default shutdown behavior.
        /// </summary>
        public void Stop() => stop();

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <param name="immediately">When <c>true</c>, closes active sessions immediately.</param>
        public void stop(bool immediately)
        {
            try
            {
                server.stop(immediately);
            }
            catch (java.lang.IllegalStateException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AMNetSshServer] Suppressed exception during stop: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <param name="immediately">When <c>true</c>, closes active sessions immediately.</param>
        public void Stop(bool immediately) => stop(immediately);

        /// <summary>
        /// Gets the addresses the server is currently bound to, or an empty collection if not started.
        /// </summary>
        /// <returns>Read-only set of bound addresses as strings (e.g., "0.0.0.0/0.0.0.0:22").</returns>
        public IReadOnlySet<string> getBoundAddresses()
        {
            var javaSet = server.getBoundAddresses();
            var result = new HashSet<string>();
            if (javaSet != null)
            {
                var iter = javaSet.iterator();
                while (iter.hasNext())
                {
                    var addr = iter.next();
                    if (addr != null)
                        result.Add(addr.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the addresses the server is currently bound to, or an empty collection if not started.
        /// </summary>
        /// <returns>Read-only set of bound addresses as strings.</returns>
        public IReadOnlySet<string> GetBoundAddresses() => getBoundAddresses();

        /// <summary>
        /// Gets the active session count.
        /// </summary>
        /// <returns>Number of currently active sessions.</returns>
        public int getActiveSessionCount()
        {
            var sessions = server.getActiveSessions();
            return sessions?.size() ?? 0;
        }

        /// <summary>
        /// Gets the active session count.
        /// </summary>
        public int GetActiveSessionCount() => getActiveSessionCount();

        /// <summary>
        /// Returns whether the server has been started.
        /// </summary>
        /// <returns><c>true</c> if the server is running; otherwise <c>false</c>.</returns>
        public bool isStarted() => server.isStarted();

        /// <summary>
        /// Returns whether the server has been started.
        /// </summary>
        /// <returns><c>true</c> if the server is running; otherwise <c>false</c>.</returns>
        public bool IsStarted() => isStarted();

        /// <summary>
        /// Returns whether the server has been closed.
        /// </summary>
        /// <returns><c>true</c> if the server is closed; otherwise <c>false</c>.</returns>
        public bool isClosed() => server.isClosed();

        /// <summary>
        /// Returns whether the server has been closed.
        /// </summary>
        /// <returns><c>true</c> if the server is closed; otherwise <c>false</c>.</returns>
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyProvider"/> is null.</exception>
        public void setKeyPairProvider(AMNetSimpleGeneratorHostKeyProvider keyProvider)
        {
            ArgumentNullException.ThrowIfNull(keyProvider);
            _keyPairProvider = keyProvider;
            server.setKeyPairProvider(keyProvider.ToJavaKeyPairProvider());
        }

        /// <summary>
        /// Gets the configured host key provider.
        /// </summary>
        /// <returns>The host key provider, or <c>null</c> if not configured.</returns>
        public AMNetSimpleGeneratorHostKeyProvider? getKeyPairProvider() => _keyPairProvider;

        /// <summary>
        /// Sets the virtual filesystem factory used to map users to server-side home directories.
        /// </summary>
        /// <param name="fileSystemFactory">The filesystem factory configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystemFactory"/> is null.</exception>
        public void setFileSystemFactory(AMNetVirtualFileSystemFactory fileSystemFactory)
        {
            ArgumentNullException.ThrowIfNull(fileSystemFactory);
            _fileSystemFactory = fileSystemFactory;
            server.setFileSystemFactory(fileSystemFactory.ToJavaFileSystemFactory());
        }

        /// <summary>
        /// Gets the configured virtual filesystem factory.
        /// </summary>
        /// <returns>The filesystem factory, or <c>null</c> if not configured.</returns>
        public AMNetVirtualFileSystemFactory? getFileSystemFactory() => _fileSystemFactory;

        /// <summary>
        /// Enables keyboard-interactive authentication.
        /// </summary>
        /// <param name="keyboardInteractiveAuthenticator">The application authenticator.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyboardInteractiveAuthenticator"/> is null.</exception>
        public void setKeyboardInteractiveAuthenticator(IAMNetKeyboardInteractiveAuthenticator keyboardInteractiveAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(keyboardInteractiveAuthenticator);
            _keyboardInteractiveAuthenticator = keyboardInteractiveAuthenticator;
            server.setKeyboardInteractiveAuthenticator(new InternalKeyboardInteractiveAuthenticator(keyboardInteractiveAuthenticator));
        }

        /// <summary>
        /// Gets the configured keyboard-interactive authenticator.
        /// </summary>
        /// <returns>The keyboard-interactive authenticator, or <c>null</c> if not configured.</returns>
        public IAMNetKeyboardInteractiveAuthenticator? getKeyboardInteractiveAuthenticator() => _keyboardInteractiveAuthenticator;

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="passwordAuthenticator"/> is null.</exception>
        public void setPasswordAuthenticator(IAMNetPasswordAuthenticator passwordAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(passwordAuthenticator);
            _passwordAuthenticator = passwordAuthenticator;
            server.setPasswordAuthenticator(new InternalPasswordAuthenticator(passwordAuthenticator));
        }

        /// <summary>
        /// Gets the configured password authenticator.
        /// </summary>
        /// <returns>The password authenticator, or <c>null</c> if not configured.</returns>
        public IAMNetPasswordAuthenticator? getPasswordAuthenticator() => _passwordAuthenticator;

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="publickeyAuthenticator"/> is null.</exception>
        public void setPublickeyAuthenticator(IAMNetPublickeyAuthenticator publickeyAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(publickeyAuthenticator);
            _publickeyAuthenticator = publickeyAuthenticator;
            server.setPublickeyAuthenticator(new InternalPublickeyAuthenticator(publickeyAuthenticator));
        }

        /// <summary>
        /// Gets the configured public key authenticator.
        /// </summary>
        /// <returns>The public key authenticator, or <c>null</c> if not configured.</returns>
        public IAMNetPublickeyAuthenticator? getPublickeyAuthenticator() => _publickeyAuthenticator;

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authorizedKeysAuthenticator"/> is null.</exception>
        public void setAuthorizedkeyAuthenticator(IAMNetAuthorizedKeysAuthenticator authorizedKeysAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(authorizedKeysAuthenticator);
            _authorizedKeysAuthenticator = authorizedKeysAuthenticator;
            server.setPublickeyAuthenticator(new InternalAuthorizedKeysAuthenticator(authorizedKeysAuthenticator));
        }

        /// <summary>
        /// Gets the configured authorized-keys authenticator.
        /// </summary>
        /// <returns>The authorized keys authenticator, or <c>null</c> if not configured.</returns>
        public IAMNetAuthorizedKeysAuthenticator? getAuthorizedkeyAuthenticator() => _authorizedKeysAuthenticator;

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverProxyAcceptor"/> is null.</exception>
        public void setServerProxyAcceptor(IAMNetServerProxyAcceptor serverProxyAcceptor)
        {
            ArgumentNullException.ThrowIfNull(serverProxyAcceptor);
            _serverProxyAcceptor = serverProxyAcceptor;
            server.setServerProxyAcceptor(new InternalServerProxyAcceptor(serverProxyAcceptor));
        }

        /// <summary>
        /// Gets the configured server proxy acceptor.
        /// </summary>
        /// <returns>The proxy acceptor, or <c>null</c> if not configured.</returns>
        public IAMNetServerProxyAcceptor? getServerProxyAcceptor() => _serverProxyAcceptor;

        /// <summary>
        /// Registers low-level I/O service callbacks.
        /// </summary>
        /// <param name="serverIoServiceEventListener">The low-level connection listener.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverIoServiceEventListener"/> is null.</exception>
        public void setIoServiceEventListener(IAMNetIoServiceEventListener serverIoServiceEventListener)
        {
            ArgumentNullException.ThrowIfNull(serverIoServiceEventListener);
            _userIoServiceEventListener = serverIoServiceEventListener;
            ApplyIoServiceEventListener();
        }

        /// <summary>
        /// Sets a per-IP connection rate limiter that is evaluated before any
        /// registered <see cref="IAMNetIoServiceEventListener"/>.
        /// </summary>
        /// <param name="rateLimiter">The rate limiter to apply, or <c>null</c> to disable rate limiting.</param>
        public void SetRateLimiter(IAmNetConnectionRateLimiter? rateLimiter)
        {
            _rateLimiter = rateLimiter;
            ApplyIoServiceEventListener();
        }

        /// <summary>
        /// Sets the TCP forwarding policy (Java-style naming).
        /// </summary>
        /// <param name="policy">The forwarding policy to apply.</param>
        public void setTcpForwardingPolicy(AMNetTcpForwardingPolicy policy)
        {
            _forwardingFilter = null;
            _tcpForwardingFilter = new AMNetTcpForwardingFilter(policy);
            _agentForwardingFilter = null;
            _x11ForwardingFilter = null;
            ApplyForwardingFilter();
        }

        /// <summary>
        /// Sets the TCP forwarding policy.
        /// </summary>
        /// <param name="policy">The forwarding policy to apply.</param>
        public void SetTcpForwardingPolicy(AMNetTcpForwardingPolicy policy)
            => setTcpForwardingPolicy(policy);

        /// <summary>
        /// Sets a combined forwarding filter (Java-style naming).
        /// </summary>
        /// <param name="filter">The combined forwarding filter.</param>
        public void setForwardingFilter(IAMNetForwardingFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            _forwardingFilter = filter;
            _tcpForwardingFilter = null;
            _agentForwardingFilter = null;
            _x11ForwardingFilter = null;
            ApplyForwardingFilter();
        }

        /// <summary>
        /// Sets a combined forwarding filter.
        /// </summary>
        /// <param name="filter">The combined forwarding filter.</param>
        public void SetForwardingFilter(IAMNetForwardingFilter filter)
            => setForwardingFilter(filter);

        /// <summary>
        /// Sets the TCP forwarding filter (Java-style naming).
        /// </summary>
        /// <param name="filter">The TCP forwarding filter.</param>
        public void setTcpForwardingFilter(IAMNetTcpForwardingFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            _tcpForwardingFilter = filter;
            _forwardingFilter = null;
            ApplyForwardingFilter();
        }

        /// <summary>
        /// Sets the TCP forwarding filter.
        /// </summary>
        /// <param name="filter">The TCP forwarding filter.</param>
        public void SetTcpForwardingFilter(IAMNetTcpForwardingFilter filter)
            => setTcpForwardingFilter(filter);

        /// <summary>
        /// Sets the agent forwarding filter (Java-style naming).
        /// </summary>
        /// <param name="filter">The agent forwarding filter.</param>
        public void setAgentForwardingFilter(IAMNetAgentForwardingFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            _agentForwardingFilter = filter;
            _forwardingFilter = null;
            ApplyForwardingFilter();
        }

        /// <summary>
        /// Sets the agent forwarding filter.
        /// </summary>
        /// <param name="filter">The agent forwarding filter.</param>
        public void SetAgentForwardingFilter(IAMNetAgentForwardingFilter filter)
            => setAgentForwardingFilter(filter);

        /// <summary>
        /// Sets the X11 forwarding filter (Java-style naming).
        /// </summary>
        /// <param name="filter">The X11 forwarding filter.</param>
        public void setX11ForwardingFilter(IAMNetX11ForwardingFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            _x11ForwardingFilter = filter;
            _forwardingFilter = null;
            ApplyForwardingFilter();
        }

        /// <summary>
        /// Sets the X11 forwarding filter.
        /// </summary>
        /// <param name="filter">The X11 forwarding filter.</param>
        public void SetX11ForwardingFilter(IAMNetX11ForwardingFilter filter)
            => setX11ForwardingFilter(filter);

        /// <summary>
        /// Gets the configured combined forwarding filter, or <c>null</c>.
        /// </summary>
        public IAMNetForwardingFilter? getForwardingFilter() => _forwardingFilter;

        /// <summary>
        /// Gets the configured TCP forwarding filter, or <c>null</c>.
        /// </summary>
        public IAMNetTcpForwardingFilter? getTcpForwardingFilter() => _tcpForwardingFilter;

        /// <summary>
        /// Gets the configured agent forwarding filter, or <c>null</c>.
        /// </summary>
        public IAMNetAgentForwardingFilter? getAgentForwardingFilter() => _agentForwardingFilter;

        /// <summary>
        /// Gets the configured X11 forwarding filter, or <c>null</c>.
        /// </summary>
        public IAMNetX11ForwardingFilter? getX11ForwardingFilter() => _x11ForwardingFilter;

        private readonly System.Collections.Generic.List<InternalPortForwardingEventListener> _portForwardingEventListeners = new();
        private readonly object _portForwardingLock = new();

        /// <summary>
        /// Registers a port forwarding event listener.
        /// </summary>
        /// <param name="listener">The port forwarding event listener.</param>
        public void addPortForwardingEventListener(IAMNetPortForwardingEventListener listener)
        {
            var internalListener = new InternalPortForwardingEventListener(listener);
            lock (_portForwardingLock)
            {
                _portForwardingEventListeners.Add(internalListener);
            }
            server.addPortForwardingEventListener(internalListener);
        }

        /// <summary>
        /// Registers a port forwarding event listener.
        /// </summary>
        public void AddPortForwardingEventListener(IAMNetPortForwardingEventListener listener)
            => addPortForwardingEventListener(listener);

        /// <summary>
        /// Removes a previously registered port forwarding event listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <returns><c>true</c> if found and removed; otherwise <c>false</c>.</returns>
        public bool removePortForwardingEventListener(IAMNetPortForwardingEventListener listener)
        {
            lock (_portForwardingLock)
            {
                for (int i = _portForwardingEventListeners.Count - 1; i >= 0; i--)
                {
                    if (_portForwardingEventListeners[i].WrappedListener == listener)
                    {
                        server.removePortForwardingEventListener(_portForwardingEventListeners[i]);
                        _portForwardingEventListeners.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a previously registered port forwarding event listener.
        /// </summary>
        public bool RemovePortForwardingEventListener(IAMNetPortForwardingEventListener listener)
            => removePortForwardingEventListener(listener);

        /// <summary>
        /// Gets the I/O service event listener, or <c>null</c> if not configured.
        /// </summary>
        public IAMNetIoServiceEventListener? getIoServiceEventListener() => _userIoServiceEventListener;

        /// <summary>
        /// Gets the connection rate limiter, or <c>null</c> if not configured.
        /// </summary>
        public IAmNetConnectionRateLimiter? getRateLimiter() => _rateLimiter;

        private void ApplyForwardingFilter()
        {
            if (_forwardingFilter != null)
            {
                server.setForwardingFilter(new InternalForwardingFilter(_forwardingFilter));
                return;
            }

            var hasTcp = _tcpForwardingFilter != null;
            var hasAgent = _agentForwardingFilter != null;
            var hasX11 = _x11ForwardingFilter != null;

            if (!hasTcp && !hasAgent && !hasX11)
                return;

            var tcp = _tcpForwardingFilter;
            var agent = _agentForwardingFilter;
            var x11 = _x11ForwardingFilter;

            server.setForwardingFilter(new InternalForwardingFilter(tcp, agent, x11));
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sessionListener"/> is null.</exception>
        private readonly System.Collections.Generic.List<InternalSessionListener> _sessionListeners = new();
        private readonly object _sessionListenerLock = new();

        /// <summary>
        /// Registers session lifecycle callbacks.
        /// </summary>
        /// <param name="sessionListener">The session listener to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sessionListener"/> is null.</exception>
        public void addSessionListener(IAMNetSessionListener sessionListener)
        {
            ArgumentNullException.ThrowIfNull(sessionListener);
            var internalListener = new InternalSessionListener(sessionListener);
            lock (_sessionListenerLock)
            {
                _sessionListeners.Add(internalListener);
            }
            server.addSessionListener(internalListener);
        }

        /// <summary>
        /// Removes a previously registered session listener.
        /// </summary>
        /// <param name="sessionListener">The session listener to remove.</param>
        /// <returns><c>true</c> if the listener was found and removed; otherwise <c>false</c>.</returns>
        public bool removeSessionListener(IAMNetSessionListener sessionListener)
        {
            lock (_sessionListenerLock)
            {
                for (int i = _sessionListeners.Count - 1; i >= 0; i--)
                {
                    if (_sessionListeners[i].WrappedListener == sessionListener)
                    {
                        server.removeSessionListener(_sessionListeners[i]);
                        _sessionListeners.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a previously registered session listener.
        /// </summary>
        public bool RemoveSessionListener(IAMNetSessionListener sessionListener) => removeSessionListener(sessionListener);

        /// <summary>
        /// Enables host-based authentication.
        /// </summary>
        /// <param name="hostBasedAuthenticator">The host-based authenticator.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostBasedAuthenticator"/> is null.</exception>
        public void setHostBasedAuthenticator(IAMNetHostBasedAuthenticator hostBasedAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(hostBasedAuthenticator);
            _hostBasedAuthenticator = hostBasedAuthenticator;
            server.setHostBasedAuthenticator(new InternalHostBasedAuthenticator(hostBasedAuthenticator));
        }

        /// <summary>
        /// Gets the configured host-based authenticator.
        /// </summary>
        /// <returns>The host-based authenticator, or <c>null</c> if not configured.</returns>
        public IAMNetHostBasedAuthenticator? getHostBasedAuthenticator() => _hostBasedAuthenticator;

        /// <summary>
        /// Enables host-based authentication.
        /// </summary>
        /// <param name="hostBasedAuthenticator">The host-based authenticator.</param>
        public void SetHostBasedAuthenticator(IAMNetHostBasedAuthenticator hostBasedAuthenticator)
        {
            setHostBasedAuthenticator(hostBasedAuthenticator);
        }

        /// <summary>
        /// Enables host-based authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, public key fingerprint, client hostname, client username, and session metadata
        /// and returns whether the host should be accepted.
        /// </param>
        public void setDelegateHostBasedAuthenticator(Func<string, string, string, string, ISshSession, bool> authenticate)
        {
            setHostBasedAuthenticator(new AMNetDelegateHostBasedAuthenticator(authenticate));
        }

        /// <summary>
        /// Enables host-based authentication using a .NET callback.
        /// </summary>
        /// <param name="authenticate">
        /// Callback that receives username, public key fingerprint, client hostname, client username, and session metadata
        /// and returns whether the host should be accepted.
        /// </param>
        public void SetDelegateHostBasedAuthenticator(Func<string, string, string, string, ISshSession, bool> authenticate)
        {
            setDelegateHostBasedAuthenticator(authenticate);
        }

        /// <summary>
        /// Enables GSSAPI/Kerberos authentication.
        /// </summary>
        /// <param name="gssapiAuthenticator">The GSSAPI authenticator.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gssapiAuthenticator"/> is null.</exception>
        public void setGssapiAuthenticator(IAMNetGssapiAuthenticator gssapiAuthenticator)
        {
            ArgumentNullException.ThrowIfNull(gssapiAuthenticator);
            _gssapiAuthenticator = gssapiAuthenticator;
            server.setGSSAuthenticator(new InternalGssapiAuthenticator(gssapiAuthenticator));
        }

        /// <summary>
        /// Gets the configured GSSAPI authenticator.
        /// </summary>
        /// <returns>The GSSAPI authenticator, or <c>null</c> if not configured.</returns>
        public IAMNetGssapiAuthenticator? getGssapiAuthenticator() => _gssapiAuthenticator;

        /// <summary>
        /// Enables GSSAPI/Kerberos authentication.
        /// </summary>
        /// <param name="gssapiAuthenticator">The GSSAPI authenticator.</param>
        public void SetGssapiAuthenticator(IAMNetGssapiAuthenticator gssapiAuthenticator)
        {
            setGssapiAuthenticator(gssapiAuthenticator);
        }

        /// <summary>
        /// Enables GSSAPI/Kerberos authentication using a .NET callback.
        /// </summary>
        /// <param name="validateIdentity">
        /// Callback that receives session and identity and returns whether the identity should be accepted.
        /// </param>
        public void setDelegateGssapiAuthenticator(Func<ISshSession, string, bool> validateIdentity)
        {
            setGssapiAuthenticator(new AMNetDelegateGssapiAuthenticator(validateIdentity));
        }

        /// <summary>
        /// Enables GSSAPI/Kerberos authentication using a .NET callback.
        /// </summary>
        /// <param name="validateIdentity">
        /// Callback that receives session and identity and returns whether the identity should be accepted.
        /// </param>
        public void SetDelegateGssapiAuthenticator(Func<ISshSession, string, bool> validateIdentity)
        {
            setDelegateGssapiAuthenticator(validateIdentity);
        }

        /// <summary>
        /// Enables one or more SFTP subsystems for incoming SSH sessions.
        /// </summary>
        /// <param name="sftpFactories">One or more SFTP subsystem factories.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sftpFactories"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sftpFactories"/> is empty.</exception>
        public void setSubsystemFactories(params AMNetSftpSubsystemFactory[] sftpFactories)
        {
            ArgumentNullException.ThrowIfNull(sftpFactories);
            if (sftpFactories.Length == 0)
                throw new ArgumentException("At least one subsystem factory is required.", nameof(sftpFactories));
            _subsystemFactories = sftpFactories;
            if (sftpFactories.Length == 1)
            {
                server.setSubsystemFactories(Collections.singletonList(sftpFactories[0].JavaFactory));
                return;
            }
            var list = new java.util.ArrayList();
            foreach (var factory in sftpFactories)
                list.add(factory.JavaFactory);
            server.setSubsystemFactories(list);
        }

        /// <summary>
        /// Gets the configured SFTP subsystem factories.
        /// </summary>
        /// <returns>The subsystem factories, or <c>null</c> if not configured.</returns>
        public IReadOnlyList<AMNetSftpSubsystemFactory>? getSubsystemFactories() => _subsystemFactories;

        /// <summary>
        /// Enables SCP command handling for incoming SSH sessions.
        /// </summary>
        /// <param name="scpFactory">The SCP command factory.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scpFactory"/> is null.</exception>
        public void setCommandFactory(AMNetScpCommandFactory scpFactory)
        {
            ArgumentNullException.ThrowIfNull(scpFactory);
            _scpCommandFactory = scpFactory;
            _commandHandler = null;
            server.setCommandFactory(scpFactory.JavaFactory);
        }

        /// <summary>
        /// Gets the configured SCP command factory, or <c>null</c> if not configured or a command handler is set instead.
        /// </summary>
        /// <returns>The SCP command factory.</returns>
        public AMNetScpCommandFactory? getCommandFactory() => _scpCommandFactory;

        /// <summary>
        /// Enables shell/exec command handling with a .NET handler.
        /// </summary>
        /// <param name="handler">The command handler for exec and shell requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
        public void setCommandHandler(IAMNetCommandHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            _commandHandler = handler;
            _scpCommandFactory = null;
            server.setCommandFactory(new InternalCommandFactory(handler));
        }

        /// <summary>
        /// Gets the configured command handler for shell/exec, or <c>null</c> if not configured.
        /// </summary>
        /// <returns>The command handler.</returns>
        public IAMNetCommandHandler? getCommandHandler() => _commandHandler;

        /// <summary>
        /// Enables shell/exec command handling with a .NET handler.
        /// </summary>
        /// <param name="handler">The command handler for exec and shell requests.</param>
        public void SetCommandHandler(IAMNetCommandHandler handler)
        {
            setCommandHandler(handler);
        }

        /// <summary>
        /// Enables exec and/or shell command handling using .NET callbacks.
        /// </summary>
        /// <param name="execHandler">Callback for exec commands, or <c>null</c> to reject all exec requests.</param>
        /// <param name="shellHandler">Callback for shell requests, or <c>null</c> to reject all shell requests.</param>
        public void setDelegateCommandHandler(
            Func<string, ISshSession, int>? execHandler = null,
            Func<ISshSession, int>? shellHandler = null)
        {
            setCommandHandler(new AMNetDelegateCommandHandler(execHandler, shellHandler));
        }

        /// <summary>
        /// Enables exec and/or shell command handling using .NET callbacks.
        /// </summary>
        /// <param name="execHandler">Callback for exec commands, or <c>null</c> to reject all exec requests.</param>
        /// <param name="shellHandler">Callback for shell requests, or <c>null</c> to reject all shell requests.</param>
        public void SetDelegateCommandHandler(
            Func<string, ISshSession, int>? execHandler = null,
            Func<ISshSession, int>? shellHandler = null)
        {
            setDelegateCommandHandler(execHandler, shellHandler);
        }

        // --- Channel listeners ---

        private readonly System.Collections.Generic.List<InternalChannelListener> _channelListeners = new();
        private readonly object _channelListenerLock = new();

        /// <summary>
        /// Registers a channel event listener.
        /// </summary>
        /// <param name="listener">The channel listener to add.</param>
        public void addChannelListener(IAMNetChannelListener listener)
        {
            ArgumentNullException.ThrowIfNull(listener);
            var internalListener = new InternalChannelListener(listener);
            lock (_channelListenerLock)
            {
                _channelListeners.Add(internalListener);
            }
            server.addChannelListener(internalListener);
        }

        /// <summary>
        /// Registers a channel event listener.
        /// </summary>
        public void AddChannelListener(IAMNetChannelListener listener)
            => addChannelListener(listener);

        /// <summary>
        /// Removes a previously registered channel listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <returns><c>true</c> if found and removed; otherwise <c>false</c>.</returns>
        public bool removeChannelListener(IAMNetChannelListener listener)
        {
            lock (_channelListenerLock)
            {
                for (int i = _channelListeners.Count - 1; i >= 0; i--)
                {
                    if (_channelListeners[i].WrappedListener == listener)
                    {
                        server.removeChannelListener(_channelListeners[i]);
                        _channelListeners.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a previously registered channel listener.
        /// </summary>
        public bool RemoveChannelListener(IAMNetChannelListener listener)
            => removeChannelListener(listener);

        // --- Service factories ---

        /// <summary>
        /// Sets the list of service factories.
        /// </summary>
        public void setServiceFactories(java.util.List? factories)
            => server.setServiceFactories(factories);

        /// <summary>
        /// Gets the list of service factories.
        /// </summary>
        public java.util.List? getServiceFactories()
            => server.getServiceFactories();

        /// <summary>
        /// Sets the list of user authentication factories.
        /// </summary>
        public void setUserAuthFactories(java.util.List? factories)
            => server.setUserAuthFactories(factories);

        /// <summary>
        /// Gets the list of user authentication factories.
        /// </summary>
        public java.util.List? getUserAuthFactories()
            => server.getUserAuthFactories();

        // --- Server attributes ---

        /// <summary>
        /// Sets a server-level attribute.
        /// </summary>
        public void setAttribute(org.apache.sshd.common.AttributeRepository.AttributeKey key, object? value)
            => server.setAttribute(key, value);

        /// <summary>
        /// Gets a server-level attribute.
        /// </summary>
        public object? getAttribute(org.apache.sshd.common.AttributeRepository.AttributeKey key)
            => server.getAttribute(key);

        // --- Simple Java interface getters/setters ---

        /// <summary>
        /// Sets the I/O service factory factory (for custom NIO/transport).
        /// </summary>
        public void setIoServiceFactoryFactory(org.apache.sshd.common.io.IoServiceFactoryFactory? factory)
        {
            _ioServiceFactoryFactory = factory;
            server.setIoServiceFactoryFactory(factory);
        }

        /// <summary>
        /// Gets the configured I/O service factory factory.
        /// </summary>
        public org.apache.sshd.common.io.IoServiceFactoryFactory? getIoServiceFactoryFactory()
            => _ioServiceFactoryFactory;

        /// <summary>
        /// Sets the scheduled executor service used for background tasks.
        /// </summary>
        public void setScheduledExecutorService(global::java.util.concurrent.ScheduledExecutorService? executor)
        {
            _scheduledExecutorService = executor;
            server.setScheduledExecutorService(executor, true);
        }

        /// <summary>
        /// Gets the configured scheduled executor service.
        /// </summary>
        public global::java.util.concurrent.ScheduledExecutorService? getScheduledExecutorService()
            => _scheduledExecutorService;

    }
}
