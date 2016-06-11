using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using System.Net.Sockets;
using NetworkLibrary.DataPackages;
using System.Net;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;

namespace PingPongClient.NetworkLayer
{
    public class ClientNetwork : NetworkInterface
    {
        public ClientNetwork(Socket connectedSocket)
            : this(connectedSocket, null)
        {
        }

        public ClientNetwork(Socket connectedSocket, LogWriter logger)
            : base(new UDPConnection(connectedSocket.LocalEndPoint as IPEndPoint, logger), logger)
        {
            TCPConnection tcpConnection = new TCPConnection(connectedSocket, logger);
            tcpConnection.InitializeConnection();
            AddClientConnection(new NetworkConnection(tcpConnection));
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendDataTCP(package, 0);
        }

        public void SendPlayerMovement(PlayerMovementPackage package)
        {
            SendDataTCP(package, 0);
        }

        public void SendUDPTestData(PlayerMovementPackage package)
        {
            SendDataUDP(package, 0);
        }

        public ServerDataPackage GetServerData()
        {
            return GetDataUDP(0) as ServerDataPackage;
        }
    }
}