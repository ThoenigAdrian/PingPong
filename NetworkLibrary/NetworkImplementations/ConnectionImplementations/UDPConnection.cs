using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class UDPConnection: ConnectionInterface
    {
        protected List<IPEndPoint> connectionEnds;
        protected IPEndPoint connectionLocal;

        public UDPConnection(IPEndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            connectionLocal = local;
            connectionEnds = new List<IPEndPoint>();
        }

        public void AddEndpoint(IPEndPoint endPoint)
        {
            connectionEnds.Add(endPoint);
        }

        protected override DataContainer<byte[]> InitializeDataContainer()
        {
            return new DoubleBuffer<byte[]>();
        }

        public virtual void Send(byte[] data, int session)
        {
            ConnectionSocket.SendTo(data, connectionEnds[session]);
        }

        public virtual void Broadcast(byte[] data)
        {
            foreach (IPEndPoint endPoint in connectionEnds)
            {
                ConnectionSocket.SendTo(data, endPoint);
            }
        }

        public override void InitializeConnection()
        {
            ConnectionSocket.Bind(connectionLocal);

            base.InitializeConnection();
        }
    }
}
