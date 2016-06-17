using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {

        public delegate void SessionDeathHandler(NetworkInterface sender, int sessionID);
        public event SessionDeathHandler SessionDied;

        SafeList<NetworkConnection> ClientConnections { get; set; }

        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }

        volatile bool m_keepAlive;

        protected int[] GetSessionIDs
        {
            get
            {
                List<int> sessionIDs = new List<int>(ClientConnections.Count);
                foreach (NetworkConnection clientCon in ClientConnections.Entries)
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

            ClientConnections = new SafeList<NetworkConnection>();
            UdpConnection = udpConnection;
            UdpConnection.ReceiveErrorEvent += HandleUDPReceiveError;
            UdpConnection.InitializeReceiving();
            m_keepAlive = true;

            new Thread(BroadCastKeepAlive).Start();
        }

        public void AddClientConnection(NetworkConnection clientConnection)
        {
            int sessionID = clientConnection.ClientSession.SessionID;
            if (GetConnection(sessionID) != null)
                throw new ConnectionException("Connection with this session ID is already in the network!");

            clientConnection.ConnectionDiedEvent += ConnectionDiedHandler;
            clientConnection.SetUDPConnection(UdpConnection);
            clientConnection.ClientSession.SessionID = sessionID;

            ClientConnections.Add(clientConnection);
        }

        public void CloseNetwork()
        {
            // Hello Dave this is not working as expected only the first connections get's closed. It looks like it breaks out of the for loop
            for (int index = 0; index < ClientConnections.Count; index++)
                ClientConnections[index].CloseConnection();
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

        protected Dictionary<int, PackageInterface[]> GetDataFromEverySessionTCP()
        {
            Dictionary<int, PackageInterface[]> packages = new Dictionary<int, PackageInterface[]>();

            foreach (int session in GetSessionIDs)
            {
                PackageInterface[] sessionPackages = GetAllDataTCP(session);
                if (sessionPackages != null)
                    packages.Add(session, sessionPackages);
            }

            if (packages.Count > 0)
                return packages;

            return null;
        }

        protected PackageInterface GetDataUDP(int session)
        {
            NetworkConnection sessionConnection = GetConnection(session);

            if (sessionConnection != null)
                return sessionConnection.ReadUDP();

            return null;
        }

        protected Dictionary<int, PackageInterface> GetDataFromEverySessionUDP()
        {
            Dictionary<int, PackageInterface> packages = new Dictionary<int, PackageInterface>();

            foreach (int session in GetSessionIDs)
            {
                PackageInterface sessionPackage = GetDataUDP(session);
                if (sessionPackage != null)
                    packages.Add(session, sessionPackage);
            }

            if (packages.Count > 0)
                return packages;

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

            foreach (NetworkConnection clientCon in ClientConnections.Entries)
            {
                clientCon.SendTCP(package);
            }
        }

        protected void BroadCastUDP(PackageInterface package)
        {
            if (!CanSend())
                return;

            foreach (NetworkConnection clientCon in ClientConnections.Entries)
            {
                clientCon.SendUDP(package);
            }
        }

        private void BroadCastKeepAlive()
        {
            while (m_keepAlive)
            {
                foreach (NetworkConnection clientCon in ClientConnections.Entries)
                {
                    clientCon.SendKeepAlive();
                }

                Thread.Sleep(1000);
            }
        }

        protected void IssueResponse(ResponseRequest responseRequest, int session)
        {
            NetworkConnection connection = GetConnection(session);
            if (connection != null)
                connection.IssueResponse(responseRequest);
        }

        private NetworkConnection GetConnection(int session)
        {
            foreach (NetworkConnection clientCon in ClientConnections.Entries)
            {
                if (clientCon.ClientSession.SessionID == session)
                    return clientCon;
            }

            return null;
        }

        void ConnectionDiedHandler(NetworkConnection sender)
        {
            sender.ConnectionDiedEvent -= ConnectionDiedHandler;

            RemoveConnection(sender);
            RaiseDeadSessionEvent(sender);
        }

        private void RemoveConnection(NetworkConnection connection)
        {
            ClientConnections.Remove(connection);
        }

        private void RaiseDeadSessionEvent(NetworkConnection connection)
        {
            if (SessionDied != null)
                SessionDied.Invoke(this, connection.ClientSession.SessionID);
        }

        public void UpdateConnections()
        {
            List<NetworkConnection> deadCons = new List<NetworkConnection>();

            foreach (NetworkConnection clientCon in ClientConnections.Entries)
            {
                if (!clientCon.Connected)
                    deadCons.Add(clientCon);
            }

            foreach (NetworkConnection deadCon in deadCons)
            {
                deadCon.CloseConnection();
            }
        }

        private void HandleUDPReceiveError(ConnectionInterface sender, IPEndPoint receiveEndPoint)
        {
            NetworkConnection deadConnection = null;

            foreach (NetworkConnection clientCon in ClientConnections.Entries)
            {
                if (clientCon.ISConnectedTo(receiveEndPoint.Port))
                {
                    deadConnection = clientCon;
                    break;
                }
            }

            if (deadConnection != null)
                deadConnection.CloseConnection();
        }

        public void Disconnect()
        {
            m_keepAlive = false;

            foreach (NetworkConnection clientCon in ClientConnections.Entries)
            {
                clientCon.ConnectionDiedEvent -= ConnectionDiedHandler;
                clientCon.CloseConnection();
            }

            UdpConnection.ReceiveErrorEvent -= HandleUDPReceiveError;
            UdpConnection.Disconnect();
        }

        protected void Log(string text)
        {
            if (Logger == null)
                return;

            Logger.Log(text);
        }
    }
}