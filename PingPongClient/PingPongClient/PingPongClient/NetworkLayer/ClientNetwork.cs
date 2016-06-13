using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations;
using System.Net.Sockets;
using NetworkLibrary.DataPackages;
using System.Net;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.PackageAdapters;
using System.Threading;

namespace PingPongClient.NetworkLayer
{
    public class ClientNetwork : NetworkInterface
    {
        public int ClientSession { get; set; }

        EventWaitHandle ReceivedEvent { get; set; }

        public ClientNetwork(Socket connectedSocket)
            : this(connectedSocket, null)
        {
        }

        public ClientNetwork(Socket connectedSocket, LogWriter logger)
            : base(new UDPConnection(connectedSocket.LocalEndPoint as IPEndPoint, logger), logger)
        {
            ReceivedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

            TCPConnection tcpConnection = new TCPConnection(connectedSocket, logger);
            tcpConnection.DataReceivedEvent += ReadIDRespone;
            tcpConnection.InitializeReceiving();

            if (ReceivedEvent.WaitOne(5000))
            {
                NetworkConnection serverConnection = new NetworkConnection(tcpConnection, ClientSession);
                AddClientConnection(serverConnection);
                return;
            }

            tcpConnection.Disconnect();

            throw new ConnectionException("Server timeout while receiving session ID!");
        }

        private void ReadIDRespone(TCPConnection sender, byte[] data)
        {
            sender.DataReceivedEvent -= ReadIDRespone;
            PackageAdapter adapter = new PackageAdapter();
            ServerSessionResponse responsePackage = adapter.CreatePackagesFromStream(data)[0] as ServerSessionResponse;
            ClientSession = responsePackage.ClientSessionID;
            ReceivedEvent.Set();
        }

        public void SendClientStart()
        {
            ClientInitializeGamePackage package = new ClientInitializeGamePackage();
            package.PlayerCount = 2;
            SendIDPackageTCP(package);
        }

        public void SendClientJoin()
        {
            ClientJoinGameRequest package = new ClientJoinGameRequest();
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