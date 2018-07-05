using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.PackageAdapters;
using XSLibrary.Network.Connections;

namespace PingPongClient.NetworkLayer
{
    public class ClientNetwork : NetworkInterface
    {
        public int ClientSession { get; set; }

        ServerSessionResponseHandler ResponseHandler { get; set; }

        public ClientNetwork(Socket connectedSocket, GameLogger logger)
            : base(new UDPConnection(connectedSocket.LocalEndPoint as IPEndPoint), logger)
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
                AddClientConnection(ResponseHandler.Connection);
            }
            else
            {
                Disconnect();
            }
            return response;
        }

        public void IssueServerResponse(ResponseRequest responseRequest)
        {
            IssueResponse(responseRequest, ClientSession);
        }

        public void SendClientQueueMatchmaking(int playerCount, int[] playerTeamWish)
        {
            SendClientInitializePackage(ClientInitializeGamePackage.RequestType.Matchmaking, playerCount, playerTeamWish);
        }

        public void SendClientObserve(int playerCount)
        {
            SendClientInitializePackage(ClientInitializeGamePackage.RequestType.Observe, playerCount, new int[playerCount]);
        }

        private void SendClientInitializePackage(ClientInitializeGamePackage.RequestType requestType, int playerCount, int[] teamwishes)
        {
            ClientInitializeGamePackage package = new ClientInitializeGamePackage();
            package.Request = requestType;
            package.GamePlayerCount = playerCount;
            package.PlayerTeamwish = teamwishes;
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

        public ServerGameControlPackage GetGameStatus()
        {
            return In.GetDataTCP(ClientSession) as ServerGameControlPackage;
        }

        public PackageInterface[] GetTCPPackages()
        {
            return In.GetAllDataTCP(ClientSession);
        }

        protected override void PostDisconnectActions()
        {
            TerminateUDPConnection();
        }
    }
}