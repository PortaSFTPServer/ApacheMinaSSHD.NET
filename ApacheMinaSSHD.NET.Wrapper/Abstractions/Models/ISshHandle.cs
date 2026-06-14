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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Represents an open SFTP handle exposed through .NET-safe metadata.
    /// </summary>
    public interface ISshHandle : IDisposable
    {
        /// <summary>Gets the server handle identifier.</summary>
        string Id { get; }
        /// <summary>Gets the local physical path associated with the handle.</summary>
        string PhysicalPath { get; }
        /// <summary>Gets whether the handle is still open.</summary>
        bool IsOpen { get; }
    }
}
