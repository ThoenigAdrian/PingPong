using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;
using System;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class UDPConnection: ConnectionInterface
    {
        protected IPEndPoint connectionEnd;
        public IPEndPoint connectionLocal;

        public UDPConnection(IPEndPoint target, IPEndPoint local) : base(new Socket(target.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            connectionEnd = target;
            connectionLocal = local;
        }

        protected override DataContainer<byte[]> InitializeDataContainer()
        {
            return new DoubleBuffer<byte[]>();
        }

        public override void Send(byte[] data)
        {
            ConnectionSocket.SendTo(data, connectionEnd);
        }

        protected override void InitializeSocket(Socket socket)
        {
            base.InitializeSocket(socket);

            ConnectionSocket.Bind(connectionLocal);
        }
    }
}
