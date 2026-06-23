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

using org.apache.sshd.common;
using org.apache.sshd.common.helpers;
using org.apache.sshd.core;
using JavaCollection = java.util.Collection;

namespace ApacheMinaSSHD.NET.Wrapper
{
    /// <summary>
    /// Provides .NET-friendly server settings for authentication limits, sessions,
    /// keep-alives, re-keying, and SSH algorithm selection.
    /// </summary>
    public sealed class AMNetSshServerConfig
    {
        private readonly AbstractFactoryManager manager;

        internal AMNetSshServerConfig(AbstractFactoryManager manager)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        /// Applies conservative baseline limits for SFTP/SCP services.
        /// Authentication, authorization, host-key management, storage policy, and
        /// deployment hardening remain application decisions.
        /// </summary>
        public void ApplyProductionDefaults()
        {
            MAX_AUTH_REQUESTS = 5;
            AUTH_TIMEOUT = TimeSpan.FromSeconds(60);
            MAX_CONCURRENT_SESSIONS = 10;
            MAX_CONCURRENT_CHANNELS = 10;
            NIO_WORKERS = Environment.ProcessorCount * 2;
            IDLE_TIMEOUT = TimeSpan.FromMinutes(10);
            HEARTBEAT_INTERVAL = TimeSpan.FromSeconds(45);
            REKEY_BYTES_LIMIT = 1024L * 1024L * 1024L;
            REKEY_TIME_LIMIT = TimeSpan.FromHours(1);
        }

        /// <summary>
        /// Applies a modern algorithm preference order, filtered to the algorithms
        /// supported by the current Apache MINA SSHD runtime.
        /// </summary>
        public void ApplyModernAlgorithmDefaults()
        {
            SetCiphers(SelectSupportedAlgorithms(
                AMNetSshAlgorithms.Presets.ModernCiphers,
                GetSupportedCiphers(),
                "cipher"));

            SetMacs(SelectSupportedAlgorithms(
                AMNetSshAlgorithms.Presets.ModernMacs,
                GetSupportedMacs(),
                "MAC"));

            SetKeyExchangeAlgorithms(SelectSupportedAlgorithms(
                AMNetSshAlgorithms.Presets.ModernKeyExchanges,
                GetSupportedKeyExchangeAlgorithms(),
                "key exchange"));

            SetHostKeyAlgorithms(SelectSupportedAlgorithms(
                AMNetSshAlgorithms.Presets.ModernHostKeys,
                GetSupportedHostKeyAlgorithms(),
                "host key"));
        }

        #region "--- AUTHENTICATION ---"

        /// <summary>
        /// AUTH_METHODS ("auth-methods"): Configures multi-step authentication.
        /// It accepts a space-separated list of comma-separated method names.
        /// For example, "publickey,password" requires both methods to succeed sequentially.
        /// </summary>
        public string AUTH_METHODS
        {
            get
            {
                string defaultValue = CoreModuleProperties.AUTH_METHODS
                    .getOrCustomDefault(manager, null)?.ToString() ?? string.Empty;

                return PropertyResolverUtils.getStringProperty(
                    manager,
                    CoreModuleProperties.AUTH_METHODS.getName(),
                    defaultValue);
            }
            set => PropertyResolverUtils.updateProperty(manager, CoreModuleProperties.AUTH_METHODS.getName(), value);
        }

        /// <summary>
        /// Gets the configured authentication method chains in evaluation order.
        /// </summary>
        /// <returns>Read-only list of authentication method chains, where each inner list is a chain of method names.</returns>
        public IReadOnlyList<IReadOnlyList<string>> GetConfiguredAuthenticationMethods()
        {
            return AMNetSshAuthenticationMethods.Parse(AUTH_METHODS);
        }

        /// <summary>
        /// Sets the authentication method policy using one or more pre-built method chains.
        /// </summary>
        /// <param name="authenticationChains">
        /// Authentication chains such as <see cref="AMNetSshAuthenticationMethods.PublicKey"/>
        /// or values returned by <see cref="AMNetSshAuthenticationMethods.RequireAll(string[])"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationChains"/> is null.</exception>
        public void SetAuthenticationMethods(params string[] authenticationChains)
        {
            SetAuthenticationMethods((IEnumerable<string>)authenticationChains);
        }

