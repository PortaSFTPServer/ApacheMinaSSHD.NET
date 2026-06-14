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

namespace ApacheMinaSSHD.NET.Wrapper
{
    /// <summary>
    /// SSH authentication method names and helpers for building multi-step
    /// authentication policies without raw protocol strings.
    /// </summary>
    /// <remarks>
    /// SSH authentication policies use a space-separated list of comma-separated
    /// method chains. Each chain lists methods that must all succeed in order, while
    /// the outer list represents alternatives. For example,
    /// <c>publickey password,keyboard-interactive</c> allows either public key alone
    /// or password followed by keyboard-interactive.
    /// </remarks>
    public static class AMNetSshAuthenticationMethods
    {
        /// <summary>
        /// Username and password authentication.
        /// </summary>
        public const string Password = "password";

        /// <summary>
        /// Public key authentication, including authorized_keys-backed flows.
        /// </summary>
        public const string PublicKey = "publickey";

        /// <summary>
        /// Keyboard-interactive authentication such as one-time codes or custom prompts.
        /// </summary>
        public const string KeyboardInteractive = "keyboard-interactive";

        /// <summary>
        /// Builds a required authentication chain where every method must succeed in order.
        /// </summary>
        /// <param name="authenticationMethods">Authentication methods such as <see cref="Password"/> or <see cref="PublicKey"/>.</param>
        /// <returns>A comma-separated authentication chain.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationMethods"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="authenticationMethods"/> is empty.</exception>
        public static string RequireAll(params string[] authenticationMethods)
        {
            return RequireAll((IEnumerable<string>)authenticationMethods);
        }

        /// <summary>
        /// Builds a required authentication chain where every method must succeed in order.
        /// </summary>
        /// <param name="authenticationMethods">Authentication methods such as <see cref="Password"/> or <see cref="PublicKey"/>.</param>
        /// <returns>A comma-separated authentication chain.</returns>
        public static string RequireAll(IEnumerable<string> authenticationMethods)
        {
            ArgumentNullException.ThrowIfNull(authenticationMethods);

            string[] methods = Normalize(authenticationMethods).ToArray();
            if (methods.Length == 0)
            {
                throw new ArgumentException("At least one authentication method is required.", nameof(authenticationMethods));
            }

            return string.Join(",", methods);
        }

        /// <summary>
        /// Builds an authentication policy where any one of the supplied chains may succeed.
        /// </summary>
        /// <param name="authenticationChains">Authentication chains such as <see cref="PublicKey"/> or values returned by <see cref="RequireAll(string[])"/>.</param>
        /// <returns>A space-separated authentication policy string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationChains"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="authenticationChains"/> is empty.</exception>
        public static string AllowAny(params string[] authenticationChains)
        {
            return AllowAny((IEnumerable<string>)authenticationChains);
        }

        /// <summary>
        /// Builds an authentication policy where any one of the supplied chains may succeed.
        /// </summary>
        /// <param name="authenticationChains">Authentication chains such as <see cref="PublicKey"/> or values returned by <see cref="RequireAll(string[])"/>.</param>
        /// <returns>A space-separated authentication policy string.</returns>
        public static string AllowAny(IEnumerable<string> authenticationChains)
        {
            ArgumentNullException.ThrowIfNull(authenticationChains);

            string[] chains = Normalize(authenticationChains).ToArray();
            if (chains.Length == 0)
            {
                throw new ArgumentException("At least one authentication chain is required.", nameof(authenticationChains));
            }

            return string.Join(" ", chains);
        }

        /// <summary>
        /// Parses an authentication policy into alternative chains.
        /// </summary>
        /// <param name="authenticationPolicy">The SSH authentication policy string.</param>
        /// <returns>Authentication chains in evaluation order. Returns an empty list when <paramref name="authenticationPolicy"/> is null or whitespace.</returns>
        public static IReadOnlyList<IReadOnlyList<string>> Parse(string? authenticationPolicy)
        {
            if (string.IsNullOrWhiteSpace(authenticationPolicy))
            {
                return Array.Empty<IReadOnlyList<string>>();
            }

            return authenticationPolicy
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(chain => (IReadOnlyList<string>)Normalize(
                    chain.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToArray())
                .ToArray();
        }

        private static IEnumerable<string> Normalize(IEnumerable<string> values)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                yield return value.Trim();
            }
        }
    }
}
