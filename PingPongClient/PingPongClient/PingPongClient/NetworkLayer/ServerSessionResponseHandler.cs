using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using System.Net.Sockets;
using System.Threading;

namespace PingPongClient.NetworkLayer
{
    class ServerSessionResponseHandler
    {
        public SessionConnectParameters ConnectParameters { get; set; }

        EventWaitHandle ReceivedEvent { get; set; }

        public LogWriter Logger { get; set; }

        public Socket AcceptedSocket { get; private set; }
        PackageAdapter Adapter { get; set; }
        public NetworkConnection ServerConnection { get; private set; }
        public int SessionID { get; private set; }
        public bool Connected { get; private set; }

        private bool Error { get; set; }
        public string ErrorMessage { get; private set; }

        public ServerSessionResponseHandler(Socket acceptedSocket, PackageAdapter adapter, LogWriter logger)
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
            HandleSessionRespone();

            return Connected;
        }

        private void HandleSessionRespone()
        {
            TCPConnection tcpConnection;
            try
            {
                ReceivedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

                ClientSessionRequest sessionRequest;
                if (ConnectParameters.Reconnect)
                    sessionRequest = new ClientSessionRequest(ConnectParameters.SessionID);
                else
                    sessionRequest = new ClientSessionRequest();


                tcpConnection = new TCPConnection(AcceptedSocket, Logger);
                tcpConnection.Send(Adapter.CreateNetworkDataFromPackage(sessionRequest));
                tcpConnection.DataReceivedEvent += ReadIDResponse;
                tcpConnection.InitializeReceiving();

                if (ReceivedEvent.WaitOne(5000) && !Error)
                {
                    ServerConnection = new NetworkConnection(tcpConnection);
                    ServerConnection.ClientSession = new Session(SessionID);
                    Connected = true;
                    return;
                }

                tcpConnection.Disconnect();
            }
            catch { }

            ErrorMessage = "Server timeout while receiving session ID!";
        }

        private void ReadIDResponse(TCPConnection sender, byte[] data)
        {
            try
            {
                sender.DataReceivedEvent -= ReadIDResponse;
                ServerSessionResponse responsePackage = Adapter.CreatePackagesFromStream(data)[0] as ServerSessionResponse;
                SessionID = responsePackage.ClientSessionID;
            }
            catch
            {
                Error = true;
            }
            ReceivedEvent.Set();
        }
    }
}
