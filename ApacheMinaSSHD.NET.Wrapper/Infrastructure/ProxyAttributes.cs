// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

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
