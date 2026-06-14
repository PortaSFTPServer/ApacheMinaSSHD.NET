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

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal sealed record SshSessionEvent : ISshSessionEvent
    {
        public required ISshSession Session { get; init; }

        public string? EventName { get; init; }

        public int? Reason { get; init; }

        public string? Message { get; init; }

        public string? Language { get; init; }

        public bool? Initiator { get; init; }

        public string? Version { get; init; }

        public IReadOnlyList<string> ExtraLines { get; init; } = Array.Empty<string>();

        public IReadOnlyDictionary<string, string> ClientProposal { get; init; } =
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> ServerProposal { get; init; } =
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> NegotiatedOptions { get; init; } =
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> Proposal { get; init; } =
            new Dictionary<string, string>();

        public Exception? Exception { get; init; }
    }
}
