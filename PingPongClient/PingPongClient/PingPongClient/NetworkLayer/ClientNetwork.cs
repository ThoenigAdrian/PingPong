using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using System.Net.Sockets;
using NetworkLibrary.DataPackages;
using System.Net;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.DataPackages.ClientSourcePackages;

namespace PingPongClient.NetworkLayer
{
    public class ClientNetwork : NetworkInterface
    {
        public int ClientSession { get; set; }

        ServerSessionResponseHandler ResponseHandler { get; set; }

        public ClientNetwork(Socket connectedSocket, LogWriter logger)
            : base(new UDPConnection(connectedSocket.LocalEndPoint as IPEndPoint, logger), logger)
        {
            ResponseHandler = new ServerSessionResponseHandler(connectedSocket);
        }

        public bool GetServerSessionResponse()
        {
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
                      

        public void SendClientStart(int playerCount)
        {
            ClientInitializeGamePackage package = new ClientInitializeGamePackage();
            package.PlayerCount = playerCount;
            SendIDPackageTCP(package);
        }

        public void SendClientJoin(int playerCount)
        {
            ClientJoinGameRequest package = new ClientJoinGameRequest();
            package.PlayerCountOfClient = playerCount;
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
            SendDataTCP(package, ClientSession);
        }

        public void SendUDPTestData(PlayerMovementPackage package)
        {
            package.SessionID = ClientSession;
            SendDataUDP(package, ClientSession);
        }

        public ServerDataPackage GetServerData()
        {
            return GetDataUDP(ClientSession) as ServerDataPackage;
        }
    }
}