using System;
using System.Collections.Generic;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default authorized_keys authenticator configuration.
    /// </summary>
    public class AMNetAuthorizedKeysAuthenticator: IAMNetAuthorizedKeysAuthenticator
    {
        /// <inheritdoc />
        public string KeysFilePath { get; }

        /// <summary>
        /// Creates an authorized_keys authenticator configuration.
        /// </summary>
        /// <param name="path">The authorized_keys file path used to validate public keys.</param>

        public AMNetAuthorizedKeysAuthenticator( string path)
        {
            this.KeysFilePath = path;
        }
    }
}
