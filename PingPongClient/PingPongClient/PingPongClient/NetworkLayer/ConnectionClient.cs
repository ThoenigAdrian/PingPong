using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataStructs;
using NetworkLibrary.Utility;
using System.Net;

namespace PingPongClient.NetworkLayer
{
    class ConnectionClient : ConnectionInterface
    {
        public ConnectionClient(IPEndPoint server)
            : base(server, null)
        {
            Initialize();
        }

        public ConnectionClient(IPEndPoint server, LogWriter logger)
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
            //ServerDataPackage package = new ServerDataPackage();
            //package.BallPosX = 100;
            //package.BallPosY = 100;

            //package.Player1PosX = 20;
            //package.Player1PosY = 20;

            //package.Player2PosX = 180;
            //package.Player2PosY = 20;

            //return package;

            return GetServerDataUDP() as ServerDataPackage;
        }
    }
}