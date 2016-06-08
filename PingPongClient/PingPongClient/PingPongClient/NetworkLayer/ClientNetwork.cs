
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
            UDPOutAdapter = new ClientUDPAdapter();
            UDPInAdapter = new ServerUDPAdapter();

            TCPOutAdapter = new ClientTCPAdapter();
            TCPInAdapter = new ServerTCPAdapter();
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendDataTCP(package);
        }

        public ServerDataPackage GetServerData()
        {
            return GetDataUDP() as ServerDataPackage;
        }
    }
}