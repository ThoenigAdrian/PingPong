using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.Utility;
using System.Net.Sockets;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataPackages;

namespace PingPongServer
{
    class ServerNetwork : NetworkInterface
    {
        public ServerNetwork(Socket acceptedSocket)
            : this(acceptedSocket, null)
        {
            
        }

        public ServerNetwork(Socket acceptedSocket, LogWriter logger)
            : base(NetworkLibrary.NetworkConstants.SERVER_PORT, logger)
        {
            Log("Built up network.");
            AddTCPConnection(acceptedSocket);
        }

        protected override PackageAdapter InitializeAdapter()
        {
            return new PackageAdapter();
        }

        public void SendObjectPositions(ServerDataPackage serverData)
        {
            SendDataUDP(serverData, 0);
        }

        public PlayerMovementPackage GetPlayerMovement()
        {
            return GetDataTCP(0) as PlayerMovementPackage;
        }
    }
}
