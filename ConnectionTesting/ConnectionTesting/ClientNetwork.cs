using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.PackageAdapters;
using System.Collections.Generic;

namespace PingPongClient.NetworkLayer
{
    public class ClientNetwork : NetworkInterface
    {
        public int ClientSession { get; set; }

        ServerSessionResponseHandler ResponseHandler { get; set; }

        public ClientNetwork(Socket connectedSocket, LogWriter logger)
            : base(new UDPConnection(connectedSocket.LocalEndPoint as IPEndPoint, logger), logger)
        {
            ResponseHandler = new ServerSessionResponseHandler(connectedSocket, new JSONAdapter(), logger);
        }

        public bool GetServerFreshSessionResponse(SessionConnectParameters connectParams)
        {
            ResponseHandler.ConnectParameters = connectParams;
            bool response = ResponseHandler.GetResponse();
            if (response)
            {
                ClientSession = ResponseHandler.SessionID;
                AddClientConnection(ResponseHandler.ServerConnection);
            }
            else
            {
                Disconnect();
            }
            return response;
        }

        public Dictionary <int, PackageInterface[]> CollectDataTCP()
        {
            return In.GetDataFromEverySessionTCP();
        }

        public Dictionary<int, PackageInterface> CollectDataUDP()
        {
            return In.GetDataFromEverySessionUDP();
        }

        public void IssueServerResponse(ResponseRequest responseRequest)
        {
            IssueResponse(responseRequest, ClientSession);
        }

        public void SendClientStart(int playerCount)
        {
            ClientInitializeGamePackage package = new ClientInitializeGamePackage();
            package.GamePlayerCount = playerCount;
            package.PlayerTeamwish = new int[] { 0 };
            SendIDPackageTCP(package);
        }

        public void SendClientJoin(int playerCount)
        {
            ClientJoinGameRequest package = new ClientJoinGameRequest();
            package.GamePlayerCount = playerCount;
            package.PlayerTeamwish = new int[] { 1 };
            SendIDPackageTCP(package);
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendIDPackageTCP(package);
        }

        public void SendPlayerMovement(PlayerMovementPackage package)
        {
            SendIDPackageTCP(package);
        }

        private void SendIDPackageTCP(ClientRegisteredPackage package)
        {
            package.SessionID = ClientSession;
            Out.SendDataTCP(package, ClientSession);
        }

        private void SendIDPackageUDP(ClientRegisteredPackage package)
        {
            package.SessionID = ClientSession;
            Out.SendDataUDP(package, ClientSession);
        }

        public void SendUDPTestData(PlayerMovementPackage package)
        {
            package.SessionID = ClientSession;
            Out.SendDataUDP(package, ClientSession);
        }

        public ServerDataPackage GetServerData()
        {
            return In.GetDataUDP(ClientSession) as ServerDataPackage;
        }

        protected override void PostDisconnectActions()
        {
            TerminateUDPConnection();
        }
    }
}