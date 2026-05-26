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
