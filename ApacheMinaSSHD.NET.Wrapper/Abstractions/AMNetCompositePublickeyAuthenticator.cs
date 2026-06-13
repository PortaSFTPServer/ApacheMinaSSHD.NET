// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Public key authenticator that accepts a key when any inner authenticator accepts it.
    /// </summary>
    public sealed class AMNetCompositePublickeyAuthenticator : IAMNetPublickeyAuthenticator
    {
        private readonly IReadOnlyList<IAMNetPublickeyAuthenticator> authenticators;

        /// <summary>
        /// Creates a composite public key authenticator.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try in order.</param>
        public AMNetCompositePublickeyAuthenticator(params IAMNetPublickeyAuthenticator[] authenticators)
            : this((IEnumerable<IAMNetPublickeyAuthenticator>)authenticators)
        {
        }

        /// <summary>
        /// Creates a composite public key authenticator.
        /// </summary>
        /// <param name="authenticators">The public key authenticators to try in order.</param>
        public AMNetCompositePublickeyAuthenticator(IEnumerable<IAMNetPublickeyAuthenticator> authenticators)
        {
            ArgumentNullException.ThrowIfNull(authenticators);
            this.authenticators = authenticators
                .Select(authenticator => authenticator ?? throw new ArgumentException("Authenticator entries cannot be null.", nameof(authenticators)))
                .ToArray();
        }

        /// <summary>
        /// Gets the configured public key authenticators in evaluation order.
        /// </summary>
        public IReadOnlyList<IAMNetPublickeyAuthenticator> Authenticators => authenticators;

        /// <inheritdoc />
        public bool Authenticate(string username, string incomingFingerprint, ISshSession session)
        {
            foreach (IAMNetPublickeyAuthenticator authenticator in authenticators)
            {
                if (authenticator.Authenticate(username, incomingFingerprint, session))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
