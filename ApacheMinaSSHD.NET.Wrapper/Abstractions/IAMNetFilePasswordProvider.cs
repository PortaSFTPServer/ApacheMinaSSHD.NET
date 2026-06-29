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

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Provides passwords for encrypted host key files.
    /// </summary>
    public interface IAMNetFilePasswordProvider
    {
        /// <summary>
        /// Returns the password for an encrypted key resource.
        /// </summary>
        /// <param name="resourceKey">The resource identifier (e.g., file path).</param>
        /// <param name="retryIndex">The retry attempt index (0 on first call, 1+ on retries).</param>
        /// <returns>The password, or <c>null</c> to abort loading.</returns>
        string? GetPassword(string resourceKey, int retryIndex);
    }
}
