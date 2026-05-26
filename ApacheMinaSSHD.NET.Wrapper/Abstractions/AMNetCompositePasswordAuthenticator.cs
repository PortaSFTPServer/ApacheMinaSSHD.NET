using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Password authenticator that accepts a login when any inner authenticator accepts it.
    /// </summary>
    public sealed class AMNetCompositePasswordAuthenticator : IAMNetPasswordAuthenticator
    {
        private readonly IReadOnlyList<IAMNetPasswordAuthenticator> authenticators;

        /// <summary>
        /// Creates a composite password authenticator.
        /// </summary>
        /// <param name="authenticators">The password authenticators to try in order.</param>
        public AMNetCompositePasswordAuthenticator(params IAMNetPasswordAuthenticator[] authenticators)
            : this((IEnumerable<IAMNetPasswordAuthenticator>)authenticators)
        {
        }

        /// <summary>
        /// Creates a composite password authenticator.
        /// </summary>
        /// <param name="authenticators">The password authenticators to try in order.</param>
        public AMNetCompositePasswordAuthenticator(IEnumerable<IAMNetPasswordAuthenticator> authenticators)
        {
            ArgumentNullException.ThrowIfNull(authenticators);
            this.authenticators = authenticators
                .Select(authenticator => authenticator ?? throw new ArgumentException("Authenticator entries cannot be null.", nameof(authenticators)))
                .ToArray();
        }

        /// <summary>
        /// Gets the configured password authenticators in evaluation order.
        /// </summary>
        public IReadOnlyList<IAMNetPasswordAuthenticator> Authenticators => authenticators;

        /// <inheritdoc />
        public bool Authenticate(string username, string password, ISshSession session)
        {
            foreach (IAMNetPasswordAuthenticator authenticator in authenticators)
            {
                if (authenticator.Authenticate(username, password, session))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
