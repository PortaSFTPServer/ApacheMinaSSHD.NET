// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using java.util;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class ResponseList(List responses) : IResponseList
    {
        private readonly List responses = responses;

        public List<string> GetResponses()
        {
            return responses.toArray().Cast<string>().ToList();

        }
    }
}
