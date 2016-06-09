
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataStructs;
using NetworkLibrary.Utility;
using System.Net;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.NetworkImplementations;

namespace PingPongClient.NetworkLayer
{
    class ClientNetwork : NetworkInterface
    {
        public ClientNetwork(IPEndPoint server)
            : base (
                  new TCPClientConnection(server),
                  new UDPConnection(server),
                  null)

        {
            Initialize();
        }

        public ClientNetwork(IPEndPoint server, LogWriter logger)
            : base (
                  new TCPClientConnection(server),
                  new UDPConnection(server),
                  logger)
        {
            Initialize();
        }

        protected void Initialize()
        {
            m_packageAdapter = new PackageAdapter();
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendDataTCP(package);
        }

        public void SendPlayerMovement(PlayerMovementPackage package)
        {
            SendDataTCP(package);
        }

        public ServerDataPackage GetServerData()
        {
            return GetDataUDP() as ServerDataPackage;
        }
    }
}