// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using System.Collections.Concurrent;

namespace ApacheMinaSSHD.NET.Wrapper;

/// <summary>
/// Sliding-window per-IP connection rate limiter.
/// Tracks connection timestamps for each remote address and rejects
/// attempts that exceed <see cref="MaxConnections"/> within <see cref="Window"/>.
/// Thread-safe.
/// </summary>
public sealed class AMNetConnectionRateLimiter : IAmNetConnectionRateLimiter
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _attempts = new();
    private readonly int _maxConnections;
    private readonly TimeSpan _window;
    private readonly TimeSpan _cleanupInterval;
    private DateTime _lastCleanup = DateTime.UtcNow;

    /// <summary>
    /// Maximum number of connections allowed per <see cref="Window"/>.
    /// </summary>
    public int MaxConnections => _maxConnections;

    /// <summary>
    /// Sliding time window for rate limit evaluation.
    /// </summary>
    public TimeSpan Window => _window;

    /// <summary>
    /// Creates a rate limiter with the given thresholds.
    /// </summary>
    /// <param name="maxConnections">Max connections per window (default 10).</param>
    /// <param name="window">Sliding window duration (default 1 second).</param>
    public AMNetConnectionRateLimiter(int maxConnections = 10, TimeSpan? window = null)
    {
        if (maxConnections <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxConnections), maxConnections,
                "Max connections must be positive.");

        _maxConnections = maxConnections;
        _window = window ?? TimeSpan.FromSeconds(1);
        _cleanupInterval = TimeSpan.FromSeconds(Math.Max(1, (int)_window.TotalSeconds * 10));
    }

    /// <inheritdoc />
    public bool IsConnectionAllowed(string remoteAddress)
    {
        var now = DateTime.UtcNow;
        var queue = _attempts.GetOrAdd(remoteAddress, _ => new ConcurrentQueue<DateTime>());

        lock (queue)
        {
            while (queue.TryPeek(out var ts) && (now - ts) > _window)
            {
                queue.TryDequeue(out _);
            }

            if (queue.Count >= _maxConnections)
                return false;

            queue.Enqueue(now);
        }

        PeriodicCleanup(now);
        return true;
    }

    /// <inheritdoc />
    public void Reset(string? remoteAddress = null)
    {
        if (remoteAddress != null)
        {
            _attempts.TryRemove(remoteAddress, out _);
        }
        else
        {
            _attempts.Clear();
        }
    }

    private void PeriodicCleanup(DateTime now)
    {
        if ((now - _lastCleanup) < _cleanupInterval)
            return;

        _lastCleanup = now;
        var cutoff = now - _window - _cleanupInterval;

        foreach (var kvp in _attempts)
        {
            if (kvp.Value.TryPeek(out var ts) && ts < cutoff)
            {
                _attempts.TryRemove(kvp.Key, out _);
            }
        }
    }
}
