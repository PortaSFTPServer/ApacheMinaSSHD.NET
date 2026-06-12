using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions;

/// <summary>
/// An <see cref="IAMNetIoServiceEventListener"/> decorator that enforces
/// per-IP connection rate limits before delegating to an inner listener.
/// </summary>
public sealed class RateLimitingIoServiceEventListener : IAMNetIoServiceEventListener
{
    private readonly IAMNetIoServiceEventListener _inner;
    private readonly IAmNetConnectionRateLimiter _rateLimiter;

    /// <summary>
    /// Creates a rate-limiting wrapper around <paramref name="inner"/>.
    /// </summary>
    public RateLimitingIoServiceEventListener(
        IAMNetIoServiceEventListener inner,
        IAmNetConnectionRateLimiter rateLimiter)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(rateLimiter);
        _inner = inner;
        _rateLimiter = rateLimiter;
    }

    /// <inheritdoc />
    public void OnConnectionAborted(ISshServiceConnection context)
    {
        _inner.OnConnectionAborted(context);
    }

    /// <inheritdoc />
    public bool OnConnectionAccepted(ISshServiceConnection context)
    {
        var remoteAddr = context.RemoteEndPoint?.Address?.ToString();
        if (!string.IsNullOrEmpty(remoteAddr) && !_rateLimiter.IsConnectionAllowed(remoteAddr))
        {
            return false;
        }

        return _inner.OnConnectionAccepted(context);
    }

    /// <inheritdoc />
    public void OnOutboundConnectionAborted(ISshServiceConnection context)
    {
        _inner.OnOutboundConnectionAborted(context);
    }

    /// <inheritdoc />
    public void OnOutboundConnectionEstablished(ISshServiceConnection context)
    {
        _inner.OnOutboundConnectionEstablished(context);
    }
}
