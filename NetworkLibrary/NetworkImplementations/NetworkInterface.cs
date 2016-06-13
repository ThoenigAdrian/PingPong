using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Generic;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {

        public delegate void SessionDeathHandler(NetworkInterface sender, int sessionID);
        public event SessionDeathHandler SessionDied;

        List<NetworkConnection> ClientConnections { get; set; }

        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }

        public int ClientCount { get { return ClientConnections.Count; } }

        private bool CanSend()
        {
            return ClientCount > 0;
        }

        protected NetworkInterface(UDPConnection udpConnection, LogWriter logger)
        {
            Logger = logger;

            ClientConnections = new List<NetworkConnection>();
            UdpConnection = udpConnection;
            UdpConnection.InitializeReceiving();
        }

        public void AddClientConnection(NetworkConnection clientConnection)
        {
            int sessionID = clientConnection.ClientSession.SessionID;
            if (GetConnection(sessionID) != null)
                throw new ConnectionException("Connection with this session ID is already in the network!");

            if (clientConnection.Connected)
            {
                clientConnection.SetUDPConnection(UdpConnection);
                clientConnection.ClientSession.SessionID = sessionID;
                ClientConnections.Add(clientConnection);
            }

            throw new ConnectionException("Could not add client connection because it is disconnected!");
        }

        public void UpdateConnections()
        {
            List<NetworkConnection> deadConnections = new List<NetworkConnection>();

            foreach (NetworkConnection clientCon in ClientConnections)
            {
                if (!clientCon.Connected)
                {
                    clientCon.CloseConnection();
                    deadConnections.Add(clientCon);
                }
            }

            foreach (NetworkConnection deadCon in deadConnections)
            {
                ClientConnections.Remove(deadCon);

                if (SessionDied != null)
                    SessionDied.Invoke(this, deadCon.ClientSession.SessionID);

            }
        }

        public void Disconnect()
        {
            foreach (NetworkConnection clientCon in ClientConnections)
                clientCon.CloseConnection();

            UdpConnection.Disconnect();
        }

        private NetworkConnection GetConnection(int session)
        {
            foreach (NetworkConnection clientCon in ClientConnections)
            {
                if (clientCon.ClientSession.SessionID == session)
                    return clientCon;
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

            foreach(NetworkConnection clientCon in ClientConnections)
            {
                clientCon.SendTCP(package);
            }
        }

        protected void BroadCastUDP(PackageInterface package)
        {
            if (!CanSend())
                return;

            foreach (NetworkConnection clientCon in ClientConnections)
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