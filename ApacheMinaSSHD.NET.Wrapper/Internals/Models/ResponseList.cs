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
