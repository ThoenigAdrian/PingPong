using NetworkLibrary;
using NetworkLibrary.Utility;
using System;
using System.Net;
using System.Net.Sockets;

namespace PingPongClient.NetworkLayer
{
    class InitializeNetworkHandler
    {
        public delegate void NetworkInitializedCallback(InitializeNetworkHandler handler);
        public event NetworkInitializedCallback NetworkInitializingFinished;

        public IPAddress ServerIP { get; private set; }
        LogWriter Logger;

        public bool Error { get; private set; }
        public string Message { get; private set; }
       
        public ClientNetwork Network { get; private set; }

        public InitializeNetworkHandler(IPAddress serverIP, LogWriter logger)
        {
            Error = false;
            Message = "";

            ServerIP = serverIP;
            Logger = logger;
        }

        public void InitializeNetwork()
        {
            IPEndPoint server = new IPEndPoint(ServerIP, NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                IAsyncResult result = connectionSocket.BeginConnect(server, null, null);
                result.AsyncWaitHandle.WaitOne(5000, true);

                if (!connectionSocket.Connected)
                {
                    connectionSocket.Close();
                    Error = true;
                    Message = "Connect timeout!";
                    if (NetworkInitializingFinished != null)
                        NetworkInitializingFinished.Invoke(this);
                    return;
                }

                Network = new ClientNetwork(connectionSocket, Logger);
                Network.NetworkBuildProcessFinished += Network_NetworkBuildProcessFinished;
                Error = !Network.GetServerSessionResponse();

                if(Error)
                    Message = "Server session response error!";

                if (NetworkInitializingFinished != null)
                    NetworkInitializingFinished.Invoke(this);
                return;
            }
            catch (Exception ex)
            {
                Error = true;
                Message = "Connection failed!\n" + ex.Message;
            }

            Error = true;
            Message = "Connection failed!";

            if (NetworkInitializingFinished != null)
                NetworkInitializingFinished.Invoke(this);


        }

        private void Network_NetworkBuildProcessFinished(bool connected, string errorMessage)
        {
            
        }
    }
}
