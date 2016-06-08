using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public class TCPClientConnection : TCPConnection
    {
        IPEndPoint ConnectionEndPoint { get; set; }

        public TCPClientConnection(IPEndPoint target)
            : base(new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            ConnectionEndPoint = target;
        }

        protected override void ConnectionSpecificInitializing()
        {
            ConnectionSocket.Connect(ConnectionEndPoint);
        }
    }

}
