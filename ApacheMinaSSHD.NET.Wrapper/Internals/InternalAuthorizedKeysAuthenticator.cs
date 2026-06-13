// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
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
