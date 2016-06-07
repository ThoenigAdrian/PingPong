using System.Net;

namespace PingPongClient.NetworkLayer
{
    class NetworkTCP : DataNetwork
    {
        public NetworkTCP(IPAddress serverIP) : base(serverIP)
        {
        }

        protected override Socket InitializeSocket()
        {
            return new Socket(NetworkFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public override void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        public override byte[] Receive()
        {
            byte[] data = null;
            ConnectionSocket.Receive(data);
            return data;
        }

        protected override void PostConnectActions()
        {
        }
    }
}
