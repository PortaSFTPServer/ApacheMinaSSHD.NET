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
        /// </summary>
        public TimeSpan HEARTBEAT_INTERVAL
        {
            get
            {
                long ms = PropertyResolverUtils.getLongProperty(
                    manager,
                    CoreModuleProperties.HEARTBEAT_INTERVAL.getName(),
                    0L);

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
        public IReadOnlyList<string> GetSupportedCiphers() => GetNamedResources(manager.getCipherFactories());

        /// <summary>
        /// Gets MAC algorithm names supported by the current runtime.
        /// </summary>
        public IReadOnlyList<string> GetSupportedMacs() => GetNamedResources(manager.getMacFactories());

        /// <summary>
        /// Gets key exchange algorithm names supported by the current runtime.
        /// </summary>
        public IReadOnlyList<string> GetSupportedKeyExchangeAlgorithms() => GetNamedResources(manager.getKeyExchangeFactories());

        /// <summary>
        /// Gets host key/signature algorithm names supported by the current runtime.
        /// </summary>
        public IReadOnlyList<string> GetSupportedHostKeyAlgorithms() => GetNamedResources(manager.getSignatureFactories());

        /// <summary>
        /// Gets the configured cipher algorithm names in preference order.
        /// </summary>
        public IReadOnlyList<string> GetConfiguredCiphers() => SplitAlgorithmList(CIPHERS);

        /// <summary>
        /// Gets the configured MAC algorithm names in preference order.
        /// </summary>
        public IReadOnlyList<string> GetConfiguredMacs() => SplitAlgorithmList(MACS);

        /// <summary>
        /// Gets the configured key exchange algorithm names in preference order.
        /// </summary>
        public IReadOnlyList<string> GetConfiguredKeyExchangeAlgorithms() => SplitAlgorithmList(KEX_ALGORITHMS);

        /// <summary>
        /// Gets the configured host key/signature algorithm names in preference order.
        /// </summary>
        public IReadOnlyList<string> GetConfiguredHostKeyAlgorithms() => SplitAlgorithmList(HOST_KEY_ALGORITHMS);

        /// <summary>
        /// Sets allowed cipher algorithms in preference order.
        /// </summary>
        /// <param name="ciphers">Cipher names such as values from <see cref="AMNetSshAlgorithms.Ciphers"/>.</param>
        public void SetCiphers(params string[] ciphers) => SetCiphers((IEnumerable<string>)ciphers);

        /// <summary>
        /// Sets allowed cipher algorithms in preference order.
        /// </summary>
        /// <param name="ciphers">Cipher names such as values from <see cref="AMNetSshAlgorithms.Ciphers"/>.</param>
        /// <exception cref="ArgumentException">Thrown when any requested cipher is not supported.</exception>
        public void SetCiphers(IEnumerable<string> ciphers)
        {
            CIPHERS = BuildValidatedAlgorithmList(ciphers, GetSupportedCiphers(), "cipher");
        }

        /// <summary>
        /// Sets allowed MAC algorithms in preference order.
        /// </summary>
        /// <param name="macs">MAC names such as values from <see cref="AMNetSshAlgorithms.Macs"/>.</param>
        public void SetMacs(params string[] macs) => SetMacs((IEnumerable<string>)macs);

        /// <summary>
        /// Sets allowed MAC algorithms in preference order.
        /// </summary>
        /// <param name="macs">MAC names such as values from <see cref="AMNetSshAlgorithms.Macs"/>.</param>
        /// <exception cref="ArgumentException">Thrown when any requested MAC is not supported.</exception>
        public void SetMacs(IEnumerable<string> macs)
        {
            MACS = BuildValidatedAlgorithmList(macs, GetSupportedMacs(), "MAC");
        }

        /// <summary>
        /// Sets allowed key exchange algorithms in preference order.
        /// </summary>
        /// <param name="keyExchangeAlgorithms">Key exchange names such as values from <see cref="AMNetSshAlgorithms.KeyExchange"/>.</param>
        public void SetKeyExchangeAlgorithms(params string[] keyExchangeAlgorithms) =>
            SetKeyExchangeAlgorithms((IEnumerable<string>)keyExchangeAlgorithms);

        /// <summary>
        /// Sets allowed key exchange algorithms in preference order.
        /// </summary>
        /// <param name="keyExchangeAlgorithms">Key exchange names such as values from <see cref="AMNetSshAlgorithms.KeyExchange"/>.</param>
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
        public void SetHostKeyAlgorithms(params string[] hostKeyAlgorithms) =>
            SetHostKeyAlgorithms((IEnumerable<string>)hostKeyAlgorithms);

        /// <summary>
        /// Sets allowed host key/signature algorithms in preference order.
        /// </summary>
        /// <param name="hostKeyAlgorithms">Host key names such as values from <see cref="AMNetSshAlgorithms.HostKeys"/>.</param>
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
    }
}
