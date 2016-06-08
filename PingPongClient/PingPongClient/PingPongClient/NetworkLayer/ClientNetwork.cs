
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataStructs;
using NetworkLibrary.Utility;
using System.Net;
using NetworkLibrary.ConnectionImplementations;
using NetworkLibrary.ConnectionImplementations.NetworkImplementations;

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
            UDPOutAdapter = new ClientUDPAdapter();
            UDPInAdapter = new ServerUDPAdapter();

            TCPOutAdapter = new ClientTCPAdapter();
            TCPInAdapter = new ServerTCPAdapter();
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendClientDataTCP(package);
        }

        public ServerDataPackage GetServerData()
        {
            return GetServerDataUDP() as ServerDataPackage;
        }
    }
}