using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.Network.Connections;

namespace PingPongClient.NetworkLayer
{
    class ServerSessionResponseHandler
    {
        public SessionConnectParameters ConnectParameters { get; set; }

        EventWaitHandle ReceivedEvent { get; set; }

        public GameLogger Logger { get; set; }

        public Socket AcceptedSocket { get; private set; }
        PackageAdapter Adapter { get; set; }
        public NetworkConnection Connection { get; private set; }
        public int SessionID { get; private set; }
        public bool GameReconnect { get; private set; }
        public bool Connected { get; private set; }

        private bool Error { get; set; }
        public string ErrorMessage { get; private set; }

        public ServerSessionResponseHandler(Socket acceptedSocket, PackageAdapter adapter, GameLogger logger)
        {
            Logger = logger;
            AcceptedSocket = acceptedSocket;
            Adapter = adapter;
            Connected = false;
            Error = false;
            ErrorMessage = "";
        }

        public bool GetResponse()
        {
            HandleSessionRequest();

            return Connected;
        }

        private void HandleSessionRequest()
        {
            TCPPacketConnection tcpConnection;
            try
            {
                ReceivedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

                ClientSessionRequest sessionRequest = new ClientSessionRequest();
                sessionRequest.Reconnect = ConnectParameters.Reconnect;
                sessionRequest.ReconnectSessionID = ConnectParameters.SessionID;

                tcpConnection = new TCPPacketConnection(AcceptedSocket);
                tcpConnection.Logger = Logger;
                tcpConnection.DataReceivedEvent += ReadIDResponse;
                tcpConnection.Send(Adapter.CreateNetworkDataFromPackage(sessionRequest));
                tcpConnection.InitializeReceiving();

                if (ReceivedEvent.WaitOne(5000) && !Error)
                {
                    Connection = new NetworkConnection(tcpConnection);
                    Connection.ClientSession = new Session(SessionID);
                    Connected = true;
                    return;
                }

                tcpConnection.Disconnect();
            }
            catch { }

            ErrorMessage = "Server timeout while receiving session ID!";
        }

        private void ReadIDResponse(object sender, byte[] data)
        {
            try
            {
                (sender as TCPConnection).DataReceivedEvent -= ReadIDResponse;
                ServerSessionResponse responsePackage = Adapter.CreatePackagesFromStream(data)[0] as ServerSessionResponse;
                SessionID = responsePackage.ClientSessionID;

                if(responsePackage.GameReconnect)
                    ConnectParameters.SetGameReconnect();
            }
            catch
            {
                Error = true;
            }
            ReceivedEvent.Set();
        }
    }
}
