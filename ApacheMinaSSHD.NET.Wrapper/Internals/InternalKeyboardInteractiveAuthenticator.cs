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
using java.util;
using org.apache.sshd.server.auth.keyboard;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalKeyboardInteractiveAuthenticator : java.lang.Object, org.apache.sshd.server.auth.keyboard.KeyboardInteractiveAuthenticator
    {
        private readonly IAMNetKeyboardInteractiveAuthenticator interactiveAuthenticator;

        public InternalKeyboardInteractiveAuthenticator(IAMNetKeyboardInteractiveAuthenticator interactiveAuthenticator)
        {
            this.interactiveAuthenticator = interactiveAuthenticator;
        }

        /// <summary>
        /// Called after generateChallenge to verify the client's responses.
        /// </summary>
        /// <param name="session">The current server session context for the client.</param>
        /// <param name="username">The username provided by the client.</param>
        /// <param name="responses">The list of responses, e.g., challenge response, TOTP, 2FA, etc.</param>
        /// <returns>True if authentication succeeds, false otherwise.</returns>
        public bool authenticate(ServerSession session, string username, List responses)
        {
            var wrappedSession = new SshSession(session);

            var managedResponses = new ResponseList(responses);

            return interactiveAuthenticator.Authenticate(wrappedSession, username, managedResponses);
        }


        /// <summary>
        /// This method is called first before the authentication
        /// </summary>
        /// <param name="session">The current server session context for the client.</param>
        /// <param name="username">The username provided by the client.</param>
        /// <param name="lang">The language preferred by the client; defaults to en-US.</param>
        /// <param name="subMethod">The submethod specified, e.g., TOTP/2FA, RADIUS, etc.</param>
        /// <returns>An InteractiveChallenge defining the prompts for the client.</returns>
        public InteractiveChallenge generateChallenge(ServerSession session, string username, string lang, string subMethod)
        {


            // Let the C# user define their requirements
            // this can come from the database / per user if necessary

            var sshChallenge = new SshChallenge(); 

            interactiveAuthenticator.GenerateChallenge(username, sshChallenge);

            // Map to Java InteractiveChallenge
            var ic = new InteractiveChallenge();


            // Check if the user is in the "optional" 2FA list.

            // Sets the "Name" of the interaction (often displayed in terminal headers)
            ic.setInteractionName(sshChallenge.InteractionName);

            // Sets the "Instruction" text shown before prompts
            ic.setInteractionInstruction(sshChallenge.InteractionInstruction);

            // Sets the language tag (RFC 3066)
            ic.setLanguageTag(sshChallenge.LanguageTag);

            // Add all prompts defined in C#
            foreach (var (text, echo) in sshChallenge.Prompts)
            {
                ic.addPrompt(text, echo);
            }

            return ic;
        }
    }
}