        /// <summary>
        /// Sets the authentication method policy using one or more pre-built method chains.
        /// </summary>
        /// <param name="authenticationChains">
        /// Authentication chains such as <see cref="AMNetSshAuthenticationMethods.PublicKey"/>
        /// or values returned by <see cref="AMNetSshAuthenticationMethods.RequireAll(string[])"/>.
        /// </param>
        public void SetAuthenticationMethods(IEnumerable<string> authenticationChains)
        {
            AUTH_METHODS = AMNetSshAuthenticationMethods.AllowAny(authenticationChains);
        }

        /// <summary>
        /// Sets the authentication method policy using one or more required method groups.
        /// </summary>
        /// <param name="authenticationMethodGroups">
        /// Each group contains methods that must all succeed in order. The outer set
        /// represents alternative groups.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationMethodGroups"/> is null.</exception>
        public void SetAuthenticationMethodGroups(params IEnumerable<string>[] authenticationMethodGroups)
        {
            ArgumentNullException.ThrowIfNull(authenticationMethodGroups);

            AUTH_METHODS = AMNetSshAuthenticationMethods.AllowAny(
                authenticationMethodGroups.Select(AMNetSshAuthenticationMethods.RequireAll));
        }

        /// <summary>
        /// MAX_AUTH_REQUESTS ("max-auth-requests"): Limits authentication attempts per session.
        /// </summary>
        public int MAX_AUTH_REQUESTS
        {
            get => PropertyResolverUtils.getIntProperty(manager, CoreModuleProperties.MAX_AUTH_REQUESTS.getName(), 10);
            set => PropertyResolverUtils.updateProperty(manager, CoreModuleProperties.MAX_AUTH_REQUESTS.getName(), value);
        }

        /// <summary>
        /// AUTH_TIMEOUT ("auth-timeout"): Maximum time allowed to complete authentication.
        /// </summary>
        public TimeSpan AUTH_TIMEOUT
        {
            get
            {
                long ms = PropertyResolverUtils.getLongProperty(
                    manager,
                    CoreModuleProperties.AUTH_TIMEOUT.getName(),
                    120000L);

                return TimeSpan.FromMilliseconds(ms);
            }
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.AUTH_TIMEOUT.getName(),
                (long)value.TotalMilliseconds);
        }

        #endregion

        #region "--- SESSION & RESOURCE LIMITS ---"

