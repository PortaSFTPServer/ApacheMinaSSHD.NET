using BenchmarkDotNet.Attributes;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace Benchmarks;

internal sealed class FakeSession : ISshSession
{
    public string ClientVersion => "SSH-2.0-Fake";
    public string ServerVersion => "SSH-2.0-ApacheMinaSSHD.NET";
    public string RemoteAddress => "127.0.0.1";
    public int RemotePort => 22;
    public Guid SessionId => Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public object? GetIoSession() => null;
    public T? GetNativeSession<T>() where T : class => null;
}

[MemoryDiagnoser]
public class PasswordAuthenticatorBenchmarks
{
    private readonly AMNetFixedPasswordAuthenticator _authenticator = new("demo", "demo");
    private static readonly FakeSession Session = new();

    [Benchmark(Baseline = true)]
    public bool CorrectPassword() => _authenticator.Authenticate("demo", "demo", Session);

    [Benchmark]
    public bool WrongPassword() => _authenticator.Authenticate("demo", "wrong", Session);
}
