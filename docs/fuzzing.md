# Fuzzing

ApacheMinaSSHD.NET performs fuzz testing of the SSH protocol layer through its integration test suite. The integration tests use SSH.NET and Windows OpenSSH clients to exercise the SSH/SFTP/SCP protocol surface with various message sequences, connection patterns, and payload sizes.

## Approach

The project's fuzzing strategy is based on:

1. **Integration test harness** — `Sample/SimpleSSHDServer` runs a live server instance and connects with real SSH clients. The harness varies connection parameters, authentication flows, file transfer operations, and concurrent sessions.
2. **Client diversity** — Tests are executed against both SSH.NET (managed .NET client) and Windows OpenSSH (native client), providing independent protocol implementations that stress the server.
3. **Stress testing** — Concurrent multi-session tests with parallel file operations expose race conditions and resource management issues.

## Running Fuzz Tests

```powershell
# Run with stress test profile
dotnet run -c Release --project Sample/SimpleSSHDServer -- --integration-tests
```

## Future Work

As the project matures, we plan to integrate:

- **SharpFuzz** — .NET fuzzing library for targeted API-level fuzz testing of the wrapper layer
- **OSS-Fuzz** — Google's continuous fuzzing service for open source software
