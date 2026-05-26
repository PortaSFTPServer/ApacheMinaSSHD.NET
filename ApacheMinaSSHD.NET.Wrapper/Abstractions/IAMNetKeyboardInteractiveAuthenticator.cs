using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Implements keyboard-interactive authentication such as one-time codes or custom prompts.
    /// </summary>
    public interface IAMNetKeyboardInteractiveAuthenticator
    {
        /// <summary>
        /// Populates the challenge sent to the client.
        /// </summary>
        /// <param name="username">The username requested by the client.</param>
        /// <param name="challenge">The challenge object to populate with prompts.</param>
        void GenerateChallenge(string username, ISshChallenge challenge);

        /// <summary>
        /// Validates the client's responses to the generated challenge.
        /// </summary>
        /// <param name="session">Session metadata for the connection being authenticated.</param>
        /// <param name="username">The username requested by the client.</param>
        /// <param name="response">The responses supplied by the client.</param>
        /// <returns><c>true</c> to accept the responses; otherwise <c>false</c>.</returns>
        bool Authenticate(ISshSession session, string username, IResponseList response);
    }
}
