using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class TCPClientConnection : TCPConnection
    {
        IPEndPoint ConnectionEndPoint { get; set; }

        public TCPClientConnection(IPEndPoint target)
            : base(new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            ConnectionEndPoint = target;
        }

        public override void Initialize()
        {
            ConnectionSocket.Connect(ConnectionEndPoint);
        }
    }

}
