// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetTcpForwardingFilter
    {
        bool CanListen(string host, int port, ISshSession session);
        bool CanConnect(AMNetForwardingType type, string host, int port, ISshSession session);
    }
}
