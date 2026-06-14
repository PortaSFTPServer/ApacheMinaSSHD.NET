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
