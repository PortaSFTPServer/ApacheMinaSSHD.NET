// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    public interface IAMNetX11ForwardingFilter
    {
        bool CanForwardX11(ISshSession session, string requestType);
    }
}
