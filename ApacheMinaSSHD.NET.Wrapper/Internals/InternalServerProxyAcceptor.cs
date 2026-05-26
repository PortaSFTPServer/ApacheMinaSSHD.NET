using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using org.apache.sshd.server.session;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalServerProxyAcceptor : java.lang.Object, ServerProxyAcceptor
    {
        private readonly IAMNetServerProxyAcceptor acceptor;

        public InternalServerProxyAcceptor(IAMNetServerProxyAcceptor  acceptor)
        {
            this.acceptor = acceptor;
        }

        public bool acceptServerProxyMetadata(ServerSession ss, org.apache.sshd.common.util.buffer.Buffer b)
        {

            // this will call the implementation from IAMNetServerProxyAcceptor/ acceptor provider
            return acceptor.acceptServerProxyMetadata(new ProxyMetadata(ss,b));
        }
    }
}
