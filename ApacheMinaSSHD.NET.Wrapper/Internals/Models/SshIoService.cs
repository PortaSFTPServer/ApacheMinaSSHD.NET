using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshIoService: ISshIoService
    {

        // We store the Java object internally but don't expose it
        private readonly org.apache.sshd.common.io.IoService _inner;

        public SshIoService(org.apache.sshd.common.io.IoService inner) => _inner = inner;

        public bool IsAcceptor => _inner is org.apache.sshd.common.io.IoAcceptor;
        // Corrected methods for SSHD 2.18.0
        public bool IsClosing => _inner.isClosing();
        public bool IsClosed => _inner.isClosed();

        public IEnumerable<IPEndPoint> BoundAddresses
        {
            get
            {
                if (_inner is org.apache.sshd.common.io.IoAcceptor acceptor)
                {
                    var bound = acceptor.getBoundAddresses().toArray();
                    foreach (var addr in bound)
                    {
                        if (addr is java.net.InetSocketAddress isa)
                            yield return new IPEndPoint(IPAddress.Parse(isa.getAddress().getHostAddress()), isa.getPort());
                    }
                }
            }
        }
    }
}

