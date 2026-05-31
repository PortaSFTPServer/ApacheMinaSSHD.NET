using WinSCP;

namespace SimpleSSHDSever
{
    internal static partial class IntegrationTestRunner
    {
        private const int DefaultWinScpStressWorkers = 6;
        private const int DefaultWinScpStressIterations = 10;

        private static async Task RunWinScpAssemblyStressTestAsync(string runRoot)
        {
            using TestServer server = StartServer(runRoot, AuthMode.Password, key: null);
            SeedPolicyFiles(server);

            string winscpExecutablePath = ResolveWinScpExecutablePath();

            ValidateWinScpPolicyBehavior(server, winscpExecutablePath);

            int workers = GetPositiveEnvironmentInt(
                "WINSCP_STRESS_WORKERS",
                DefaultWinScpStressWorkers);

            int iterations = GetPositiveEnvironmentInt(
                "WINSCP_STRESS_ITERATIONS",
                DefaultWinScpStressIterations);

            Task[] workerTasks = Enumerable.Range(0, workers)
                .Select(workerIndex => Task.Run(() =>
                    RunWinScpStressWorker(
                        server,
                        winscpExecutablePath,
                        workerIndex,
                        iterations)))
                .ToArray();

            await Task.WhenAll(workerTasks);
        }

        private static void ValidateWinScpPolicyBehavior(
            TestServer server,
            string winscpExecutablePath)
        {
            using var session = OpenWinScpSession(server, winscpExecutablePath);

            RemoteDirectoryInfo listing = session.ListDirectory("/");
            string[] names = listing.Files
                .Cast<RemoteFileInfo>()
                .Select(file => file.Name)
                .ToArray();

            if (!names.Contains("visible.txt", StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("WinSCP listing did not include visible.txt.");
            }

            if (names.Contains("secret_data.txt", StringComparer.OrdinalIgnoreCase) ||
                names.Contains(".hidden", StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("WinSCP listing exposed a hidden test file.");
            }

            ExpectWinScpFailure(
                () => session.GetFiles(
                    "/../outside.txt",
                    Path.Combine(server.ClientRoot, "winscp-outside-download.txt")).Check(),
                "WinSCP traversal download should fail.");
        }

        private static void RunWinScpStressWorker(
            TestServer server,
            string winscpExecutablePath,
            int workerIndex,
            int iterations)
        {
            string workerRoot = Path.Combine(server.ClientRoot, $"winscp-worker-{workerIndex}");
            Directory.CreateDirectory(workerRoot);

            using var session = OpenWinScpSession(server, winscpExecutablePath);
            TransferOptions transferOptions = CreateWinScpTransferOptions();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                string fileName = $"winscp-{workerIndex}-{iteration}.txt";
                string localUpload = Path.Combine(workerRoot, fileName);
                string localDownload = Path.Combine(workerRoot, $"download-{fileName}");
                string remotePath = "/" + fileName;
                string payload = CreateWinScpPayload(workerIndex, iteration);

                File.WriteAllText(localUpload, payload);

                if (session.FileExists(remotePath))
                {
                    session.RemoveFiles(remotePath).Check();
                }

                session.PutFiles(localUpload, remotePath, remove: false, transferOptions).Check();

                RemoteFileInfo remoteFile = session.GetFileInfo(remotePath);
                long localLength = new FileInfo(localUpload).Length;
                if (remoteFile.Length != localLength)
                {
                    throw new InvalidOperationException(
                        $"WinSCP remote size mismatch for {remotePath}. Expected {localLength}, got {remoteFile.Length}.");
                }

                session.GetFiles(remotePath, localDownload, remove: false, transferOptions).Check();
                AssertFileText(localDownload, payload, $"WinSCP downloaded content mismatch for {remotePath}.");

                session.RemoveFiles(remotePath).Check();
                if (session.FileExists(remotePath))
                {
                    throw new InvalidOperationException($"WinSCP failed to remove {remotePath}.");
                }
            }
        }

        private static WinSCP.Session OpenWinScpSession(
            TestServer server,
            string winscpExecutablePath)
        {
            var session = new WinSCP.Session
            {
                ExecutablePath = winscpExecutablePath,
                DisableVersionCheck = true,
                Timeout = TimeSpan.FromSeconds(45)
            };

            string? logDirectory = Environment.GetEnvironmentVariable("WINSCP_STRESS_LOG_DIR");
            if (!string.IsNullOrWhiteSpace(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                session.SessionLogPath = Path.Combine(
                    logDirectory,
                    $"winscp-{Guid.NewGuid():N}.log");
            }

            try
            {
                session.Open(new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = Host,
                    PortNumber = server.Port,
                    UserName = Username,
                    Password = Password,
                    Timeout = TimeSpan.FromSeconds(30),
                    SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny
                });

                return session;
            }
            catch
            {
                session.Dispose();
                throw;
            }
        }

        private static TransferOptions CreateWinScpTransferOptions()
        {
            var transferOptions = new TransferOptions
            {
                TransferMode = TransferMode.Binary,
                OverwriteMode = OverwriteMode.Overwrite,
                PreserveTimestamp = false
            };

            transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
            return transferOptions;
        }

        private static string ResolveWinScpExecutablePath()
        {
            string? configuredPath = Environment.GetEnvironmentVariable("WINSCP_EXE");
            if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
            {
                return configuredPath;
            }

            string[] outputCandidates =
            [
                Path.Combine(AppContext.BaseDirectory, "WinSCP.exe"),
                Path.Combine(AppContext.BaseDirectory, "winscp.exe")
            ];

            foreach (string candidate in outputCandidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            string packageRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget",
                "packages",
                "winscp");

            if (Directory.Exists(packageRoot))
            {
                string? packageExecutable = Directory
                    .EnumerateFiles(packageRoot, "WinSCP.exe", SearchOption.AllDirectories)
                    .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(packageExecutable))
                {
                    return packageExecutable;
                }
            }

            throw new FileNotFoundException(
                "WinSCP.exe was not found. Restore the WinSCP NuGet package or set WINSCP_EXE to the executable path.");
        }

        private static string CreateWinScpPayload(int workerIndex, int iteration)
        {
            string header = $"WinSCP stress payload worker={workerIndex}; iteration={iteration};";
            return header + Environment.NewLine + new string((char)('A' + workerIndex % 26), 8192);
        }

        private static int GetPositiveEnvironmentInt(string name, int defaultValue)
        {
            string? rawValue = Environment.GetEnvironmentVariable(name);
            return int.TryParse(rawValue, out int parsedValue) && parsedValue > 0
                ? parsedValue
                : defaultValue;
        }

        private static void ExpectWinScpFailure(Action action, string failureMessage)
        {
            try
            {
                action();
            }
            catch (SessionRemoteException)
            {
                return;
            }
            catch (SessionLocalException)
            {
                return;
            }

            throw new InvalidOperationException($"{failureMessage} Command unexpectedly succeeded.");
        }
    }
}
