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
        EventWaitHandle ReceivedEvent { get; set; }

        public LogWriter Logger { get; set; }

        public Socket AcceptedSocket { get; private set; }
        public NetworkConnection ServerConnection { get; private set; }
        public int SessionID { get; private set; }
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
                    ServerConnection = new NetworkConnection(tcpConnection, SessionID);
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
