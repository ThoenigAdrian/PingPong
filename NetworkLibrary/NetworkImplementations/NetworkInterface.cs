using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {

        public delegate void SessionDeathHandler(NetworkInterface sender, int sessionID);
        public event SessionDeathHandler SessionDied;

        List<NetworkConnection> ClientConnections { get; set; }

        Semaphore m_listLock;

        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }

        protected int[] GetSessionIDs
        {
            get
            {
                List<int> sessionIDs = new List<int>(ClientConnections.Count);
                foreach (NetworkConnection clientCon in ClientConnections)
                {
                    sessionIDs.Add(clientCon.ClientSession.SessionID);
                }

                return sessionIDs.ToArray();
            }
        }

        public int ClientCount { get { return ClientConnections.Count; } }

        private bool CanSend()
        {
            return ClientCount > 0;
        }

        protected NetworkInterface(UDPConnection udpConnection, LogWriter logger)
        {
            Logger = logger;
            m_listLock = new Semaphore(1, 1);

            ClientConnections = new List<NetworkConnection>();
            UdpConnection = udpConnection;
            UdpConnection.ReceiveErrorEvent += HandleUDPReceiveError;
            UdpConnection.InitializeReceiving();
        }

        public void AddClientConnection(NetworkConnection clientConnection)
        {
            int sessionID = clientConnection.ClientSession.SessionID;
            if (GetConnection(sessionID) != null)
                throw new ConnectionException("Connection with this session ID is already in the network!");

            clientConnection.ConnectionDiedEvent += ConnectionDiedHandler;
            clientConnection.SetUDPConnection(UdpConnection);
            clientConnection.ClientSession.SessionID = sessionID;

            m_listLock.WaitOne();
            ClientConnections.Add(clientConnection);
            m_listLock.Release();
        }

        void ConnectionDiedHandler(NetworkConnection sender)
        {
            sender.ConnectionDiedEvent -= ConnectionDiedHandler;
            RaiseDeadSessionEvent(sender);
        }

        public void UpdateConnections()
        {
            List<NetworkConnection> deadCons = new List<NetworkConnection>();

            m_listLock.WaitOne();
            foreach (NetworkConnection clientCon in ClientConnections)
            {
                if (!clientCon.Connected)
                {
                    deadCons.Add(clientCon);
                }
            }
            m_listLock.Release();

            foreach (NetworkConnection deadCon in deadCons)
            {
                RaiseDeadSessionEvent(deadCon);
            }
        }

        private void HandleUDPReceiveError(ConnectionInterface sender, IPEndPoint receiveEndPoint)
        {
            NetworkConnection deadConnection = null;

            foreach (NetworkConnection clientCon in ClientConnections)
            {
                if (clientCon.ISConnectedTo(receiveEndPoint.Port))
                {
                    deadConnection = clientCon;
                    break;
                }
            }

            RaiseDeadSessionEvent(deadConnection);
        }

        private void RaiseDeadSessionEvent(NetworkConnection connection)
        {
            m_listLock.WaitOne();
            ClientConnections.Remove(connection);
            m_listLock.Release();

            if (SessionDied != null)
                SessionDied.Invoke(this, connection.ClientSession.SessionID);
        }

        public void Disconnect()
        {
            m_listLock.WaitOne();
            UdpConnection.ReceiveErrorEvent -= HandleUDPReceiveError;
            UdpConnection.Disconnect();

            foreach (NetworkConnection clientCon in ClientConnections)
            {
                clientCon.ConnectionDiedEvent -= RaiseDeadSessionEvent;
                clientCon.CloseConnection();
            }

            m_listLock.Release();
        }

        private NetworkConnection GetConnection(int session)
        {
            m_listLock.WaitOne();

            try
            {
                foreach (NetworkConnection clientCon in ClientConnections)
                {
                    if (clientCon.ClientSession.SessionID == session)
                        return clientCon;
                }
            }
            finally
            {
                m_listLock.Release();
            }

            return null;
        }

        protected PackageInterface GetDataTCP(int session)
        {
            NetworkConnection sessionConnection = GetConnection(session);

            if (sessionConnection != null)
                return sessionConnection.ReadTCP();

            return null;
        }

        protected PackageInterface[] GetAllDataTCP(int session)
        {
            List<PackageInterface> packages = new List<PackageInterface>();

            PackageInterface package;
            while ((package = GetDataTCP(session)) != null)
            {
                packages.Add(package);
            }

            if(packages.Count > 0)
                return packages.ToArray();

            return null;
        }

        protected PackageInterface GetDataUDP(int session)
        {
            NetworkConnection sessionConnection = GetConnection(session);

            if (sessionConnection != null)
                return sessionConnection.ReadUDP();

            return null;
        }

        protected void SendDataTCP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            NetworkConnection sessionConnection = GetConnection(session);

            if (sessionConnection != null)
                sessionConnection.SendTCP(package);
        }

        protected void SendDataUDP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            NetworkConnection sessionConnection = GetConnection(session);

            if (sessionConnection != null)
                sessionConnection.SendUDP(package);
        }

        protected void BroadCastTCP(PackageInterface package)
        {
            if (!CanSend())
                return;

            List<NetworkConnection> clients = ClientConnections;
            foreach (NetworkConnection clientCon in clients)
            {
                clientCon.SendTCP(package);
            }
        }

        protected void BroadCastUDP(PackageInterface package)
        {
            if (!CanSend())
                return;

            List<NetworkConnection> clients = ClientConnections;
            foreach (NetworkConnection clientCon in clients)
            {
                clientCon.SendUDP(package);
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