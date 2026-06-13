// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper
{
    /// <summary>
    /// Common SSH algorithm names and presets for configuring <see cref="AMNetSshServerConfig"/>
    /// without importing Apache MINA or Java types.
    /// </summary>
    /// <remarks>
    /// Runtime availability depends on Apache MINA SSHD, IKVM, and the configured
    /// security providers. See docs/STANDARDS-AND-ALGORITHMS.md for RFC references.
    /// </remarks>
    public static class AMNetSshAlgorithms
    {
        /// <summary>
        /// SSH symmetric cipher algorithm names, including RFC-backed AES modes and
        /// OpenSSH extension names.
        /// </summary>
        public static class Ciphers
        {
            /// <summary>ChaCha20-Poly1305 authenticated encryption using the OpenSSH extension name.</summary>
            public const string ChaCha20Poly1305 = "chacha20-poly1305@openssh.com";
            /// <summary>AES-256 GCM authenticated encryption for SSH, described by RFC 5647.</summary>
            public const string Aes256Gcm = "aes256-gcm@openssh.com";
            /// <summary>AES-128 GCM authenticated encryption for SSH, described by RFC 5647.</summary>
            public const string Aes128Gcm = "aes128-gcm@openssh.com";
            /// <summary>AES-256 CTR mode encryption for SSH, described by RFC 4344.</summary>
            public const string Aes256Ctr = "aes256-ctr";
            /// <summary>AES-192 CTR mode encryption for SSH, described by RFC 4344.</summary>
            public const string Aes192Ctr = "aes192-ctr";
            /// <summary>AES-128 CTR mode encryption for SSH, described by RFC 4344.</summary>
            public const string Aes128Ctr = "aes128-ctr";
        }

        /// <summary>
        /// SSH message authentication code algorithm names, including HMAC-SHA2 from
        /// RFC 6668 and OpenSSH encrypt-then-MAC extension names.
        /// </summary>
        public static class Macs
        {
            /// <summary>HMAC-SHA2-512 encrypt-then-MAC using the OpenSSH extension name.</summary>
            public const string HmacSha512Etm = "hmac-sha2-512-etm@openssh.com";
            /// <summary>HMAC-SHA2-256 encrypt-then-MAC using the OpenSSH extension name.</summary>
            public const string HmacSha256Etm = "hmac-sha2-256-etm@openssh.com";
            /// <summary>HMAC-SHA2-512 for SSH, described by RFC 6668.</summary>
            public const string HmacSha512 = "hmac-sha2-512";
            /// <summary>HMAC-SHA2-256 for SSH, described by RFC 6668.</summary>
            public const string HmacSha256 = "hmac-sha2-256";
        }

        /// <summary>
        /// SSH key exchange algorithm names, including Curve25519, ECDH, and MODP
        /// Diffie-Hellman methods.
        /// </summary>
        public static class KeyExchange
        {
            /// <summary>Curve25519 SHA-256 key exchange for SSH, described by RFC 8731.</summary>
            public const string Curve25519Sha256 = "curve25519-sha256";
            /// <summary>Historical OpenSSH/libssh Curve25519 SHA-256 key exchange name.</summary>
            public const string Curve25519Sha256LibSsh = "curve25519-sha256@libssh.org";
            /// <summary>ECDH over NIST P-521 for SSH, described by RFC 5656.</summary>
            public const string EcdhNistp521 = "ecdh-sha2-nistp521";
            /// <summary>ECDH over NIST P-384 for SSH, described by RFC 5656.</summary>
            public const string EcdhNistp384 = "ecdh-sha2-nistp384";
            /// <summary>ECDH over NIST P-256 for SSH, described by RFC 5656.</summary>
            public const string EcdhNistp256 = "ecdh-sha2-nistp256";
            /// <summary>Diffie-Hellman group 18 with SHA-512 for SSH, described by RFC 8268.</summary>
            public const string DiffieHellmanGroup18Sha512 = "diffie-hellman-group18-sha512";
            /// <summary>Diffie-Hellman group 16 with SHA-512 for SSH, described by RFC 8268.</summary>
            public const string DiffieHellmanGroup16Sha512 = "diffie-hellman-group16-sha512";
            /// <summary>Diffie-Hellman group 14 with SHA-256 for SSH, described by RFC 8268.</summary>
            public const string DiffieHellmanGroup14Sha256 = "diffie-hellman-group14-sha256";
        }

        /// <summary>
        /// SSH host key/signature algorithm names advertised to clients.
        /// </summary>
        public static class HostKeys
        {
            /// <summary>Ed25519 host key signature for SSH, described by RFC 8709.</summary>
            public const string Ed25519 = "ssh-ed25519";
            /// <summary>ECDSA NIST P-521 host key signature for SSH, described by RFC 5656.</summary>
            public const string EcdsaNistp521 = "ecdsa-sha2-nistp521";
            /// <summary>ECDSA NIST P-384 host key signature for SSH, described by RFC 5656.</summary>
            public const string EcdsaNistp384 = "ecdsa-sha2-nistp384";
            /// <summary>ECDSA NIST P-256 host key signature for SSH, described by RFC 5656.</summary>
            public const string EcdsaNistp256 = "ecdsa-sha2-nistp256";
            /// <summary>RSA SHA-512 host key signature for SSH, described by RFC 8332.</summary>
            public const string RsaSha512 = "rsa-sha2-512";
            /// <summary>RSA SHA-256 host key signature for SSH, described by RFC 8332.</summary>
            public const string RsaSha256 = "rsa-sha2-256";
            /// <summary>Legacy RSA SHA-1 host key signature from RFC 4253. Prefer RSA SHA-2 where clients support it.</summary>
            public const string SshRsa = "ssh-rsa";
        }

        /// <summary>
        /// Host key generation algorithm names.
        /// </summary>
        public static class HostKeyAlgorithms
        {
            /// <summary>RSA host key generation. Prefer RSA SHA-2 signatures from RFC 8332.</summary>
            public const string Rsa = "RSA";
            /// <summary>Legacy DSA host key generation. Prefer RSA, ECDSA, or Ed25519 for new deployments.</summary>
            public const string Dsa = "DSA";
            /// <summary>ECDSA host key generation, described by RFC 5656.</summary>
            public const string Ecdsa = "EC";
            /// <summary>Ed25519 host key generation when supported by the runtime, described by RFC 8709.</summary>
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
