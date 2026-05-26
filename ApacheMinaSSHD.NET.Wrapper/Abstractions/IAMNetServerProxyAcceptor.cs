using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Abstractions
{
    /// <summary>
    /// Provides a way to implement proxied connections where metadata about the
    /// client is sent before the actual SSH protocol is executed.
    /// </summary>
    public interface IAMNetServerProxyAcceptor
    {
        /// <summary>
        /// Parses and validates metadata sent before the SSH handshake, such as PROXY protocol headers.
        /// </summary>
        /// <param name="proxyMetadata">The incoming metadata buffer and connection attributes.</param>
        /// <returns><c>true</c> when SSH handshake processing may continue; otherwise <c>false</c>.</returns>
        bool acceptServerProxyMetadata(IProxyMetadata proxyMetadata);
    }
}
