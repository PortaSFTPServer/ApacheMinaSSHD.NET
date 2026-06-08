# Production Deployment

This guide covers deploying ApacheMinaSSHD.NET in production environments — as a Windows Service, inside Docker, or as an embedded library.

## Windows Service Deployment

ApacheMinaSSHD.NET can run as a Windows Service for headless, always-on operation:

```csharp
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Microsoft.Extensions.Hosting;

class SftpWindowsService : BackgroundService
{
    private AMNetSshServer _server;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _server = AMNetSshServer.SetUpDefaultServer();
        _server.Host = "0.0.0.0";
        _server.Port = 22;

        // Configure authentication, filesystem, etc.
        ConfigureServer();

        _server.Start();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _server?.Stop();
        await base.StopAsync(cancellationToken);
    }
}
```

Install as a Windows Service:

```powershell
sc.exe create SftpService binPath="C:\app\MyServer.exe"
sc.exe start SftpService
```

## Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MyServer/MyServer.csproj", "MyServer/"]
RUN dotnet restore "MyServer/MyServer.csproj"
COPY . .
RUN dotnet publish "MyServer/MyServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 22
ENTRYPOINT ["dotnet", "MyServer.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  sftp-server:
    build: .
    ports:
      - "22:22"
    volumes:
      - sftp-data:/app/sftp-root
      - hostkeys:/app/hostkeys
    environment:
      - AMNET_SAMPLE_PASSWORD=${SFTP_PASSWORD}
    restart: unless-stopped

volumes:
  sftp-data:
  hostkeys:
```

## High Availability with PROXY Protocol

When fronting with a load balancer (HAProxy, Nginx, AWS NLB), enable PROXY protocol so the real client IP is forwarded:

```csharp
server.setServerProxyAcceptor(new AMNetServerProxyAcceptor());
```

HAProxy configuration:

```
frontend sftp
    bind *:22
    mode tcp
    default_backend sftp_servers

backend sftp_servers
    mode tcp
    server sftp1 10.0.1.10:22 send-proxy-v2
    server sftp2 10.0.1.11:22 send-proxy-v2
```

## Integration Testing

ApacheMinaSSHD.NET includes real OpenSSH integration tests. Run them to validate your configuration:

```powershell
# Windows Forms sample
dotnet run --project Sample\SimpleSSHDSever\SimpleSSHDSever.csproj -- --integration-tests

# Console sample (lightweight, no GUI)
dotnet run --project Sample\ConsoleSftpServer\ConsoleSftpServer.csproj
```

This tests:
- SFTP file transfer
- SCP file transfer
- Multiple concurrent connections
- Large file transfers
- Authentication methods

## CI/CD Pipeline

The repository CI workflow performs:

1. **Build** — Compile all projects
2. **Unit tests** — Run xUnit tests
3. **Public API guard** — Ensure no Java types leak through public APIs
4. **XML documentation guard** — Verify all public APIs have IntelliSense docs
5. **Pack validation** — Build NuGet packages
6. **Integration tests** — Real OpenSSH client testing
7. **Security scan** — Check NuGet and Maven dependencies for CVEs

Use the provided scripts in your own CI:

```powershell
dotnet build ApacheMinaSSHD.NET.Wrapper\ApacheMinaSSHD.NET.Wrapper.csproj
./eng/verify-public-api.ps1
./eng/verify-xml-docs.ps1
./eng/security-scan.ps1
```

## Production Hardening Checklist

- [ ] Run as a non-administrator user
- [ ] Use strong, unique host keys per instance
- [ ] Implement authentication against your identity provider
- [ ] Apply production defaults: `server.Config.ApplyProductionDefaults()`
- [ ] Apply modern algorithms: `server.Config.ApplyModernAlgorithmDefaults()`
- [ ] Configure absolute root jail paths
- [ ] Set up log forwarding to a centralized system
- [ ] Enable PROXY protocol if behind a load balancer
- [ ] Run `security-scan.ps1` in your deployment pipeline
- [ ] Restrict firewall rules to authorized IP ranges
- [ ] Set up monitoring and alerting for failed authentications
- [ ] Plan host key rotation policy
- [ ] Keep ApacheMinaSSHD.NET updated via NuGet

## Sample Project Reference

For a complete production server implementation, see [ProductionServer](../../Sample/ProductionServer/) — it includes external JSON configuration, algorithm selection, combined SFTP/SCP listeners, and all the patterns described in this guide.

## Troubleshooting Common Issues

### "Host key file not found"

The `AMNetSimpleGeneratorHostKeyProvider` generates the key on first run. Ensure the directory is writable and persistent between restarts.

### Connection refused

- Verify the port is not blocked by a firewall
- Check that another process is not already listening on the port

### Slow transfers

- Check network latency between client and server
- Verify `REKEY_BYTES_LIMIT` is not too low (default: 1 GB)
- Consider enabling GCM ciphers for hardware-accelerated encryption

### Authentication failures

- Verify the authenticator is configured with `SetPasswordAuthenticator()` or equivalent
- Check that `MAX_AUTH_REQUESTS` has not been exceeded
- Ensure `AUTH_TIMEOUT` provides enough time for credential entry

---

*ApacheMinaSSHD.NET is developed and maintained by **SERALYNX LLC**. For a production-ready portable SFTP server with GUI management, automated deployment, and enterprise support, visit [Porta SFTP Server](https://portasftpserver.com/portable-sftp-server-community-edition/).*
