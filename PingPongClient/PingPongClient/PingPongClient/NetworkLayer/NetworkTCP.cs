using System.Net;

namespace PingPongClient.NetworkLayer
{
    class NetworkTCP : DataNetwork<byte[]>
    {
        public NetworkTCP(IPAddress serverIP) : base(serverIP)
        {
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
