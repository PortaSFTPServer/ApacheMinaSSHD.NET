// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

var mode = args.Length > 0 ? args[0].ToLower() : "password";
var server = AMNetSshServer.SetUpDefaultServer();
server.Host = "127.0.0.1";
server.Port = 2222;
server.Config.ApplyProductionDefaults();
server.Config.ApplyModernAlgorithmDefaults();
server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(@"C:\sftp-root"));
server.setSubsystemFactories(new AMNetSftpSubsystemFactory());
server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider("hostkey.ser"));

switch (mode)
{
    case "password":
        DemoSinglePassword(server);
        break;
    case "delegate-password":
        DemoDelegatePassword(server);
        break;
    case "composite-password":
        DemoCompositePassword(server);
        break;
    case "publickey":
        DemoPublicKey(server);
        break;
    case "fingerprint":
        DemoFingerprint(server);
        break;
    case "authorized-keys":
        DemoAuthorizedKeys(server);
        break;
    case "keyboard-interactive":
        DemoKeyboardInteractive(server);
        break;
    case "mfa":
        DemoMfa(server);
        break;
    default:
        Console.WriteLine($"Unknown mode: {mode}");
        Console.WriteLine("Usage: AuthenticationServer [mode]");
        Console.WriteLine("Modes: password, delegate-password, composite-password, publickey, fingerprint,");
        Console.WriteLine("       authorized-keys, keyboard-interactive, mfa");
        return;
}

server.Start();
Console.WriteLine($"AuthenticationServer ({mode}) running on port 2222. Press Enter to stop.");
Console.ReadLine();
server.Stop();

static void DemoSinglePassword(AMNetSshServer server)
{
    server.SetFixedPasswordAuthenticator("admin", "s3cret!");
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.Password);
    Console.WriteLine("Mode: single fixed password (admin:s3cret!)");
}

static void DemoDelegatePassword(AMNetSshServer server)
{
    server.SetDelegatePasswordAuthenticator((username, password, session) =>
    {
        Console.WriteLine($"Login attempt: {username} from {session.RemoteAddress}");
        return username == "alice" && password == "secure1"
            || username == "bob" && password == "secure2";
    });
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.Password);
    Console.WriteLine("Mode: delegate password (alice:secure1, bob:secure2)");
}

static void DemoCompositePassword(AMNetSshServer server)
{
    server.SetCompositePasswordAuthenticator(
        new AMNetFixedPasswordAuthenticator("admin", "master"),
        new AMNetDelegatePasswordAuthenticator((username, password, session) =>
            username == "backup" && password == "override"));
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.Password);
    Console.WriteLine("Mode: composite password (tries admin:master, then backup:override)");
}

static void DemoPublicKey(AMNetSshServer server)
{
    server.setPublickeyAuthenticator(new AMNetPublickeyAuthenticator("Authorized_Keys"));
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
    Console.WriteLine("Mode: directory-backed public key (Authorized_Keys folder)");
}

static void DemoFingerprint(AMNetSshServer server)
{
    server.SetFingerprintPublicKeyAuthenticator("alice",
        "SHA256:abc123...", "SHA256:def456...");
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
    Console.WriteLine("Mode: fingerprint-based public key (alice with 2 fingerprints)");
}

static void DemoAuthorizedKeys(AMNetSshServer server)
{
    server.SetAuthorizedKeysAuthenticator("authorized_keys");
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
    Console.WriteLine("Mode: OpenSSH-style authorized_keys file");
}

static void DemoKeyboardInteractive(AMNetSshServer server)
{
    server.SetFixedKeyboardInteractiveAuthenticator("123456",
        prompt: "TOTP Code", instruction: "Enter the code from your authenticator app.");
    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.KeyboardInteractive);
    Console.WriteLine("Mode: keyboard-interactive (expects '123456')");
}

static void DemoMfa(AMNetSshServer server)
{
    server.SetFixedPasswordAuthenticator("admin", "s3cret!");
    server.SetFixedKeyboardInteractiveAuthenticator("987654",
        prompt: "2FA Code", instruction: "Enter your two-factor code.");
    server.SetAuthenticationMethods(
        AMNetSshAuthenticationMethods.RequireAll(
            AMNetSshAuthenticationMethods.Password,
            AMNetSshAuthenticationMethods.KeyboardInteractive));
    Console.WriteLine("Mode: MFA (require password AND keyboard-interactive)");
}
