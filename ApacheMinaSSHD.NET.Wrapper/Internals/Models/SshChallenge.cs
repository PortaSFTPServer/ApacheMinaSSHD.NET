// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshChallenge: ISshChallenge
    {

        public string InteractionName { get; set; } = "Authentication";
        public string InteractionInstruction { get; set; } = "";
        public string LanguageTag { get; set; } = "en-US"; // default language

        private readonly List<(string, bool)> _prompts = [];
        public IReadOnlyList<(string Prompt, bool Echo)> Prompts => _prompts;
        public void AddPrompt(string prompt, bool echo = false) => _prompts.Add((prompt, echo));
    }
}
