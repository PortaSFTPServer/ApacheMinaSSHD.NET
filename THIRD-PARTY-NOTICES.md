# Third-Party Notices

ApacheMinaSSHD.NET is created by [SERALYNX LLC](https://portasftpserver.com) —
the team behind **[Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/)**,
a portable SFTP server for Windows and Linux serving critical infrastructure. Project code
is licensed under the MIT License. See `LICENSE`.

This file summarizes third-party software used by the source tree and NuGet
packages. It is provided to make package review easier for downstream
developers; it is not legal advice.

## Redistributed in ApacheMinaSSHD.NET.Bindings

The Bindings package includes IKVM-generated .NET assemblies for the Maven
artifacts below. These generated assemblies are included so application
developers can consume the wrapper without importing Java or Apache MINA types.

| Component | Version | Package contents | License |
| --- | --- | --- | --- |
| Apache MINA SSHD `sshd-common`, `sshd-core`, `sshd-scp`, `sshd-sftp` | 2.17.1 | `org.apache.sshd.common.dll`, `org.apache.sshd.core.dll`, `org.apache.sshd.scp.dll`, `org.apache.sshd.sftp.dll` | Apache License 2.0 |
| SLF4J API and Simple Binding | 1.7.36 | `org.slf4j.dll`, `org.slf4j.simple.dll` | MIT License |
| JCL-over-SLF4J bridge | 1.7.36 | `org.apache.commons.logging.dll` | Apache License 2.0 according to the artifact POM |

Apache License 2.0 text is included in `licenses/APACHE-2.0.txt`. SLF4J MIT
License text is included in `licenses/SLF4J-MIT.txt`. Apache MINA SSHD notice
text is preserved in `NOTICE`.

## NuGet Dependencies

These packages are referenced as NuGet dependencies rather than embedded as
project-owned binaries. Their NuGet packages carry their own license metadata
and license files.

| Component | Version | Used by | License note |
| --- | --- | --- | --- |
| IKVM | 8.15.0 | Wrapper and Bindings runtime support | IKVM's package license file includes IKVM's permissive license and notes that parts of IKVM.Java/OpenJDK/GNU Classpath are GPLv2 with the Classpath exception. |
| IKVM.Maven.Sdk | 1.11.0 | Build-time Maven reference support | MIT License; referenced as a private build asset. |
| Portable.BouncyCastle | 1.9.0 | Wrapper cryptography helpers | See the package's declared Bouncy Castle C# license URL. |

## Maintainer Checklist

- Update this file when adding, removing, or changing Maven or NuGet
  dependencies.
- Preserve upstream license texts and notice text when redistributing generated
  or compiled third-party binaries.
- Keep `PackageLicenseExpression` set to `MIT` only for the ApacheMinaSSHD.NET
  project code. Third-party licenses are documented here and shipped as package
  content.
- Re-run package verification after changing dependency versions so the notice
  list matches the actual `.nupkg` contents.
