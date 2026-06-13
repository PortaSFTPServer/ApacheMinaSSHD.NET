// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Keyboard-interactive authenticator backed by .NET callbacks for challenge
    /// generation and response validation.
    /// </summary>
    public sealed class AMNetDelegateKeyboardInteractiveAuthenticator : IAMNetKeyboardInteractiveAuthenticator
    {
        private readonly Action<string, ISshChallenge> generateChallenge;
        private readonly Func<ISshSession, string, IResponseList, bool> authenticate;

        /// <summary>
        /// Creates a keyboard-interactive authenticator backed by .NET callbacks.
        /// </summary>
        /// <param name="generateChallenge">Callback that populates prompts sent to the client.</param>
        /// <param name="authenticate">Callback that validates the client responses.</param>
        public AMNetDelegateKeyboardInteractiveAuthenticator(
            Action<string, ISshChallenge> generateChallenge,
            Func<ISshSession, string, IResponseList, bool> authenticate)
        {
            this.generateChallenge = generateChallenge ?? throw new ArgumentNullException(nameof(generateChallenge));
            this.authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
        }

        /// <inheritdoc />
        public void GenerateChallenge(string username, ISshChallenge challenge)
        {
            generateChallenge(username, challenge);
        }

        /// <inheritdoc />
        public bool Authenticate(ISshSession session, string username, IResponseList response)
        {
            return authenticate(session, username, response);
        }
    }
}
