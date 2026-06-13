// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides responses returned by a keyboard-interactive authentication client.
    /// </summary>
    public interface IResponseList
    {
        /// <summary>
        /// Gets the client responses in prompt order.
        /// </summary>
        public List<string> GetResponses();
    }
}
