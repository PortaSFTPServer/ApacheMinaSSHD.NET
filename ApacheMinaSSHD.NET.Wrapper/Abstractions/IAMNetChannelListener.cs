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
/// Defines a contract for monitoring SSH channel lifecycle events.
/// Implement this interface to observe channel initialization, open, and close events.
/// </summary>
public interface IAMNetChannelListener
{
    /// <summary>
    /// Called when a new channel has been initialized on the session.
    /// </summary>
    /// <param name="session">The SSH session owning the channel.</param>
    void OnChannelInitialized(ISshSession session);

    /// <summary>
    /// Called when the channel open request has succeeded and the channel is ready for use.
    /// </summary>
    /// <param name="session">The SSH session owning the channel.</param>
    void OnChannelOpenSuccess(ISshSession session);

    /// <summary>
    /// Called when the channel open request has failed.
    /// </summary>
    /// <param name="session">The SSH session owning the channel.</param>
    /// <param name="reason">An exception describing the failure, or <c>null</c> if not available.</param>
    void OnChannelOpenFailure(ISshSession session, Exception? reason);

    /// <summary>
    /// Called when the channel state has changed.
    /// </summary>
    /// <param name="session">The SSH session owning the channel.</param>
    /// <param name="hint">An optional hint about the state change.</param>
    void OnChannelStateChanged(ISshSession session, string? hint);

    /// <summary>
    /// Called when the channel has been closed.
    /// </summary>
    /// <param name="session">The SSH session owning the channel.</param>
    /// <param name="reason">An exception describing the close reason, or <c>null</c> if normal closure.</param>
    void OnChannelClosed(ISshSession session, Exception? reason);
}
