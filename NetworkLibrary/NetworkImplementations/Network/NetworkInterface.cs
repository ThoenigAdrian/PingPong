using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Generic;
using System.Threading;
using NetworkLibrary.NetworkImplementations.Network;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract partial class NetworkInterface
    {
        public delegate void SessionDeathHandler(NetworkInterface sender, int sessionID);
        public event SessionDeathHandler SessionDied;

        public int ClientCount { get { return ClientConnections.Count; } }

        protected int KeepAliveInterval { get; set; } // Milliseconds
        volatile bool m_keepAlive;

        protected NetworkInput In { get; private set; }
        protected NetworkOutput Out { get; private set; }

        NetworkErrorHandling ErrorHandling { get; set; }

        NetworkConnectionPool ClientConnections { get; set; }
        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }


        protected NetworkInterface(UDPConnection udpConnection, LogWriter logger)
        {
            Logger = logger;

            ClientConnections = new NetworkConnectionPool();
            In = new NetworkInput(ClientConnections);
            Out = new NetworkOutput(ClientConnections);

            ErrorHandling = new NetworkErrorHandling(ClientConnections, this);

            UdpConnection = udpConnection;
            UdpConnection.ReceiveErrorEvent += ErrorHandling.HandleUDPReceiveError;
            UdpConnection.InitializeReceiving();

            KeepAliveInterval = 1000;
            m_keepAlive = true;

            new Thread(KeepAliveLoop).Start();
        }

        public void AddClientConnection(NetworkConnection clientConnection)
        {
            int sessionID = clientConnection.ClientSession.SessionID;
            if (ClientConnections[sessionID] != null)
                throw new ConnectionException("Connection with this session ID is already in the network!");

            clientConnection.ConnectionDiedEvent += ErrorHandling.ConnectionDiedHandler;
            clientConnection.SetUDPConnection(UdpConnection);
            clientConnection.ClientSession.SessionID = sessionID;

            if (!ClientConnections.TryAdd(sessionID, clientConnection))
                throw new ConnectionException("Adding the connection failed!");
        }

        public void Disconnect()
        {
            m_keepAlive = false;

            foreach (NetworkConnection clientCon in ClientConnections.Values)
            {
                clientCon.ConnectionDiedEvent -= ErrorHandling.ConnectionDiedHandler;
                clientCon.CloseConnection();
            }

            UdpConnection.ReceiveErrorEvent -= ErrorHandling.HandleUDPReceiveError;
            UdpConnection.Disconnect();
        }

        private void KeepAliveLoop()
        {
            while (m_keepAlive)
            {
                Out.BroadCastKeepAlive();
                Thread.Sleep(KeepAliveInterval);
            }
        }

        protected void IssueResponse(ResponseRequest responseRequest, int session)
        {
            NetworkConnection connection = ClientConnections[session];
            if (connection != null)
                connection.IssueResponse(responseRequest);
        }

        public void UpdateConnections()
        {
            List<NetworkConnection> deadCons = new List<NetworkConnection>();

            foreach (NetworkConnection clientCon in ClientConnections.Values)
            {
                if (!clientCon.Connected)
                    deadCons.Add(clientCon);
            }

            foreach (NetworkConnection deadCon in deadCons)
            {
                deadCon.CloseConnection();
            }
        }

        protected void Log(string text)
        {
            if (Logger == null)
                return;

            Logger.Log(text);
        }
    }
}