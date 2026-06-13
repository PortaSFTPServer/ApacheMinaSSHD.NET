// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Represents a keyboard-interactive authentication challenge sent to a client.
    /// </summary>
    public interface ISshChallenge
    {
        /// <summary>Gets or sets the challenge name shown to the client.</summary>
        string InteractionName { get; set; }
        /// <summary>Gets or sets instructions shown with the challenge.</summary>
        string InteractionInstruction { get; set; }
        /// <summary>Gets or sets the language tag for challenge text.</summary>
        string LanguageTag { get; set; }
        /// <summary>Gets prompts already added to the challenge.</summary>
        IReadOnlyList<(string Prompt, bool Echo)> Prompts { get; }
        /// <summary>
        /// Adds a prompt to the challenge.
        /// </summary>
        /// <param name="prompt">Prompt text shown to the client.</param>
        /// <param name="echo">Whether the client may echo the response while typing.</param>
        void AddPrompt(string prompt, bool echo = false);
    }


}
