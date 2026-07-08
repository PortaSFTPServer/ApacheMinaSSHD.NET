# Standards and Supported Algorithms

ApacheMinaSSHD.NET — a .NET SFTP server library and C# wrapper created by
[SERALYNX LLC](https://seralynx.com/) (the team behind
**[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)**) —
wraps Apache MINA SSHD through IKVM. This document lists the SSH/SFTP/SCP
protocol references and the public algorithm names exposed by the .NET wrapper.

The constants below are the wrapper's .NET-facing names for SSH algorithm
configuration. Actual runtime availability can vary by Apache MINA SSHD version,
IKVM runtime behavior, Java security providers, platform crypto support, and the
developer's server configuration. Application code should inspect runtime support
before enforcing a strict policy.

## Protocol References

| Area | Reference | Notes |
| --- | --- | --- |
| SSH architecture | [RFC 4251](https://www.rfc-editor.org/rfc/rfc4251) | Defines SSH architecture, terminology, host keys, and algorithm naming. |
| SSH user authentication | [RFC 4252](https://www.rfc-editor.org/rfc/rfc4252) | Defines SSH user authentication, including password and public-key authentication. |
| SSH transport | [RFC 4253](https://www.rfc-editor.org/rfc/rfc4253) | Defines SSH transport, key exchange, server authentication, encryption, and MAC negotiation. |
| SSH connection protocol | [RFC 4254](https://www.rfc-editor.org/rfc/rfc4254) | Defines SSH channels used by subsystems and command execution. |
| AES CTR modes | [RFC 4344](https://www.rfc-editor.org/rfc/rfc4344) | Defines CTR encryption modes for SSH transport. |
| AES GCM modes | [RFC 5647](https://www.rfc-editor.org/rfc/rfc5647) | Defines AES-GCM authenticated encryption for SSH transport. |
| ECDH and ECDSA | [RFC 5656](https://www.rfc-editor.org/rfc/rfc5656) | Defines elliptic-curve key exchange and public-key algorithms for SSH. |
| HMAC-SHA2 | [RFC 6668](https://www.rfc-editor.org/rfc/rfc6668) | Defines HMAC-SHA2-256 and HMAC-SHA2-512 for SSH transport integrity. |
| MODP DH groups | [RFC 8268](https://www.rfc-editor.org/rfc/rfc8268) | Defines additional Diffie-Hellman groups for SSH key exchange. |
| RSA SHA-2 signatures | [RFC 8332](https://www.rfc-editor.org/rfc/rfc8332) | Defines RSA SHA-256 and SHA-512 signature algorithms for SSH. |
| Ed25519 and Ed448 | [RFC 8709](https://www.rfc-editor.org/rfc/rfc8709) | Defines Ed25519 and Ed448 public-key algorithms for SSH. |
| Curve25519 and Curve448 KEX | [RFC 8731](https://www.rfc-editor.org/rfc/rfc8731) | Defines Curve25519 and Curve448 key exchange methods for SSH. |
| KEX recommendations | [RFC 9142](https://www.rfc-editor.org/rfc/rfc9142) | Updates and recommends SSH key exchange methods. |
| SFTP subsystem | [draft-ietf-secsh-filexfer-02](https://datatracker.ietf.org/doc/html/draft-ietf-secsh-filexfer-02) | SFTP is based on SSH File Transfer Protocol drafts, not a final IETF RFC. |
| SCP command protocol | [RFC 4254](https://www.rfc-editor.org/rfc/rfc4254) | SCP has no standalone IETF RFC; it is commonly implemented as an SSH command-channel protocol. |

## Runtime Inspection

Developers can inspect the algorithms exposed by the current runtime without
importing Apache MINA, IKVM, or Java types:

```csharp
IReadOnlyList<string> ciphers = server.Config.GetSupportedCiphers();
IReadOnlyList<string> macs = server.Config.GetSupportedMacs();
IReadOnlyList<string> kex = server.Config.GetSupportedKeyExchangeAlgorithms();
IReadOnlyList<string> hostKeys = server.Config.GetSupportedHostKeyAlgorithms();

server.Config.ApplyModernAlgorithmDefaults();
```

`ApplyModernAlgorithmDefaults()` applies the wrapper's preferred modern ordering
after filtering out algorithms that are not supported by the current runtime.

## Cipher Constants

| Wrapper constant | SSH algorithm name | Standards note |
| --- | --- | --- |
| `AMNetSshAlgorithms.Ciphers.ChaCha20Poly1305` | `chacha20-poly1305@openssh.com` | OpenSSH extension name; widely implemented but not defined by an IETF SSH RFC. |
| `AMNetSshAlgorithms.Ciphers.Aes256Gcm` | `aes256-gcm@openssh.com` | AES-GCM for SSH is described by RFC 5647. |
| `AMNetSshAlgorithms.Ciphers.Aes128Gcm` | `aes128-gcm@openssh.com` | AES-GCM for SSH is described by RFC 5647. |
| `AMNetSshAlgorithms.Ciphers.Aes256Ctr` | `aes256-ctr` | AES CTR mode for SSH is described by RFC 4344. |
| `AMNetSshAlgorithms.Ciphers.Aes192Ctr` | `aes192-ctr` | AES CTR mode for SSH is described by RFC 4344. |
| `AMNetSshAlgorithms.Ciphers.Aes128Ctr` | `aes128-ctr` | AES CTR mode for SSH is described by RFC 4344. |

## MAC Constants

| Wrapper constant | SSH algorithm name | Standards note |
| --- | --- | --- |
| `AMNetSshAlgorithms.Macs.HmacSha512Etm` | `hmac-sha2-512-etm@openssh.com` | OpenSSH encrypt-then-MAC extension using HMAC-SHA2-512. |
| `AMNetSshAlgorithms.Macs.HmacSha256Etm` | `hmac-sha2-256-etm@openssh.com` | OpenSSH encrypt-then-MAC extension using HMAC-SHA2-256. |
| `AMNetSshAlgorithms.Macs.HmacSha512` | `hmac-sha2-512` | HMAC-SHA2 for SSH is described by RFC 6668. |
| `AMNetSshAlgorithms.Macs.HmacSha256` | `hmac-sha2-256` | HMAC-SHA2 for SSH is described by RFC 6668. |

## Key Exchange Constants

| Wrapper constant | SSH algorithm name | Standards note |
| --- | --- | --- |
| `AMNetSshAlgorithms.KeyExchange.Curve25519Sha256` | `curve25519-sha256` | Curve25519 KEX for SSH is described by RFC 8731. |
| `AMNetSshAlgorithms.KeyExchange.Curve25519Sha256LibSsh` | `curve25519-sha256@libssh.org` | Historical OpenSSH/libssh extension name for Curve25519 KEX. |
| `AMNetSshAlgorithms.KeyExchange.EcdhNistp521` | `ecdh-sha2-nistp521` | ECDH for SSH is described by RFC 5656. |
| `AMNetSshAlgorithms.KeyExchange.EcdhNistp384` | `ecdh-sha2-nistp384` | ECDH for SSH is described by RFC 5656. |
| `AMNetSshAlgorithms.KeyExchange.EcdhNistp256` | `ecdh-sha2-nistp256` | ECDH for SSH is described by RFC 5656. |
| `AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup18Sha512` | `diffie-hellman-group18-sha512` | MODP DH groups for SSH are described by RFC 8268. |
| `AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup16Sha512` | `diffie-hellman-group16-sha512` | MODP DH groups for SSH are described by RFC 8268. |
| `AMNetSshAlgorithms.KeyExchange.DiffieHellmanGroup14Sha256` | `diffie-hellman-group14-sha256` | MODP DH groups for SSH are described by RFC 8268. |
| `AMNetSshAlgorithms.KeyExchange.Sntrup761x25519Sha512` | `sntrup761x25519-sha512@openssh.com` | Post-quantum hybrid Streamlined NTRU Prime 761 + X25519 ECDH. Available when the IKVM-transpiled BouncyCastle JCA provider is registered on the boot classpath (see `IkvmInitializer.cs`). |

## Host Key and Signature Constants

| Wrapper constant | SSH algorithm name | Standards note |
| --- | --- | --- |
| `AMNetSshAlgorithms.HostKeys.Ed25519` | `ssh-ed25519` | Ed25519 for SSH is described by RFC 8709. |
| `AMNetSshAlgorithms.HostKeys.EcdsaNistp521` | `ecdsa-sha2-nistp521` | ECDSA for SSH is described by RFC 5656. |
| `AMNetSshAlgorithms.HostKeys.EcdsaNistp384` | `ecdsa-sha2-nistp384` | ECDSA for SSH is described by RFC 5656. |
| `AMNetSshAlgorithms.HostKeys.EcdsaNistp256` | `ecdsa-sha2-nistp256` | ECDSA for SSH is described by RFC 5656. |
| `AMNetSshAlgorithms.HostKeys.RsaSha512` | `rsa-sha2-512` | RSA SHA-2 signatures for SSH are described by RFC 8332. |
| `AMNetSshAlgorithms.HostKeys.RsaSha256` | `rsa-sha2-256` | RSA SHA-2 signatures for SSH are described by RFC 8332. |
| `AMNetSshAlgorithms.HostKeys.SshRsa` | `ssh-rsa` | Legacy RSA SHA-1 signature name from RFC 4253; prefer RSA SHA-2 where clients support it. |

## Host Key Generation Constants

| Wrapper constant | Key type | Standards note |
| --- | --- | --- |
| `AMNetSshAlgorithms.HostKeyAlgorithms.Rsa` | `RSA` | Use with RSA SHA-2 host key signatures where possible; see RFC 8332. |
| `AMNetSshAlgorithms.HostKeyAlgorithms.Dsa` | `DSA` | Legacy SSH DSA generation option; avoid for new deployments. |
| `AMNetSshAlgorithms.HostKeyAlgorithms.Ecdsa` | `EC` | Use for ECDSA host keys; see RFC 5656. |
| `AMNetSshAlgorithms.HostKeyAlgorithms.Ed25519` | `EdDSA` | Use for Ed25519 host keys when supported by the runtime; see RFC 8709. |

## Default Policy Notes

The wrapper's modern presets prefer authenticated encryption and modern key
exchange methods when available. Developers building production systems should
still define an explicit security policy for their application, test that policy
against their supported clients, and periodically review algorithm choices
against current SSH recommendations.

---

*ApacheMinaSSHD.NET is maintained by **SERALYNX LLC** — building secure file transfer solutions for critical infrastructure since 2015. For a turnkey portable SFTP server for Windows and Linux with GUI management, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
