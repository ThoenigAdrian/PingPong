using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Generic;
using System.Threading;
using NetworkLibrary.NetworkImplementations.Network;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract partial class NetworkInterface
    {
        public delegate void SessionDeathHandler(NetworkInterface sender, int sessionID);
        public event SessionDeathHandler SessionDied;

        public int[] GetSessionIDs { get { return ClientConnections.GetSessionIDs; } }

        public int ClientCount { get { return ClientConnections.Count; } }

        protected int KeepAliveInterval { get; set; } // Milliseconds
        volatile bool m_keepAlive;

        protected int HolePunchingInterval { get; set; } // Milliseconds
        volatile bool m_holePunching;

        protected NetworkInput In { get; private set; }
        protected NetworkOutput Out { get; private set; }

        NetworkErrorHandling ErrorHandling { get; set; }

        NetworkConnectionPool ClientConnections { get; set; }
        UDPConnection UdpConnection { get; set; }

        Logger Logger { get; set; }

        protected NetworkInterface(UDPConnection udpConnection, Logger logger)
        {
            Logger = logger;

            ClientConnections = new NetworkConnectionPool();
            In = new NetworkInput(ClientConnections);
            Out = new NetworkOutput(ClientConnections);

            ErrorHandling = new NetworkErrorHandling(ClientConnections, this);

            UdpConnection = udpConnection;
            UdpConnection.Logger = logger;
            UdpConnection.ReceiveErrorEvent += ErrorHandling.HandleUDPReceiveError;
            UdpConnection.InitializeReceiving();

            KeepAliveInterval = 1000;
            m_keepAlive = true;

            HolePunchingInterval = 1000;
            m_holePunching = false;

            Thread keepAliveThread = new Thread(KeepAliveLoop);
            keepAliveThread.Name = "Keep alive";
            keepAliveThread.Start();
        }

        public void AddClientConnection(NetworkConnection clientConnection)
        {
            if (clientConnection.ClientSession == null)
                throw new ConnectionException("Connection does not have an ID!");

            int sessionID = clientConnection.ClientSession.SessionID;
            if (ClientConnections[sessionID] != null)
                throw new ConnectionException("Connection with this session ID is already in the network!");

            clientConnection.ConnectionDiedEvent += ErrorHandling.ConnectionDiedHandler;
            clientConnection.SetUDPConnection(UdpConnection);
            clientConnection.ClientSession.SessionID = sessionID;

            if (!ClientConnections.TryAdd(sessionID, clientConnection))
                throw new ConnectionException("Adding the connection failed!");
        }

        public void RemoveClientConnection(int SessionID)
        {
            ClientConnections.TryRemove(SessionID, out _);

            // CaptainOachkatzl how to do this properly ?? Event unsubscribe ? 
            //clientConnection.ConnectionDiedEvent += ErrorHandling.ConnectionDiedHandler;
            //clientConnection.SetUDPConnection(UdpConnection);
            //clientConnection.ClientSession.SessionID = sessionID;
        }

        public bool ClientStillConnected(int sessionID)
        {
            bool clientConnected = false;
            foreach(NetworkConnection clientCon in ClientConnections.Values)
            {
                if(clientCon.ClientSession.SessionID == sessionID)
                {
                    clientConnected = clientCon.Connected;
                }
            }
            return clientConnected;
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

        public void Disconnect()
        {
            m_keepAlive = false;
            m_holePunching = false;

            foreach (NetworkConnection clientCon in ClientConnections.Values)
            {
                clientCon.ConnectionDiedEvent -= ErrorHandling.ConnectionDiedHandler;
                clientCon.CloseConnection();
            }

            UdpConnection.ReceiveErrorEvent -= ErrorHandling.HandleUDPReceiveError;
            PostDisconnectActions();
        }

        public void StartHolePunching()
        {
            if (m_holePunching)
                return;

            m_holePunching = true;

            Thread holePunchingThread = new Thread(HolePunchingLoop);
            holePunchingThread.Name = "Hole punching";
            holePunchingThread.Start();
        }

        protected virtual void PostDisconnectActions()
        {
            // Add what you wanna do after a disconnect in the override of this function
        }

        protected void TerminateUDPConnection()
        {
            UdpConnection.ReceiveErrorEvent -= ErrorHandling.HandleUDPReceiveError;
            UdpConnection.Disconnect();
        }

        protected void IssueResponse(ResponseRequest responseRequest, int session)
        {
            NetworkConnection connection = ClientConnections[session];
            if (connection != null)
                connection.IssueResponse(responseRequest);
        }

        private void KeepAliveLoop()
        {
            while (m_keepAlive)
            {
                Out.BroadCastKeepAlive();
                Thread.Sleep(KeepAliveInterval);
            }
        }

        private void HolePunchingLoop()
        {
            while (m_holePunching)
            {
                Out.BroadCastHolePunching();
                Thread.Sleep(HolePunchingInterval);
            }
        }

        protected void Log(string text)
        {
            Logger.Log(text);
        }
    }
}