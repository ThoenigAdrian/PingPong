
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using System.Net.Sockets;
using NetworkLibrary.DataPackages;
using System.Net;

namespace PingPongClient.NetworkLayer
{
    class ClientNetwork : NetworkInterface
    {
        public ClientNetwork(Socket connectedSocket)
            : this(connectedSocket, null)
        {
        }

        public ClientNetwork(Socket connectedSocket, LogWriter logger)
            : base ((connectedSocket.LocalEndPoint as IPEndPoint).Port, logger)
        {
            AddClientConnection(connectedSocket);
        }

        protected override PackageAdapter InitializeAdapter()
        {
            return new PackageAdapter();
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