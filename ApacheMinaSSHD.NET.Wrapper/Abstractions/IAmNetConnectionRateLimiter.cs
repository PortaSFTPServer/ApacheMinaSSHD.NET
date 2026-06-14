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
