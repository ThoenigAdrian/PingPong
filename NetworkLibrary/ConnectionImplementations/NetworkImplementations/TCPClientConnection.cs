using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public class TCPClientConnection : TCPConnection
    {
        IPEndPoint NetworkEndPoint { get; set; }

        public TCPClientConnection(IPEndPoint target)
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
