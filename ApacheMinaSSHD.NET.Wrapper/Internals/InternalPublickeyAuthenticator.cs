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
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using java.security;
using java.security.interfaces;
using org.apache.sshd.common.config.keys;
using org.apache.sshd.server.session;


namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalPublickeyAuthenticator : java.lang.Object, org.apache.sshd.server.auth.pubkey.PublickeyAuthenticator
    {

        // enforce the same naming convention
        private readonly IAMNetPublickeyAuthenticator _publickeyAuthenticator;

        public InternalPublickeyAuthenticator(IAMNetPublickeyAuthenticator PublickeyAuthenticator)
        {

            _publickeyAuthenticator = PublickeyAuthenticator;
        }

        /// <summary>
        /// This is the mapping for the Public key Authentication using the PublickeyAuthenticator
        /// </summary>
        /// <param name="username">The username from the SSH session.</param>
        /// <param name="pk">The public key provided by the client.</param>
        /// <param name="session">The current server session.</param>
        /// <returns>True if authentication succeeds, false otherwise.</returns>
        public bool authenticate(string username, PublicKey pk, ServerSession session)
        {
            var wrappedSession = new SshSession(session);

            string incomingFingerprint = KeyUtils.getFingerPrint(pk);

            ExtractKeyInfo(pk, out var algorithmId, out var algorithm, out var keySize);
            wrappedSession.SetKeyInfo(algorithmId, algorithm, keySize);

            var result = _publickeyAuthenticator.Authenticate(username, incomingFingerprint, wrappedSession);

            return result;
        }

        private static void ExtractKeyInfo(PublicKey pk, out string? algorithmId, out string? algorithm, out int keySize)
        {
            algorithm = pk.getAlgorithm();
            algorithmId = null;
            keySize = 0;

            if (pk is RSAPublicKey rsaKey)
            {
                algorithmId = "ssh-rsa";
                keySize = rsaKey.getModulus().bitLength();
            }
            else if (pk is DSAPublicKey dsaKey)
            {
                algorithmId = "ssh-dss";
                keySize = dsaKey.getParams().getP().bitLength();
            }
            else if (pk is ECPublicKey ecKey)
            {
                keySize = ecKey.getParams().getCurve().getField().getFieldSize();
                algorithmId = keySize switch
                {
                    256 => "ecdsa-sha2-nistp256",
                    384 => "ecdsa-sha2-nistp384",
                    521 => "ecdsa-sha2-nistp521",
                    _ => "ecdsa-sha2-nistp256"
                };
            }
            else
            {
                algorithmId = algorithm?.ToLowerInvariant() switch
                {
                    "eddsa" or "ed25519" => "ssh-ed25519",
                    "ed448" => "ssh-ed448",
                    _ => null
                };
            }
        }

    }

}
