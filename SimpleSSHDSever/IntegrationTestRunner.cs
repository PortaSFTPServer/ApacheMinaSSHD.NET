using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleSSHDSever
{
    internal static partial class IntegrationTestRunner
    {
        private const string Username = "integration";
        private const string Password = "Password-12345!";
        private const string Host = "127.0.0.1";
        private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(45);

        public static async Task<int> RunAsync(string[] args)
        {
            AMNetOutputStream javaErrorOutput = RedirectJavaErrorsForIntegrationTests();

            string runRoot = Path.Combine(
                Path.GetTempPath(),
                "ApacheMinaSSHD.NET.OpenSshTests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(runRoot);
            Console.WriteLine($"Integration test root: {runRoot}");

            var results = new List<TestResult>();
            try
            {
                OpenSshTools tools = OpenSshTools.Resolve();
                ClientKeyMaterial key = await GenerateClientKeyAsync(tools, runRoot);

                results.Add(await RunTestAsync(
                    "OpenSSH SFTP/SCP public-key transfers and filesystem policy",
                    () => RunPublicKeyTransferAndPolicyTestsAsync(tools, key, runRoot)));

                results.Add(await RunTestAsync(
                    "OpenSSH password authentication",
                    () => RunPasswordAuthenticationTestAsync(tools, runRoot)));

                results.Add(await RunTestAsync(
                    "OpenSSH authorized_keys authentication",
                    () => RunAuthorizedKeysAuthenticationTestAsync(tools, key, runRoot)));

                results.Add(await RunTestAsync(
                    "OpenSSH SFTP concurrency soak",
                    () => RunConcurrencySoakTestAsync(tools, key, runRoot)));

                results.Add(await RunTestAsync(
                    "WinSCP .NET assembly SFTP stress",
                    () => RunWinScpAssemblyStressTestAsync(runRoot)));
            }
            finally
            {
                TryDeleteDirectory(runRoot);
            }

            Console.WriteLine();
            Console.WriteLine("Integration test summary:");
            foreach (TestResult result in results)
            {
                Console.WriteLine($"  [{(result.Passed ? "PASS" : "FAIL")}] {result.Name}");
                if (!string.IsNullOrWhiteSpace(result.Details))
                {
                    Console.WriteLine($"        {result.Details}");
                }
            }

            GC.KeepAlive(javaErrorOutput);
            return results.All(result => result.Passed) ? 0 : 1;
        }

        private static AMNetOutputStream RedirectJavaErrorsForIntegrationTests()
        {
            java.lang.System.setProperty("org.slf4j.simpleLogger.defaultLogLevel", "error");

            bool suppressShutdownTrace = false;
            var outputStream = new AMNetOutputStream(line =>
            {
                string cleanLine = line.Trim();

                if (cleanLine.StartsWith("Exception in thread", StringComparison.Ordinal))
                {
                    suppressShutdownTrace = true;
                    return;
                }

                if (cleanLine.Contains("java.lang.IllegalStateException: Executor has been shut down", StringComparison.Ordinal))
                {
                    suppressShutdownTrace = true;
                    return;
                }

                if (suppressShutdownTrace)
                {
                    if (cleanLine.StartsWith("at ", StringComparison.Ordinal)
                        || cleanLine.Contains("org.apache.sshd", StringComparison.Ordinal)
                        || cleanLine.Contains("sun.nio", StringComparison.Ordinal)
                        || cleanLine.Contains("java.lang.Thread", StringComparison.Ordinal))
                    {
                        return;
                    }

                    suppressShutdownTrace = false;
                }

                if (IsExpectedNegativePathLog(cleanLine))
                {
                    return;
                }

                Console.Error.WriteLine(line);
            });

            outputStream.RedirectStandardError();
            return outputStream;
        }

        private static bool IsExpectedNegativePathLog(string line)
        {
            return line.Contains("SCP path is not allowed", StringComparison.Ordinal)
                || line.Contains("scp -f ../outside.txt", StringComparison.Ordinal)
                || line.Contains("An existing connection was forcibly closed by the remote host", StringComparison.Ordinal);
        }

        private static async Task RunPublicKeyTransferAndPolicyTestsAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            string runRoot)
        {
            using TestServer server = StartServer(runRoot, AuthMode.PublicKey, key);
            SeedPolicyFiles(server);

            string localUpload = Path.Combine(server.ClientRoot, "sftp-upload.txt");
            string sftpDownload = Path.Combine(server.ClientRoot, "sftp-download.txt");
            File.WriteAllText(localUpload, "sftp payload");

            ProcessResult sftpTransfer = await RunSftpWithKeyAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                $"put {SftpLocalPath(localUpload)} upload.txt",
                $"get upload.txt {SftpLocalPath(sftpDownload)}");

            sftpTransfer.EnsureSuccess("SFTP upload/download failed.");
            AssertFileText(sftpDownload, "sftp payload", "SFTP downloaded content did not match.");

            ProcessResult sftpList = await RunSftpWithKeyAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                "ls -la");

            sftpList.EnsureSuccess("SFTP directory listing failed.");
            AssertContains(sftpList.StdOut, "visible.txt", "SFTP listing did not include visible.txt.");
            AssertDoesNotContain(sftpList.StdOut, "secret_data.txt", "SFTP listing exposed secret_data.txt.");
            AssertDoesNotContain(sftpList.StdOut, ".hidden", "SFTP listing exposed .hidden.");

            await ExpectSftpFailureAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                "SFTP hidden file download should fail.",
                $"get secret_data.txt {SftpLocalPath(Path.Combine(server.ClientRoot, "secret-download.txt"))}");

            await ExpectSftpFailureAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                "SFTP traversal download should fail.",
                $"get ../outside.txt {SftpLocalPath(Path.Combine(server.ClientRoot, "outside-download.txt"))}");

            await RunSymlinkBoundaryTestAsync(tools, key, server);

            string scpUpload = Path.Combine(server.ClientRoot, "scp-upload.txt");
            string scpDownload = Path.Combine(server.ClientRoot, "scp-download.txt");
            File.WriteAllText(scpUpload, "scp payload");

            ProcessResult scpPut = await RunScpWithKeyAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                scpUpload,
                $"{Username}@{Host}:scp-upload.txt");

            scpPut.EnsureSuccess("SCP upload failed.");

            ProcessResult scpGet = await RunScpWithKeyAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                $"{Username}@{Host}:scp-upload.txt",
                scpDownload);

            scpGet.EnsureSuccess("SCP download failed.");
            AssertFileText(scpDownload, "scp payload", "SCP downloaded content did not match.");

            await ExpectScpFailureAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                $"{Username}@{Host}:secret_data.txt",
                Path.Combine(server.ClientRoot, "scp-secret-download.txt"),
                "SCP hidden file download should fail.");

            await ExpectScpFailureAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                $"{Username}@{Host}:../outside.txt",
                Path.Combine(server.ClientRoot, "scp-outside-download.txt"),
                "SCP traversal download should fail.");
        }

        private static async Task RunPasswordAuthenticationTestAsync(OpenSshTools tools, string runRoot)
        {
            using TestServer server = StartServer(runRoot, AuthMode.Password, key: null);

            ProcessResult result = await RunSftpWithPasswordAsync(
                tools,
                server.Port,
                server.ClientRoot,
                "pwd");

            result.EnsureSuccess("Password authentication through OpenSSH SFTP failed.");
        }

        private static async Task RunAuthorizedKeysAuthenticationTestAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            string runRoot)
        {
            using TestServer server = StartServer(runRoot, AuthMode.AuthorizedKeys, key);

            string localUpload = Path.Combine(server.ClientRoot, "authorized-upload.txt");
            string download = Path.Combine(server.ClientRoot, "authorized-download.txt");
            File.WriteAllText(localUpload, "authorized_keys payload");

            ProcessResult result = await RunSftpWithKeyAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                $"put {SftpLocalPath(localUpload)} authorized-upload.txt",
                $"get authorized-upload.txt {SftpLocalPath(download)}");

            result.EnsureSuccess("authorized_keys SFTP flow failed.");
            AssertFileText(download, "authorized_keys payload", "authorized_keys downloaded content did not match.");
        }

        private static async Task RunConcurrencySoakTestAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            string runRoot)
        {
            using TestServer server = StartServer(runRoot, AuthMode.PublicKey, key);

            const int workerCount = 6;
            Task[] workers = Enumerable.Range(0, workerCount)
                .Select(async index =>
                {
                    string local = Path.Combine(server.ClientRoot, $"soak-{index}.txt");
                    string download = Path.Combine(server.ClientRoot, $"soak-{index}-download.txt");
                    string payload = $"soak payload {index}";
                    File.WriteAllText(local, payload);

                    ProcessResult result = await RunSftpWithKeyAsync(
                        tools,
                        key,
                        server.Port,
                        server.ClientRoot,
                        $"put {SftpLocalPath(local)} soak-{index}.txt",
                        $"get soak-{index}.txt {SftpLocalPath(download)}");

                    result.EnsureSuccess($"Concurrent SFTP worker {index} failed.");
                    AssertFileText(download, payload, $"Concurrent SFTP worker {index} content mismatch.");
                })
                .ToArray();

            await Task.WhenAll(workers);
        }

        private static async Task RunSymlinkBoundaryTestAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            TestServer server)
        {
            string outside = Path.Combine(server.BaseRoot, "outside-symlink-target.txt");
            string link = Path.Combine(server.UserHome, "outside-link.txt");
            string download = Path.Combine(server.ClientRoot, "symlink-download.txt");

            File.WriteAllText(outside, "outside symlink target");

            try
            {
                File.CreateSymbolicLink(link, outside);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
            {
                Console.WriteLine($"  [SKIP] Symlink boundary check skipped: {ex.Message}");
                return;
            }

            if (!CanDetectSymlink(link))
            {
                Console.WriteLine("  [SKIP] Symlink boundary check skipped: symlink undetectable on this platform.");
                return;
            }

            await ExpectSftpFailureAsync(
                tools,
                key,
                server.Port,
                server.ClientRoot,
                "SFTP symlink escape should fail.",
                $"get outside-link.txt {SftpLocalPath(download)}");
        }

        private static bool CanDetectSymlink(string path)
        {
            try
            {
                return File.ResolveLinkTarget(path, true) != null;
            }
            catch
            {
                return false;
            }
        }

        private static TestServer StartServer(string runRoot, AuthMode authMode, ClientKeyMaterial? key)
        {
            string serverRoot = Path.Combine(runRoot, $"server-{Guid.NewGuid():N}");
            string baseRoot = Path.Combine(serverRoot, "data");
            string userHome = Path.Combine(baseRoot, Username);
            string clientRoot = Path.Combine(serverRoot, "client");
            string hostKeyPath = Path.Combine(serverRoot, "hostkey.ser");

            Directory.CreateDirectory(userHome);
            Directory.CreateDirectory(clientRoot);

            int port = GetFreeTcpPort();
            AMNetSshServer server = AMNetSshServer.SetUpDefaultServer();
            server.setHost(Host);
            server.setPort(port);
            server.Config.ApplyProductionDefaults();
            server.Config.ApplyModernAlgorithmDefaults();

            var hostKeyProvider = new AMNetSimpleGeneratorHostKeyProvider(hostKeyPath);
            hostKeyProvider.setAlgorithm("RSA");
            hostKeyProvider.setKeySize(2048);
            hostKeyProvider.setStrictFilePermissions(false);
            server.setKeyPairProvider(hostKeyProvider);

            server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(baseRoot));

            var sftpFactory = new AMNetSftpSubsystemFactory();
            sftpFactory.setFileSystemAccessor(new AMNetSftpFileSystemAccessor());
            server.setSubsystemFactories(sftpFactory);

            var scpFactory = new AMNetScpCommandFactory(new AMNetScpFileOpener(userHome));
            server.setCommandFactory(scpFactory);

            switch (authMode)
            {
                case AuthMode.Password:
                    server.SetFixedPasswordAuthenticator(Username, Password);
                    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.Password);
                    break;
                case AuthMode.PublicKey:
                    ClientKeyMaterial publicKey = RequireKey(key);
                    server.SetFingerprintPublicKeyAuthenticator(Username, publicKey.Fingerprint);
                    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
                    break;
                case AuthMode.AuthorizedKeys:
                    ClientKeyMaterial authorizedKey = RequireKey(key);
                    string authorizedKeysFile = Path.Combine(serverRoot, "authorized_keys");
                    File.Copy(authorizedKey.PublicKeyPath, authorizedKeysFile, overwrite: true);
                    server.SetAuthorizedKeysAuthenticator(authorizedKeysFile);
                    server.SetAuthenticationMethods(AMNetSshAuthenticationMethods.PublicKey);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported auth mode: {authMode}");
            }

            server.start();
            return new TestServer(server, port, serverRoot, baseRoot, userHome, clientRoot);
        }

        private static void SeedPolicyFiles(TestServer server)
        {
            File.WriteAllText(Path.Combine(server.UserHome, "visible.txt"), "visible");
            File.WriteAllText(Path.Combine(server.UserHome, "secret_data.txt"), "secret");
            File.WriteAllText(Path.Combine(server.UserHome, ".hidden"), "hidden");
            File.WriteAllText(Path.Combine(server.BaseRoot, "outside.txt"), "outside");

            string hiddenByAttribute = Path.Combine(server.UserHome, "windows-hidden.txt");
            File.WriteAllText(hiddenByAttribute, "hidden attribute");
            try
            {
                File.SetAttributes(hiddenByAttribute, File.GetAttributes(hiddenByAttribute) | FileAttributes.Hidden);
            }
            catch
            {
                // The dotfile and secret_data checks still cover cross-platform hidden filtering.
            }
        }

        private static async Task<ClientKeyMaterial> GenerateClientKeyAsync(OpenSshTools tools, string runRoot)
        {
            string keyRoot = Path.Combine(runRoot, "keys");
            Directory.CreateDirectory(keyRoot);

            string privateKey = Path.Combine(keyRoot, "id_rsa");
            string publicKey = Path.Combine(keyRoot, "client-authorized-key.pub");

            ProcessResult keygen = await RunProcessAsync(
                tools.SshKeygen,
                keyRoot,
                arguments:
                [
                    "-q",
                    "-t", "rsa",
                    "-b", "2048",
                    "-N", string.Empty,
                    "-f", privateKey
                ]);

            if (keygen.ExitCode != 0 && !File.Exists(privateKey))
            {
                keygen.EnsureSuccess("ssh-keygen failed to create a client key.");
            }

            ProcessResult publicKeyResult = await RunProcessAsync(
                tools.SshKeygen,
                keyRoot,
                arguments:
                [
                    "-y",
                    "-f", privateKey
                ]);

            publicKeyResult.EnsureSuccess("ssh-keygen failed to derive the client public key.");
            File.WriteAllText(publicKey, publicKeyResult.StdOut.Trim() + Environment.NewLine);

            ProcessResult fingerprint = await RunProcessAsync(
                tools.SshKeygen,
                keyRoot,
                arguments:
                [
                    "-lf", publicKey,
                    "-E", "sha256"
                ]);

            fingerprint.EnsureSuccess("ssh-keygen failed to compute the client key fingerprint.");

            string[] parts = fingerprint.StdOut.Split(
                [' ', '\r', '\n', '\t'],
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2 || !parts[1].StartsWith("SHA256:", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Unable to parse SSH key fingerprint: {fingerprint.StdOut}");
            }

            return new ClientKeyMaterial(privateKey, publicKey, parts[1]);
        }

        private static async Task<ProcessResult> RunSftpWithKeyAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            int port,
            string workingDirectory,
            params string[] batchCommands)
        {
            string batchFile = WriteBatchFile(workingDirectory, batchCommands);
            return await RunProcessAsync(
                tools.Sftp,
                workingDirectory,
                arguments:
                [
                    "-i", key.PrivateKeyPath,
                    "-o", "IdentitiesOnly=yes",
                    "-o", "PreferredAuthentications=publickey",
                    "-o", "PasswordAuthentication=no",
                    "-o", "BatchMode=yes",
                    "-o", "StrictHostKeyChecking=no",
                    "-o", $"UserKnownHostsFile={KnownHostsNullFile()}",
                    "-P", port.ToString(),
                    "-b", batchFile,
                    $"{Username}@{Host}"
                ]);
        }

        private static async Task<ProcessResult> RunSftpWithPasswordAsync(
            OpenSshTools tools,
            int port,
            string workingDirectory,
            params string[] batchCommands)
        {
            string batchFile = WriteBatchFile(workingDirectory, batchCommands);
            string askPass = WriteAskPassScript(workingDirectory);

            var environment = new Dictionary<string, string>
            {
                ["SSH_ASKPASS"] = askPass,
                ["SSH_ASKPASS_REQUIRE"] = "force",
                ["DISPLAY"] = "localhost:0"
            };

            return await RunProcessAsync(
                tools.Sftp,
                workingDirectory,
                environment,
                arguments:
                [
                    "-o", "PreferredAuthentications=password",
                    "-o", "PubkeyAuthentication=no",
                    "-o", "BatchMode=no",
                    "-o", "NumberOfPasswordPrompts=1",
                    "-o", "StrictHostKeyChecking=no",
                    "-o", $"UserKnownHostsFile={KnownHostsNullFile()}",
                    "-P", port.ToString(),
                    "-b", batchFile,
                    $"{Username}@{Host}"
                ]);
        }

        private static async Task<ProcessResult> RunScpWithKeyAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            int port,
            string workingDirectory,
            string source,
            string destination)
        {
            return await RunProcessAsync(
                tools.Scp,
                workingDirectory,
                arguments:
                [
                    "-O",
                    "-i", key.PrivateKeyPath,
                    "-o", "IdentitiesOnly=yes",
                    "-o", "PreferredAuthentications=publickey",
                    "-o", "PasswordAuthentication=no",
                    "-o", "BatchMode=yes",
                    "-o", "StrictHostKeyChecking=no",
                    "-o", $"UserKnownHostsFile={KnownHostsNullFile()}",
                    "-P", port.ToString(),
                    source,
                    destination
                ]);
        }

        private static async Task ExpectSftpFailureAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            int port,
            string workingDirectory,
            string failureMessage,
            params string[] batchCommands)
        {
            ProcessResult result = await RunSftpWithKeyAsync(tools, key, port, workingDirectory, batchCommands);

            bool hasError =
                result.ExitCode != 0 ||
                result.StdErr.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                result.StdErr.Contains("no such file", StringComparison.OrdinalIgnoreCase) ||
                result.StdErr.Contains("permission denied", StringComparison.OrdinalIgnoreCase) ||
                result.StdErr.Contains("error", StringComparison.OrdinalIgnoreCase);

            if (!hasError)
            {
                throw new InvalidOperationException(
                    $"{failureMessage} Command unexpectedly succeeded. " +
                    $"exitCode={result.ExitCode}; stdout={result.StdOut.Trim()}; stderr={result.StdErr.Trim()}");
            }

            if (result.ExitCode == 0)
            {
                Console.WriteLine("  [DIAG] sftp -b returned 0 but stderr indicates failure; accepting as expected denial.");
            }
        }

        private static async Task ExpectScpFailureAsync(
            OpenSshTools tools,
            ClientKeyMaterial key,
            int port,
            string workingDirectory,
            string source,
            string destination,
            string failureMessage)
        {
            ProcessResult result = await RunScpWithKeyAsync(
                tools,
                key,
                port,
                workingDirectory,
                source,
                destination);

            if (result.ExitCode == 0)
            {
                throw new InvalidOperationException($"{failureMessage} Command unexpectedly succeeded.");
            }
        }

        private static async Task<TestResult> RunTestAsync(string name, Func<Task> test)
        {
            Console.WriteLine($"[RUN ] {name}");
            try
            {
                await test();
                Console.WriteLine($"[PASS] {name}");
                return new TestResult(name, Passed: true, Details: null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {name}: {ex.Message}");
                return new TestResult(name, Passed: false, Details: ex.Message);
            }
        }

        private static async Task<ProcessResult> RunProcessAsync(
            string fileName,
            string workingDirectory,
            IReadOnlyDictionary<string, string>? environment = null,
            params string[] arguments)
        {
            var startInfo = new ProcessStartInfo(fileName)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            if (environment != null)
            {
                foreach (var pair in environment)
                {
                    startInfo.Environment[pair.Key] = pair.Value;
                }
            }

            using var process = new Process { StartInfo = startInfo };
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stdout.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stderr.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var timeout = new CancellationTokenSource(ProcessTimeout);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw new TimeoutException(
                    $"Process timed out: {fileName} {string.Join(" ", arguments)}");
            }

            return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString());
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static void AssertFileText(string path, string expected, string message)
        {
            if (!File.Exists(path) || File.ReadAllText(path) != expected)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertContains(string text, string expected, string message)
        {
            if (!text.Contains(expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertDoesNotContain(string text, string unexpected, string message)
        {
            if (text.Contains(unexpected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string WriteBatchFile(string workingDirectory, params string[] commands)
        {
            string path = Path.Combine(workingDirectory, $"sftp-batch-{Guid.NewGuid():N}.txt");
            File.WriteAllLines(path, commands);
            return path;
        }

        private static string WriteAskPassScript(string workingDirectory)
        {
            string path = Path.Combine(workingDirectory, "askpass.cmd");
            File.WriteAllText(path, $"@echo off{Environment.NewLine}echo {Password}{Environment.NewLine}");
            return path;
        }

        private static string SftpLocalPath(string path)
        {
            return $"\"{path.Replace('\\', '/')}\"";
        }

        private static string KnownHostsNullFile()
        {
            return OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
        }

        private static ClientKeyMaterial RequireKey(ClientKeyMaterial? key)
        {
            return key ?? throw new ArgumentNullException(nameof(key));
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
            }
            catch
            {
            }
        }

        private sealed record TestResult(string Name, bool Passed, string? Details);

        private sealed record ClientKeyMaterial(
            string PrivateKeyPath,
            string PublicKeyPath,
            string Fingerprint);

        private sealed record ProcessResult(int ExitCode, string StdOut, string StdErr)
        {
            public void EnsureSuccess(string message)
            {
                if (ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"{message} ExitCode={ExitCode}; stdout={StdOut.Trim()}; stderr={StdErr.Trim()}");
                }
            }
        }

        private sealed class TestServer : IDisposable
        {
            public TestServer(
                AMNetSshServer server,
                int port,
                string serverRoot,
                string baseRoot,
                string userHome,
                string clientRoot)
            {
                Server = server;
                Port = port;
                ServerRoot = serverRoot;
                BaseRoot = baseRoot;
                UserHome = userHome;
                ClientRoot = clientRoot;
            }

            public AMNetSshServer Server { get; }
            public int Port { get; }
            public string ServerRoot { get; }
            public string BaseRoot { get; }
            public string UserHome { get; }
            public string ClientRoot { get; }

            public void Dispose()
            {
                try
                {
                    Server.stop(immediately: false);
                }
                catch
                {
                }
            }
        }

        private sealed class OpenSshTools
        {
            private OpenSshTools(string ssh, string scp, string sftp, string sshKeygen)
            {
                Ssh = ssh;
                Scp = scp;
                Sftp = sftp;
                SshKeygen = sshKeygen;
            }

            public string Ssh { get; }
            public string Scp { get; }
            public string Sftp { get; }
            public string SshKeygen { get; }

            public static OpenSshTools Resolve()
            {
                string windowsOpenSsh = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "System32",
                    "OpenSSH");

                return new OpenSshTools(
                    ResolveTool("ssh", Path.Combine(windowsOpenSsh, "ssh.exe")),
                    ResolveTool("scp", Path.Combine(windowsOpenSsh, "scp.exe")),
                    ResolveTool("sftp", Path.Combine(windowsOpenSsh, "sftp.exe")),
                    ResolveTool("ssh-keygen", Path.Combine(windowsOpenSsh, "ssh-keygen.exe")));
            }

            private static string ResolveTool(string commandName, string preferredPath)
            {
                if (File.Exists(preferredPath))
                {
                    return preferredPath;
                }

                return commandName;
            }
        }

        private enum AuthMode
        {
            Password,
            PublicKey,
            AuthorizedKeys
        }

    }
}
