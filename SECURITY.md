# Security Model

ApacheMinaSSHD.NET is a library wrapper around Apache MINA SSHD. It is not a
turnkey SFTP/SCP server product. The wrapper provides .NET-facing APIs,
extension points, and integration tests; the application developer is
responsible for the final security policy used in production.

## Library Responsibilities

- Hide Apache MINA and Java types from application-facing interfaces.
- Expose .NET hooks for authentication, authorization, filesystem access,
  event handling, logging, SFTP, and SCP behavior.
- Deny password and keyboard-interactive authentication by default unless the
  application configures an explicit authenticator.
- Provide conservative baseline configuration helpers for common SSH limits.
- Keep SFTP/SCP behavior covered by real OpenSSH integration tests.
- Add dependency and vulnerability scanning for NuGet and Maven dependencies.

## Application Responsibilities

- Implement authentication and authorization policy for the deployment.
- Manage host keys, user keys, passwords, key rotation, and secret storage.
- Define filesystem jail behavior, path traversal handling, symlink behavior,
  hidden-file policy, and per-user storage isolation.
- Decide allowed SSH algorithms, banners, session limits, rate limits, logging,
  audit retention, monitoring, backup, and incident-response behavior.
- Run the integration tests and security scan in the application's CI pipeline.

## Sample Project

`Sample/SimpleSSHDSever` is a sample and integration harness. It demonstrates how the
library can be wired to real OpenSSH clients and how policy hooks can be tested.
It should not be copied as a complete production server without replacing the
sample authentication, authorization, storage, logging, and deployment policy
with application-specific implementations.
