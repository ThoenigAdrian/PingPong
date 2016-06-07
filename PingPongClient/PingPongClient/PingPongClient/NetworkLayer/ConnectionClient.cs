using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataStructs;
using NetworkLibrary.Utility;
using System.Net;

namespace PingPongClient.NetworkLayer
{
    class ClientConnection : ConnectionInterface
    {
        public ClientConnection(IPEndPoint server)
            : base(server, null)
        {
            Initialize();
        }

        public ClientConnection(IPEndPoint server, LogWriter logger)
            : base(server, logger)
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