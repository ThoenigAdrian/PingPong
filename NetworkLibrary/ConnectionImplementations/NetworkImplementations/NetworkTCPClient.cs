using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    class NetworkTCPClient : NetworkTCP
    {
        IPEndPoint NetworkEndPoint { get; set; }

        public NetworkTCPClient(IPEndPoint target)
            : base(new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            NetworkEndPoint = target;
        }

        protected override void NetworkSpecificInitializing()
        {
            ConnectionSocket.Connect(NetworkEndPoint);
        }
    }

}