        /// <summary>
        /// MAX_CONCURRENT_SESSIONS ("max-concurrent-sessions"): Limits active sessions per username.
        /// </summary>
        public int MAX_CONCURRENT_SESSIONS
        {
            get => PropertyResolverUtils.getIntProperty(
                manager,
                CoreModuleProperties.MAX_CONCURRENT_SESSIONS.getName(),
                10);
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.MAX_CONCURRENT_SESSIONS.getName(),
                value);
        }

        /// <summary>
        /// MAX_CONCURRENT_CHANNELS ("max-concurrent-channels"): Limits channels within one SSH session.
        /// </summary>
        public int MAX_CONCURRENT_CHANNELS
        {
            get => PropertyResolverUtils.getIntProperty(
                manager,
                CoreModuleProperties.MAX_CONCURRENT_CHANNELS.getName(),
                10);
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.MAX_CONCURRENT_CHANNELS.getName(),
                value);
        }

        /// <summary>
        /// NIO_WORKERS ("nio-workers"): Number of NIO worker threads for I/O operations.
        /// Must be positive. Default 2 workers per CPU core is typical.
        /// </summary>
        public int NIO_WORKERS
        {
            get => PropertyResolverUtils.getIntProperty(
                manager,
                CoreModuleProperties.NIO_WORKERS.getName(),
                Environment.ProcessorCount * 2);
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "NIO worker count must be positive.");
                }

                PropertyResolverUtils.updateProperty(
                    manager,
                    CoreModuleProperties.NIO_WORKERS.getName(),
                    value);
            }
        }

        #endregion

        #region "--- SOCKET OPTIONS ---"

        /// <summary>
        /// SOCKET_BACKLOG ("socket-backlog"): TCP socket backlog queue size.
        /// Default is 0 (system default).
        /// </summary>
        public int SOCKET_BACKLOG
        {
            get => PropertyResolverUtils.getIntProperty(
                manager,
                "socket-backlog",
                0);
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "Socket backlog must be non-negative.");
                }

                PropertyResolverUtils.updateProperty(manager, "socket-backlog", value);
            }
        }

        /// <summary>
        /// SOCKET_KEEPALIVE ("socket-keepalive"): Enable TCP keepalive on server sockets.
        /// </summary>
        public bool SOCKET_KEEPALIVE
        {
            get => PropertyResolverUtils.getBooleanProperty(
                manager,
                "socket-keepalive",
                false);
            set => PropertyResolverUtils.updateProperty(manager, "socket-keepalive", value);
        }

        /// <summary>
        /// TCP_NODELAY ("tcp-nodelay"): Disable Nagle's algorithm for lower-latency I/O.
        /// </summary>
        public bool TCP_NODELAY
        {
            get => PropertyResolverUtils.getBooleanProperty(
                manager,
                "tcp-nodelay",
                true);
            set => PropertyResolverUtils.updateProperty(manager, "tcp-nodelay", value);
        }

        #endregion

        #region "--- TIMEOUTS & KEEP-ALIVES ---"

        /// <summary>
        /// IDLE_TIMEOUT ("idle-timeout"): Closes inactive sessions after this duration.
        /// </summary>
        public TimeSpan IDLE_TIMEOUT
        {
            get
            {
                long ms = PropertyResolverUtils.getLongProperty(
                    manager,
                    CoreModuleProperties.IDLE_TIMEOUT.getName(),
                    600000L);

                return TimeSpan.FromMilliseconds(ms);
            }
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.IDLE_TIMEOUT.getName(),
                (long)value.TotalMilliseconds);
        }

        /// <summary>
        /// HEARTBEAT_INTERVAL ("heartbeat-interval"): Server keep-alive interval.
        /// Default is 45 seconds to prevent idle session resource exhaustion.
        /// Set to <see cref="TimeSpan.Zero"/> to disable heartbeats.
        /// </summary>
        public TimeSpan HEARTBEAT_INTERVAL
        {
            get
            {
                long ms = PropertyResolverUtils.getLongProperty(
                    manager,
                    CoreModuleProperties.HEARTBEAT_INTERVAL.getName(),
                    45000L);

                return TimeSpan.FromMilliseconds(ms);
            }
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.HEARTBEAT_INTERVAL.getName(),
                (long)value.TotalMilliseconds);
        }

        #endregion

        #region "--- CRYPTOGRAPHY & RE-KEYING ---"

        /// <summary>
        /// REKEY_BYTES_LIMIT ("rekey-bytes-limit"): Data volume before key renegotiation.
        /// </summary>
        public long REKEY_BYTES_LIMIT
        {
            get => PropertyResolverUtils.getLongProperty(
                manager,
                CoreModuleProperties.REKEY_BYTES_LIMIT.getName(),
                1024L * 1024L * 1024L);
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.REKEY_BYTES_LIMIT.getName(),
                value);
        }

        /// <summary>
        /// REKEY_TIME_LIMIT ("rekey-time-limit"): Elapsed time before key renegotiation.
        /// </summary>
        public TimeSpan REKEY_TIME_LIMIT
        {
            get
            {
                long ms = PropertyResolverUtils.getLongProperty(
                    manager,
                    CoreModuleProperties.REKEY_TIME_LIMIT.getName(),
                    3600000L);

                return TimeSpan.FromMilliseconds(ms);
            }
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.REKEY_TIME_LIMIT.getName(),
                (long)value.TotalMilliseconds);
        }

        #endregion

        #region "--- IDENTIFICATION ---"

        /// <summary>
        /// WELCOME_BANNER ("welcome-banner"): Optional message displayed to clients upon connection.
        /// </summary>
        public string WELCOME_BANNER
        {
            get => PropertyResolverUtils.getStringProperty(
                manager,
                CoreModuleProperties.WELCOME_BANNER.getName(),
                string.Empty);
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.WELCOME_BANNER.getName(),
                value);
        }

        /// <summary>
        /// SERVER_IDENTIFICATION ("server-identification"): Optional server identification override.
        /// </summary>
        public string SERVER_IDENTIFICATION
        {
            get => PropertyResolverUtils.getStringProperty(
                manager,
                CoreModuleProperties.SERVER_IDENTIFICATION.getName(),
                string.Empty);
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.SERVER_IDENTIFICATION.getName(),
                value);
        }

        #endregion

        #region "--- PORT FORWARDING ---"

        /// <summary>
        /// FORWARDER_BUFFER_SIZE ("forwarder-buffer-size"): Buffer size in bytes for port forwarding.
        /// Default is 32768. Minimum is 4096, maximum is 65536.
        /// </summary>
        public int FORWARDER_BUFFER_SIZE
        {
            get => PropertyResolverUtils.getIntProperty(
                manager,
                CoreModuleProperties.FORWARDER_BUFFER_SIZE.getName(),
                CoreModuleProperties.DEFAULT_FORWARDER_BUF_SIZE);
            set
            {
                if (value < CoreModuleProperties.MIN_FORWARDER_BUF_SIZE || value > CoreModuleProperties.MAX_FORWARDER_BUF_SIZE)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"FORWARDER_BUFFER_SIZE must be between {CoreModuleProperties.MIN_FORWARDER_BUF_SIZE} and {CoreModuleProperties.MAX_FORWARDER_BUF_SIZE}.");
                }
                PropertyResolverUtils.updateProperty(
                    manager,
                    CoreModuleProperties.FORWARDER_BUFFER_SIZE.getName(),
                    value);
            }
        }

        /// <summary>
        /// FORWARD_REQUEST_TIMEOUT ("tcpip-forward-request-timeout"): Maximum time to wait for a
        /// TCP/IP forwarding request to be processed. Set to <see cref="TimeSpan.Zero"/> for no timeout.
        /// </summary>
        public TimeSpan FORWARD_REQUEST_TIMEOUT
        {
            get
            {
                long ms = PropertyResolverUtils.getLongProperty(
                    manager,
                    CoreModuleProperties.FORWARD_REQUEST_TIMEOUT.getName(),
                    30000L);
                return TimeSpan.FromMilliseconds(ms);
            }
            set => PropertyResolverUtils.updateProperty(
                manager,
                CoreModuleProperties.FORWARD_REQUEST_TIMEOUT.getName(),
                (long)value.TotalMilliseconds);
        }

        #endregion

        #region "--- COMPRESSION ---"

        /// <summary>
        /// Comma-separated list of allowed compression algorithms.
        /// </summary>
        public string COMPRESSION
        {
            get
            {
                string configuredValue = PropertyResolverUtils.getStringProperty(manager, "compression", null);
                return string.IsNullOrEmpty(configuredValue)
                    ? JoinNamedResources(manager.getCompressionFactories())
                    : configuredValue;
            }
            set => PropertyResolverUtils.updateProperty(manager, "compression", value);
        }

        /// <summary>
        /// Gets compression algorithm names supported by the current runtime.
        /// </summary>
        public IReadOnlyList<string> GetSupportedCompressionAlgorithms() => GetNamedResources(manager.getCompressionFactories());

        /// <summary>
        /// Gets the configured compression algorithm names in preference order.
        /// </summary>
        public IReadOnlyList<string> GetConfiguredCompressionAlgorithms() => SplitAlgorithmList(COMPRESSION);

        /// <summary>
        /// Sets allowed compression algorithms in preference order.
        /// </summary>
        /// <param name="algorithms">Compression names such as values from <see cref="AMNetSshAlgorithms.Compression"/>.</param>
        public void SetCompressionAlgorithms(params string[] algorithms) => SetCompressionAlgorithms((IEnumerable<string>)algorithms);

        /// <summary>
        /// Sets allowed compression algorithms in preference order.
        /// </summary>
        /// <param name="algorithms">Compression names such as values from <see cref="AMNetSshAlgorithms.Compression"/>.</param>
        public void SetCompressionAlgorithms(IEnumerable<string> algorithms)
        {
            COMPRESSION = BuildValidatedAlgorithmList(algorithms, GetSupportedCompressionAlgorithms(), "compression");
        }

        #endregion

        #region "--- CRYPTOGRAPHIC ALGORITHMS ---"

        /// <summary>
        /// Comma-separated list of allowed symmetric ciphers.
        /// </summary>
        public string CIPHERS
        {
            get
            {
                string configuredValue = PropertyResolverUtils.getStringProperty(manager, "ciphers", null);
                return string.IsNullOrEmpty(configuredValue)
                    ? JoinNamedResources(manager.getCipherFactories())
                    : configuredValue;
            }
            set => PropertyResolverUtils.updateProperty(manager, "ciphers", value);
        }

        /// <summary>
        /// Comma-separated list of allowed message authentication codes.
        /// </summary>
        public string MACS
        {
            get
            {
                string configuredValue = PropertyResolverUtils.getStringProperty(manager, "macs", null);
                return string.IsNullOrEmpty(configuredValue)
                    ? JoinNamedResources(manager.getMacFactories())
                    : configuredValue;
            }
            set => PropertyResolverUtils.updateProperty(manager, "macs", value);
        }

        /// <summary>
        /// Comma-separated list of allowed key exchange algorithms.
        /// </summary>
        public string KEX_ALGORITHMS
        {
            get
            {
                string configuredValue = PropertyResolverUtils.getStringProperty(manager, "kex-algorithms", null);
                return string.IsNullOrEmpty(configuredValue)
                    ? JoinNamedResources(manager.getKeyExchangeFactories())
                    : configuredValue;
            }
            set => PropertyResolverUtils.updateProperty(manager, "kex-algorithms", value);
        }

        /// <summary>
        /// Comma-separated list of allowed signature/host-key algorithms.
        /// </summary>
        public string HOST_KEY_ALGORITHMS
        {
            get
            {
                string configuredValue = PropertyResolverUtils.getStringProperty(manager, "host-key-algorithms", null);
                return string.IsNullOrEmpty(configuredValue)
                    ? JoinNamedResources(manager.getSignatureFactories())
                    : configuredValue;
            }
            set => PropertyResolverUtils.updateProperty(manager, "host-key-algorithms", value);
        }

        #endregion

        /// <summary>
        /// Gets cipher algorithm names supported by the current runtime.
        /// </summary>
        /// <returns>Read-only list of supported cipher names.</returns>
        public IReadOnlyList<string> GetSupportedCiphers() => GetNamedResources(manager.getCipherFactories());

        /// <summary>
        /// Gets MAC algorithm names supported by the current runtime.
        /// </summary>
        /// <returns>Read-only list of supported MAC names.</returns>
        public IReadOnlyList<string> GetSupportedMacs() => GetNamedResources(manager.getMacFactories());

        /// <summary>
        /// Gets key exchange algorithm names supported by the current runtime.
        /// </summary>
        /// <returns>Read-only list of supported key exchange names.</returns>
        public IReadOnlyList<string> GetSupportedKeyExchangeAlgorithms() => GetNamedResources(manager.getKeyExchangeFactories());

        /// <summary>
        /// Gets host key/signature algorithm names supported by the current runtime.
        /// </summary>
        /// <returns>Read-only list of supported host key names.</returns>
        public IReadOnlyList<string> GetSupportedHostKeyAlgorithms() => GetNamedResources(manager.getSignatureFactories());

        /// <summary>
        /// Gets the configured cipher algorithm names in preference order.
        /// </summary>
        /// <returns>Read-only list of configured cipher names.</returns>
        public IReadOnlyList<string> GetConfiguredCiphers() => SplitAlgorithmList(CIPHERS);

        /// <summary>
        /// Gets the configured MAC algorithm names in preference order.
        /// </summary>
        /// <returns>Read-only list of configured MAC names.</returns>
        public IReadOnlyList<string> GetConfiguredMacs() => SplitAlgorithmList(MACS);

        /// <summary>
        /// Gets the configured key exchange algorithm names in preference order.
        /// </summary>
        /// <returns>Read-only list of configured key exchange names.</returns>
        public IReadOnlyList<string> GetConfiguredKeyExchangeAlgorithms() => SplitAlgorithmList(KEX_ALGORITHMS);

        /// <summary>
        /// Gets the configured host key/signature algorithm names in preference order.
        /// </summary>
        /// <returns>Read-only list of configured host key names.</returns>
        public IReadOnlyList<string> GetConfiguredHostKeyAlgorithms() => SplitAlgorithmList(HOST_KEY_ALGORITHMS);

        /// <summary>
        /// Sets allowed cipher algorithms in preference order.
        /// </summary>
        /// <param name="ciphers">Cipher names such as values from <see cref="AMNetSshAlgorithms.Ciphers"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ciphers"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested cipher is not supported.</exception>
        public void SetCiphers(params string[] ciphers) => SetCiphers((IEnumerable<string>)ciphers);

        /// <summary>
        /// Sets allowed cipher algorithms in preference order.
        /// </summary>
        /// <param name="ciphers">Cipher names such as values from <see cref="AMNetSshAlgorithms.Ciphers"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ciphers"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested cipher is not supported.</exception>
        public void SetCiphers(IEnumerable<string> ciphers)
        {
            CIPHERS = BuildValidatedAlgorithmList(ciphers, GetSupportedCiphers(), "cipher");
        }

        /// <summary>
        /// Sets allowed MAC algorithms in preference order.
        /// </summary>
        /// <param name="macs">MAC names such as values from <see cref="AMNetSshAlgorithms.Macs"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macs"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested MAC is not supported.</exception>
        public void SetMacs(params string[] macs) => SetMacs((IEnumerable<string>)macs);

        /// <summary>
        /// Sets allowed MAC algorithms in preference order.
        /// </summary>
        /// <param name="macs">MAC names such as values from <see cref="AMNetSshAlgorithms.Macs"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="macs"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested MAC is not supported.</exception>
        public void SetMacs(IEnumerable<string> macs)
        {
            MACS = BuildValidatedAlgorithmList(macs, GetSupportedMacs(), "MAC");
        }

        /// <summary>
        /// Sets allowed key exchange algorithms in preference order.
        /// </summary>
        /// <param name="keyExchangeAlgorithms">Key exchange names such as values from <see cref="AMNetSshAlgorithms.KeyExchange"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyExchangeAlgorithms"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested key exchange algorithm is not supported.</exception>
        public void SetKeyExchangeAlgorithms(params string[] keyExchangeAlgorithms) =>
            SetKeyExchangeAlgorithms((IEnumerable<string>)keyExchangeAlgorithms);

        /// <summary>
        /// Sets allowed key exchange algorithms in preference order.
        /// </summary>
        /// <param name="keyExchangeAlgorithms">Key exchange names such as values from <see cref="AMNetSshAlgorithms.KeyExchange"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyExchangeAlgorithms"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested key exchange algorithm is not supported.</exception>
        public void SetKeyExchangeAlgorithms(IEnumerable<string> keyExchangeAlgorithms)
        {
            KEX_ALGORITHMS = BuildValidatedAlgorithmList(
                keyExchangeAlgorithms,
                GetSupportedKeyExchangeAlgorithms(),
                "key exchange");
        }

        /// <summary>
        /// Sets allowed host key/signature algorithms in preference order.
        /// </summary>
        /// <param name="hostKeyAlgorithms">Host key names such as values from <see cref="AMNetSshAlgorithms.HostKeys"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostKeyAlgorithms"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested host key algorithm is not supported.</exception>
        public void SetHostKeyAlgorithms(params string[] hostKeyAlgorithms) =>
            SetHostKeyAlgorithms((IEnumerable<string>)hostKeyAlgorithms);

        /// <summary>
        /// Sets allowed host key/signature algorithms in preference order.
        /// </summary>
        /// <param name="hostKeyAlgorithms">Host key names such as values from <see cref="AMNetSshAlgorithms.HostKeys"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hostKeyAlgorithms"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any requested host key algorithm is not supported.</exception>
        public void SetHostKeyAlgorithms(IEnumerable<string> hostKeyAlgorithms)
        {
            HOST_KEY_ALGORITHMS = BuildValidatedAlgorithmList(
                hostKeyAlgorithms,
                GetSupportedHostKeyAlgorithms(),
                "host key");
        }

        private static string JoinNamedResources(JavaCollection? factories)
        {
            if (factories == null)
            {
                return string.Empty;
            }

            return string.Join(
                ",",
                factories.toArray()
                    .Select(factory => ((NamedResource)factory).getName()));
        }

        private static IReadOnlyList<string> GetNamedResources(JavaCollection? factories)
        {
            if (factories == null)
            {
                return Array.Empty<string>();
            }

            return factories.toArray()
                .Select(factory => ((NamedResource)factory).getName())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray();
        }

        private static IReadOnlyList<string> SplitAlgorithmList(string? algorithms)
        {
            return (algorithms ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(algorithm => !string.IsNullOrWhiteSpace(algorithm))
                .ToArray();
        }

        private static string BuildValidatedAlgorithmList(
            IEnumerable<string> requestedAlgorithms,
            IReadOnlyList<string> supportedAlgorithms,
            string algorithmType)
        {
            ArgumentNullException.ThrowIfNull(requestedAlgorithms);

            var supportedLookup = supportedAlgorithms
                .Where(algorithm => !string.IsNullOrWhiteSpace(algorithm))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(algorithm => algorithm, StringComparer.OrdinalIgnoreCase);

            if (supportedLookup.Count == 0)
            {
                throw new InvalidOperationException($"No supported {algorithmType} algorithms were reported by the server.");
            }

            var selected = new System.Collections.Generic.List<string>();
            var unsupported = new System.Collections.Generic.List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string requestedAlgorithm in ExpandAlgorithmList(requestedAlgorithms))
            {
                if (supportedLookup.TryGetValue(requestedAlgorithm, out string? canonicalAlgorithm))
                {
                    if (seen.Add(canonicalAlgorithm))
                    {
                        selected.Add(canonicalAlgorithm);
                    }
                }
                else
                {
                    unsupported.Add(requestedAlgorithm);
                }
            }

            if (selected.Count == 0)
            {
                throw new ArgumentException($"At least one supported {algorithmType} algorithm is required.");
            }

            if (unsupported.Count > 0)
            {
                throw new ArgumentException(
                    $"Unsupported {algorithmType} algorithm(s): {string.Join(", ", unsupported)}. " +
                    $"Supported values: {string.Join(", ", supportedLookup.Keys)}.");
            }

            return string.Join(",", selected);
        }

        private static IReadOnlyList<string> SelectSupportedAlgorithms(
            IEnumerable<string> requestedAlgorithms,
            IReadOnlyList<string> supportedAlgorithms,
            string algorithmType)
        {
            var supportedLookup = supportedAlgorithms
                .Where(algorithm => !string.IsNullOrWhiteSpace(algorithm))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(algorithm => algorithm, StringComparer.OrdinalIgnoreCase);

            var selected = new System.Collections.Generic.List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string requestedAlgorithm in ExpandAlgorithmList(requestedAlgorithms))
            {
                if (supportedLookup.TryGetValue(requestedAlgorithm, out string? canonicalAlgorithm)
                    && seen.Add(canonicalAlgorithm))
                {
                    selected.Add(canonicalAlgorithm);
                }
            }

            if (selected.Count == 0)
            {
                throw new InvalidOperationException(
                    $"None of the preferred {algorithmType} algorithms are supported by the current server runtime.");
            }

            return selected;
        }

        private static IEnumerable<string> ExpandAlgorithmList(IEnumerable<string> algorithms)
        {
            foreach (string algorithmGroup in algorithms)
            {
                foreach (string algorithm in SplitAlgorithmList(algorithmGroup))
                {
                    yield return algorithm;
                }
            }
        }

        #region "--- PROPERTY CONFIGURATION ---"

        /// <summary>
        /// Sets a named property on the underlying server configuration.
        /// Use for advanced settings not exposed by a dedicated wrapper property,
        /// such as channel window sizes or bandwidth limits.
        /// </summary>
        /// <param name="key">The property key (e.g., "max-packet-size", "window-size").</param>
        /// <param name="value">The property value (will be converted via <c>toString()</c>).</param>
        public void SetProperty(string key, object value)
        {
            PropertyResolverUtils.updateProperty(manager, key, value);
        }

        /// <summary>
        /// Gets a named property from the underlying server configuration as a string.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="defaultValue">Default value if the property is not set.</param>
        /// <returns>The property value, or <paramref name="defaultValue"/> if unset.</returns>
        public string GetProperty(string key, string? defaultValue = null)
        {
            return PropertyResolverUtils.getStringProperty(manager, key, defaultValue);
        }

        /// <summary>
        /// Gets a named integer property from the underlying server configuration.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="defaultValue">Default value if the property is not set.</param>
        /// <returns>The property value, or <paramref name="defaultValue"/> if unset.</returns>
        public int GetIntProperty(string key, int defaultValue = 0)
        {
            return PropertyResolverUtils.getIntProperty(manager, key, defaultValue);
        }

        /// <summary>
        /// Gets a named long integer property from the underlying server configuration.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="defaultValue">Default value if the property is not set.</param>
        /// <returns>The property value, or <paramref name="defaultValue"/> if unset.</returns>
        public long GetLongProperty(string key, long defaultValue = 0)
        {
            return PropertyResolverUtils.getLongProperty(manager, key, defaultValue);
        }

        /// <summary>
        /// Gets a named boolean property from the underlying server configuration.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="defaultValue">Default value if the property is not set.</param>
        /// <returns>The property value, or <paramref name="defaultValue"/> if unset.</returns>
        public bool GetBoolProperty(string key, bool defaultValue = false)
        {
            return PropertyResolverUtils.getBooleanProperty(manager, key, defaultValue);
        }

        #endregion
    }
}
