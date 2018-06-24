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

        public SessionConnectParameters ConnectParameters { get; private set; }

        GameLogger Logger;

        public bool Error { get; private set; }
        public string Message { get; private set; }
       
        public ClientNetwork Network { get; private set; }

        public InitializeNetworkHandler(SessionConnectParameters connectParams, GameLogger logger)
        {
            ConnectParameters = connectParams;

            Error = false;
            Message = "";
            Logger = logger;
        }

        public void InitializeNetwork()
        {
            Connect();

            if (NetworkInitializingFinished != null)
                NetworkInitializingFinished.Invoke(this);
        }

        private void Connect()
        {
            IPEndPoint server = new IPEndPoint(ConnectParameters.ServerIP, NetworkConstants.SERVER_PORT);
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
                    return;
                }

                Network = new ClientNetwork(connectionSocket, Logger);
                Network.SessionDied += ConnectParameters.SessionDeathHandler;
                Error = !Network.GetServerFreshSessionResponse(ConnectParameters);

                if (Error)
                    Message = "Server session response error!";
                return;
            }
            catch (Exception ex)
            {
                Message = "Connection failed!\n" + ex.Message;
            }

            Error = true;
            Message = "Connection failed!";
        }
    }
}
