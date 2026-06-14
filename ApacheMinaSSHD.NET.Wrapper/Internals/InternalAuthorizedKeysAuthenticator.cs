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

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using java.nio.file;
using org.apache.sshd.server.config.keys;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalAuthorizedKeysAuthenticator : AuthorizedKeysAuthenticator
    {

        public InternalAuthorizedKeysAuthenticator(IAMNetAuthorizedKeysAuthenticator authorizedKeysAuthenticator)
            : base(GetJavaPath(authorizedKeysAuthenticator.KeysFilePath)) { }

        /// <summary>
        /// Path must be absolute for AuthorizedKeysAuthenticator to work in IKVM.
        /// </summary>
        /// <param name="path">The filesystem path to convert to a Java Path.</param>
        /// <returns>A java.nio.file.Path for the given string path.</returns>
        /// <exception cref="System.ArgumentException">Thrown when path is not absolute.</exception>
        private static java.nio.file.Path GetJavaPath(string path)
        {
            if (!System.IO.Path.IsPathRooted(path))
            {
                throw new System.ArgumentException("Path must be absolute for AuthorizedKeysAuthenticator to work in IKVM.");
            }

            return Paths.get(path);
        }
    }
}
