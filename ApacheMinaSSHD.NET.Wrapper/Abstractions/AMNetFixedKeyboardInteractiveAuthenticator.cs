// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System.Security.Cryptography;
using System.Text;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Keyboard-interactive authenticator for a single fixed response.
    /// </summary>
    /// <remarks>
    /// This class is intended for samples, tests, or simple controlled deployments.
    /// Production applications should normally validate one-time codes or secondary
    /// factors through their own identity provider.
    /// </remarks>
    public sealed class AMNetFixedKeyboardInteractiveAuthenticator : IAMNetKeyboardInteractiveAuthenticator, IDisposable
    {
        private byte[]? expectedResponseBytes;

        /// <summary>
        /// Creates a fixed keyboard-interactive authenticator.
        /// </summary>
        /// <param name="expectedResponse">The exact response to accept.</param>
        /// <param name="username">Optional exact username to accept. When null, any username can answer the challenge.</param>
        /// <param name="prompt">Prompt text shown to the client.</param>
        /// <param name="interactionName">Challenge name shown to the client.</param>
        /// <param name="instruction">Instruction text shown with the challenge.</param>
        public AMNetFixedKeyboardInteractiveAuthenticator(
            string expectedResponse,
            string? username = null,
            string prompt = "Verification code",
            string interactionName = "Authentication",
            string instruction = "Enter the verification code.")
        {
            if (expectedResponse is null)
            {
                throw new ArgumentNullException(nameof(expectedResponse));
            }

            Username = username;
            Prompt = string.IsNullOrWhiteSpace(prompt) ? "Verification code" : prompt;
            InteractionName = string.IsNullOrWhiteSpace(interactionName) ? "Authentication" : interactionName;
            Instruction = string.IsNullOrWhiteSpace(instruction) ? "Enter the verification code." : instruction;
            expectedResponseBytes = Encoding.UTF8.GetBytes(expectedResponse);
        }

        /// <summary>
        /// Gets the optional exact username accepted by this authenticator.
        /// </summary>
        public string? Username { get; }

        /// <summary>
        /// Gets the prompt text shown to the client.
        /// </summary>
        public string Prompt { get; }

        /// <summary>
        /// Gets the challenge name shown to the client.
        /// </summary>
        public string InteractionName { get; }

        /// <summary>
        /// Gets the instruction text shown with the challenge.
        /// </summary>
        public string Instruction { get; }

        /// <inheritdoc />
        public void GenerateChallenge(string username, ISshChallenge challenge)
        {
            challenge.InteractionName = InteractionName;
            challenge.InteractionInstruction = Instruction;
            challenge.AddPrompt(Prompt, echo: false);
        }

        /// <inheritdoc />
        public bool Authenticate(ISshSession session, string username, IResponseList response)
        {
            if (Username is not null && !string.Equals(Username, username, StringComparison.Ordinal))
            {
                return false;
            }

            List<string> responses = response.GetResponses();
            if (responses.Count != 1)
            {
                return false;
            }

            if (expectedResponseBytes == null)
            {
                return false;
            }

            byte[] incomingBytes = Encoding.UTF8.GetBytes(responses[0] ?? string.Empty);
            return incomingBytes.Length == expectedResponseBytes.Length
                && CryptographicOperations.FixedTimeEquals(expectedResponseBytes, incomingBytes);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (expectedResponseBytes != null)
            {
                Array.Clear(expectedResponseBytes, 0, expectedResponseBytes.Length);
                expectedResponseBytes = null;
            }
        }
    }
}
