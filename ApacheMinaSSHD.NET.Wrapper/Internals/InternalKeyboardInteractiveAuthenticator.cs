// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
        /// This is authentication method is called after the generateChallenge has been process / called.
        /// </summary>
        /// <param name="session">This is the current server session context for the client</param>
        /// <param name="username">This is username that is derived from the client side</param>
        /// <param name="responses">This is the list of responses e.g generated challenge, TOTP, 2FA, etc.</param>
        /// <returns></returns>
        public bool authenticate(ServerSession session, string username, List responses)
        {
            var wrappedSession = new SshSession(session);

            var managedResponses = new ResponseList(responses);

            return interactiveAuthenticator.Authenticate(wrappedSession, username, managedResponses);
        }


        /// <summary>
        /// This method is called first before the authentication
        /// </summary>
        /// <param name="session">This is the current server session context for the client</param>
        /// <param name="username">This is username that is derived from the client side</param>
        /// <param name="lang">This is the language the is preferred by the client, default is en-US</param>
        /// <param name="subMethod">This the submethod specified such as if for the TOTP/2FA, RADIUS, etc.</param>
        /// <returns></returns>
        public InteractiveChallenge generateChallenge(ServerSession session, string username, string lang, string subMethod)
        {


            // Let the C# user define their requirements
            // this can come from the database / per user if necessary

            var sshChallenge = new SshChallenge(); 

            interactiveAuthenticator.GenerateChallenge(username, sshChallenge);

            // Map to Java InteractiveChallenge
            var ic = new InteractiveChallenge();


            // Check if the user is in the "optional" 2FA list.
            // this is usefull if we want to NOT set multi-step auth for some user.
            // if (username == "test")
            // {
            //    return javaChallenge; // Return null to skip sending a visual challenge
            // }

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
