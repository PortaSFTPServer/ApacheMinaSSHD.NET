// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
