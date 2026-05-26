using org.apache.sshd.common;
using System.Collections.Concurrent;

namespace ApacheMinaSSHD.NET.Wrapper.Infrastructure
{
    internal static class ProxyAttributes
    {
        public static readonly AttributeRepository.AttributeKey PROXY_REMOTE_ADDRESS =
            new AttributeRepository.AttributeKey();

        private static readonly ConcurrentDictionary<string, AttributeRepository.AttributeKey> NamedAttributes = new();

        public static AttributeRepository.AttributeKey GetOrCreate(string key)
        {
            return NamedAttributes.GetOrAdd(key, _ => new AttributeRepository.AttributeKey());
        }
    }
}
