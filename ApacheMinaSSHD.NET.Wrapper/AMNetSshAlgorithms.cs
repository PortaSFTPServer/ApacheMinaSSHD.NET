namespace ApacheMinaSSHD.NET.Wrapper
{
    /// <summary>
    /// Common SSH algorithm names and presets for configuring <see cref="AMNetSshServerConfig"/>
    /// without importing Apache MINA or Java types.
    /// </summary>
    public static class AMNetSshAlgorithms
    {
        /// <summary>
        /// SSH symmetric cipher algorithm names.
        /// </summary>
        public static class Ciphers
        {
            /// <summary>ChaCha20-Poly1305 authenticated encryption.</summary>
            public const string ChaCha20Poly1305 = "chacha20-poly1305@openssh.com";
            /// <summary>AES-256 GCM authenticated encryption.</summary>
            public const string Aes256Gcm = "aes256-gcm@openssh.com";
            /// <summary>AES-128 GCM authenticated encryption.</summary>
            public const string Aes128Gcm = "aes128-gcm@openssh.com";
            /// <summary>AES-256 CTR mode encryption.</summary>
            public const string Aes256Ctr = "aes256-ctr";
            /// <summary>AES-192 CTR mode encryption.</summary>
            public const string Aes192Ctr = "aes192-ctr";
            /// <summary>AES-128 CTR mode encryption.</summary>
            public const string Aes128Ctr = "aes128-ctr";
        }

        /// <summary>
        /// SSH message authentication code algorithm names.
        /// </summary>
        public static class Macs
        {
            /// <summary>HMAC-SHA2-512 encrypt-then-MAC.</summary>
            public const string HmacSha512Etm = "hmac-sha2-512-etm@openssh.com";
            /// <summary>HMAC-SHA2-256 encrypt-then-MAC.</summary>
            public const string HmacSha256Etm = "hmac-sha2-256-etm@openssh.com";
            /// <summary>HMAC-SHA2-512.</summary>
            public const string HmacSha512 = "hmac-sha2-512";
            /// <summary>HMAC-SHA2-256.</summary>
            public const string HmacSha256 = "hmac-sha2-256";
        }

        /// <summary>
        /// SSH key exchange algorithm names.
        /// </summary>
        public static class KeyExchange
        {
            /// <summary>Curve25519 SHA-256 key exchange.</summary>
            public const string Curve25519Sha256 = "curve25519-sha256";
            /// <summary>OpenSSH/libssh Curve25519 SHA-256 key exchange name.</summary>
            public const string Curve25519Sha256LibSsh = "curve25519-sha256@libssh.org";
            /// <summary>ECDH over NIST P-521.</summary>
            public const string EcdhNistp521 = "ecdh-sha2-nistp521";
            /// <summary>ECDH over NIST P-384.</summary>
            public const string EcdhNistp384 = "ecdh-sha2-nistp384";
            /// <summary>ECDH over NIST P-256.</summary>
            public const string EcdhNistp256 = "ecdh-sha2-nistp256";
            /// <summary>Diffie-Hellman group 18 with SHA-512.</summary>
            public const string DiffieHellmanGroup18Sha512 = "diffie-hellman-group18-sha512";
            /// <summary>Diffie-Hellman group 16 with SHA-512.</summary>
            public const string DiffieHellmanGroup16Sha512 = "diffie-hellman-group16-sha512";
            /// <summary>Diffie-Hellman group 14 with SHA-256.</summary>
            public const string DiffieHellmanGroup14Sha256 = "diffie-hellman-group14-sha256";
        }

        /// <summary>
        /// SSH host key/signature algorithm names advertised to clients.
        /// </summary>
        public static class HostKeys
        {
            /// <summary>Ed25519 host key signature.</summary>
            public const string Ed25519 = "ssh-ed25519";
            /// <summary>ECDSA NIST P-521 host key signature.</summary>
            public const string EcdsaNistp521 = "ecdsa-sha2-nistp521";
            /// <summary>ECDSA NIST P-384 host key signature.</summary>
            public const string EcdsaNistp384 = "ecdsa-sha2-nistp384";
            /// <summary>ECDSA NIST P-256 host key signature.</summary>
            public const string EcdsaNistp256 = "ecdsa-sha2-nistp256";
            /// <summary>RSA SHA-512 host key signature.</summary>
            public const string RsaSha512 = "rsa-sha2-512";
            /// <summary>RSA SHA-256 host key signature.</summary>
            public const string RsaSha256 = "rsa-sha2-256";
            /// <summary>Legacy RSA SHA-1 host key signature. Prefer RSA SHA-2 where clients support it.</summary>
            public const string SshRsa = "ssh-rsa";
        }

        /// <summary>
        /// Host key generation algorithm names.
        /// </summary>
        public static class HostKeyAlgorithms
        {
            /// <summary>RSA host key generation.</summary>
            public const string Rsa = "RSA";
            /// <summary>DSA host key generation. Prefer RSA, ECDSA, or Ed25519 for new deployments.</summary>
            public const string Dsa = "DSA";
            /// <summary>ECDSA host key generation.</summary>
            public const string Ecdsa = "EC";
            /// <summary>Ed25519 host key generation when supported by the runtime.</summary>
            public const string Ed25519 = "EdDSA";
        }

        /// <summary>
        /// Recommended algorithm preference lists.
        /// </summary>
        public static class Presets
        {
            /// <summary>
            /// Modern cipher preference order, filtered by <see cref="AMNetSshServerConfig.ApplyModernAlgorithmDefaults"/>.
            /// </summary>
            public static IReadOnlyList<string> ModernCiphers { get; } =
            [
                Ciphers.ChaCha20Poly1305,
                Ciphers.Aes256Gcm,
                Ciphers.Aes128Gcm,
                Ciphers.Aes256Ctr,
                Ciphers.Aes192Ctr,
                Ciphers.Aes128Ctr
            ];

            /// <summary>
            /// Modern MAC preference order, filtered by <see cref="AMNetSshServerConfig.ApplyModernAlgorithmDefaults"/>.
            /// </summary>
            public static IReadOnlyList<string> ModernMacs { get; } =
            [
                Macs.HmacSha512Etm,
                Macs.HmacSha256Etm,
                Macs.HmacSha512,
                Macs.HmacSha256
            ];

            /// <summary>
            /// Modern key exchange preference order, filtered by <see cref="AMNetSshServerConfig.ApplyModernAlgorithmDefaults"/>.
            /// </summary>
            public static IReadOnlyList<string> ModernKeyExchanges { get; } =
            [
                KeyExchange.Curve25519Sha256,
                KeyExchange.Curve25519Sha256LibSsh,
                KeyExchange.EcdhNistp521,
                KeyExchange.EcdhNistp384,
                KeyExchange.EcdhNistp256,
                KeyExchange.DiffieHellmanGroup18Sha512,
                KeyExchange.DiffieHellmanGroup16Sha512,
                KeyExchange.DiffieHellmanGroup14Sha256
            ];

            /// <summary>
            /// Modern host key/signature preference order, filtered by <see cref="AMNetSshServerConfig.ApplyModernAlgorithmDefaults"/>.
            /// </summary>
            public static IReadOnlyList<string> ModernHostKeys { get; } =
            [
                HostKeys.Ed25519,
                HostKeys.EcdsaNistp521,
                HostKeys.EcdsaNistp384,
                HostKeys.EcdsaNistp256,
                HostKeys.RsaSha512,
                HostKeys.RsaSha256
            ];
        }
    }
}
