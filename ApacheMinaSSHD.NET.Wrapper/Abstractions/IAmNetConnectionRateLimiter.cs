// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions;

/// <summary>
/// Evaluates whether a new connection from a given remote address should be allowed.
/// Implementations should be thread-safe since <see cref="IsConnectionAllowed"/>
/// may be invoked concurrently from multiple I/O worker threads.
/// </summary>
public interface IAmNetConnectionRateLimiter
{
    /// <summary>
    /// Returns <c>true</c> if the connection from <paramref name="remoteAddress"/>
    /// is within the configured rate limit; <c>false</c> to reject.
    /// </summary>
    bool IsConnectionAllowed(string remoteAddress);

    /// <summary>
    /// Resets rate-limit state for a specific address, or for all addresses
    /// when <paramref name="remoteAddress"/> is <c>null</c>.
    /// </summary>
    void Reset(string? remoteAddress = null);
}
