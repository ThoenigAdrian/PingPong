
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataStructs;
using NetworkLibrary.Utility;
using System.Net;
using NetworkLibrary.ConnectionImplementations;
using NetworkLibrary.ConnectionImplementations.NetworkImplementations;

namespace PingPongClient.NetworkLayer
{
    class ClientConnection : ConnectionInterface
    {
        public ClientConnection(IPEndPoint server)
            : base (
                  new NetworkTCPClient(server),
                  new NetworkUDP(server),
                  null)

        {
            Initialize();
        }

        public ClientConnection(IPEndPoint server, LogWriter logger)
            : base (
                  new NetworkTCPClient(server),
                  new NetworkUDP(server),
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