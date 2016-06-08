using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
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
