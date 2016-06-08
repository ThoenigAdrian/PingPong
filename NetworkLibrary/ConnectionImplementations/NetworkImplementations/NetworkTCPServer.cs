using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public class NetworkTCPServer : NetworkTCP
    {
        public NetworkTCPServer(Socket acceptedSocket) : base(acceptedSocket)
        {

        }

        protected override void NetworkSpecificInitializing()
        {
            return;
        }
    }
}
