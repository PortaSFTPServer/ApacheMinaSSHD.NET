// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Secure default keyboard-interactive authenticator implementation.
    /// </summary>
    /// <remarks>
    /// This implementation sends no prompts and denies all responses. Override this
    /// class, use <see cref="AMNetDelegateKeyboardInteractiveAuthenticator"/>, or
    /// implement <see cref="IAMNetKeyboardInteractiveAuthenticator"/> to enforce
    /// application-specific challenge and response validation.
    /// </remarks>
    public class AMNetKeyboardInteractiveAuthenticator : IAMNetKeyboardInteractiveAuthenticator
    {
        /// <summary>
        /// Creates a default keyboard-interactive authenticator.
        /// </summary>
        public AMNetKeyboardInteractiveAuthenticator()
        {
        }

        /// <inheritdoc />
        public virtual bool Authenticate(ISshSession session, string username, IResponseList response)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void GenerateChallenge(string username, ISshChallenge challenge)
        {
        }
    }
}
