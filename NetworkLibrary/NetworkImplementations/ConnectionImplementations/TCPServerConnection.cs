using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public class TCPServerConnection : TCPConnection
    {
        public TCPServerConnection(Socket acceptedSocket) : base(acceptedSocket)
        {

        }

        public override void Initialize()
        {
            return;
        }
    }
}
