using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Default keyboard-interactive authenticator implementation.
    /// </summary>
    /// <remarks>
    /// Override this class or implement <see cref="IAMNetKeyboardInteractiveAuthenticator"/>
    /// to enforce application-specific challenge and response validation.
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

            // do the authentication here

            return true;
        }

        /// <inheritdoc />
        public virtual void GenerateChallenge(string username, ISshChallenge challenge)
        {

            
            challenge.InteractionInstruction = "Additional Security Authentication";
            challenge.AddPrompt("2FA Code",false);
        }
    }
}
