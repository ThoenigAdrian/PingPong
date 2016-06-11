using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Generic;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {
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
        }

        public bool AddClientConnection(NetworkConnection clientConnection)
        {
            if (clientConnection.Connected)
            {
                clientConnection.SetUDPConnection(UdpConnection);
                ClientConnections.Add(clientConnection);
                return true;
            }

            Log("Could not add connection!\nNot connected!");

            return false;
        }

        public void Disconnect()
        {
            foreach (NetworkConnection clientCon in ClientConnections)
                clientCon.CloseConnection();

            UdpConnection.Disconnect();
        }

        protected PackageInterface GetDataTCP(int session)
        {
            return ClientConnections[session].ReadTCP();
        }

        protected PackageInterface[] GetAllDataTCP(int session)
        {
            List<PackageInterface> packages = new List<PackageInterface>();

            PackageInterface package = GetDataTCP(session);
            while (package != null)
            {
                packages.Add(package);
                package = GetDataTCP(session);
            }

            return packages.ToArray();
        }

        protected PackageInterface GetDataUDP(int session)
        {
            return ClientConnections[session].ReadUDP();
        }

        protected void SendDataTCP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            ClientConnections[session].SendTCP(package);
        }

        protected void SendDataUDP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            ClientConnections[session].SendUDP(package);
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