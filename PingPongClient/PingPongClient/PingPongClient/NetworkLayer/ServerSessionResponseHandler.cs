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
        public Socket AcceptedSocket { get; private set; }

        public delegate void ServerResponseHandler(ServerSessionResponseHandler handler);
        public event ServerResponseHandler ServerResponded;

        EventWaitHandle ReceivedEvent { get; set; }

        public LogWriter Logger { get; set; }

        int SessionID { get; set; }

        public bool Connected { get; private set; }
        private bool Error { get; set; }

        public string ErrorMessage { get; private set; }

        public ServerSessionResponseHandler(Socket acceptedSocket)
        {
            AcceptedSocket = acceptedSocket;
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

                tcpConnection = new TCPConnection(AcceptedSocket, Logger);
                tcpConnection.DataReceivedEvent += ReadIDResponse;
                tcpConnection.InitializeReceiving();

                if (ReceivedEvent.WaitOne(5000) && !Error)
                {
                    NetworkConnection serverConnection = new NetworkConnection(tcpConnection, SessionID);
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
                PackageAdapter adapter = new PackageAdapter();
                ServerSessionResponse responsePackage = adapter.CreatePackagesFromStream(data)[0] as ServerSessionResponse;
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
