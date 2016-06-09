
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using System.Net.Sockets;
using NetworkLibrary.DataPackages;

namespace PingPongClient.NetworkLayer
{
    class ClientNetwork : NetworkInterface
    {
        public ClientNetwork(Socket connectedSocket)
            : this(connectedSocket, null)
        {
        }

        public ClientNetwork(Socket connectedSocket, LogWriter logger)
            : base (connectedSocket, logger)
        {
        }

        protected override PackageAdapter InitializeAdapter()
        {
            return new PackageAdapter();
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendDataTCP(package);
        }

        public void SendPlayerMovement(PlayerMovementPackage package)
        {
            SendDataTCP(package);
        }

        public void SendUDPTestData(PlayerMovementPackage package)
        {
            SendDataUDP(package);
        }

        public ServerDataPackage GetServerData()
        {
            return GetDataUDP() as ServerDataPackage;
        }
    }
}