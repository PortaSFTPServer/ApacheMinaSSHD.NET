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
