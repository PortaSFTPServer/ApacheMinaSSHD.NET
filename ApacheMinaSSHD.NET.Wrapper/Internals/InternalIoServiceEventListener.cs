// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Internals.Models;
using ApacheMinaSSHD.NET.Wrapper.Logging;
using java.net;
using org.apache.sshd.common;
using org.apache.sshd.common.io;

namespace ApacheMinaSSHD.NET.Wrapper.Internals
{
    internal class InternalIoServiceEventListener : java.lang.Object, IoServiceEventListener
    {
        private readonly IAMNetIoServiceEventListener ioServiceEventListener;

        static readonly IAMNetLogger logger = new AMNetLogger(typeof(InternalIoServiceEventListener), AMNetLogger.LogLevel.Info);


        public InternalIoServiceEventListener(IAMNetIoServiceEventListener ioServiceEventListener )
        {
            this.ioServiceEventListener = ioServiceEventListener;
        }
        public void abortAcceptedConnection(IoAcceptor acceptor, SocketAddress local, SocketAddress remote, SocketAddress service, Exception reason)
        {
            ioServiceEventListener.OnConnectionAborted(CreateContext(acceptor, local, remote, service, reason));

        }

        public void abortEstablishedConnection(IoConnector connector, SocketAddress local, AttributeRepository context, SocketAddress remote, Exception reason)
        {
            ioServiceEventListener.OnConnectionAborted(CreateClientContext(connector, local, remote, context, reason));

        }


        public void connectionAccepted(IoAcceptor acceptor, SocketAddress local, SocketAddress remote, SocketAddress service)
        {

            var ctx = CreateContext(acceptor, local, remote, service, null!);

            if (!ioServiceEventListener.OnConnectionAccepted(ctx))
            {
                throw new IOException("Blocked by policy.");

            }
            else
            {
                logger.Info("A connection was accepted.");
            }

        }

        public void connectionEstablished(IoConnector connector, SocketAddress local, AttributeRepository context, SocketAddress remote)
        {
            ioServiceEventListener.OnOutboundConnectionEstablished(CreateClientContext(connector, local, remote, context, null!));

        }



        private ISshServiceConnection CreateContext(IoService serviceHandle, SocketAddress local, SocketAddress remote, SocketAddress serviceAddr, Exception reason)
        {
            // Internal class instantiated, but returned as the Public Interface
            return new SshServiceConnection
            {
                IoService = new SshIoService(serviceHandle),
                LocalEndPoint = Map(local),
                RemoteEndPoint = Map(remote),
                ServiceEndPoint = Map(serviceAddr),
                Attributes = new Dictionary<string, object>(), // Empty for server
                Exception = reason != null ? new Exception(reason.Message) : null!
            };
        }

        private ISshServiceConnection CreateClientContext(IoConnector connector, SocketAddress local, SocketAddress remote, AttributeRepository repo, Exception reason)
        {
            return new SshServiceConnection
            {
                IoService = new SshIoService(connector),
                LocalEndPoint = Map(local),
                RemoteEndPoint = Map(remote),
                ServiceEndPoint = null!, // Not applicable for client
                Attributes = MapAttributes(repo),
                Exception = reason != null ? new Exception(reason.Message) : null!
            };
        }

        private System.Net.IPEndPoint Map(SocketAddress addr)
        {
            if (addr is InetSocketAddress isa)
                return new System.Net.IPEndPoint(System.Net.IPAddress.Parse(isa.getAddress().getHostAddress()), isa.getPort());
            return null!;
        }

        private IReadOnlyDictionary<string, object> MapAttributes(AttributeRepository repo)
        {
            var dict = new Dictionary<string, object>();
            if (repo == null) return dict;

            // AttributeRepository.attributeKeys() returns an Iterable of AttributeKey
            var keys = repo.attributeKeys().iterator();
            while (keys.hasNext())
            {
                var key = (AttributeRepository.AttributeKey)keys.next();
                dict[key.toString()] = repo.getAttribute(key);
            }

            return dict;
        }

    }
}
