using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {
        List<NetworkConnection> ClientConnections { get; set; }
        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }

        protected PackageAdapter NetworkPackageAdapter { get; private set; }

        public int ClientCount { get { return ClientConnections.Count; } }

        private bool CanSend()
        {
            return ClientCount > 0;
        }

        protected NetworkInterface(UDPConnection udpConnection, LogWriter logger)
        {
            Logger = logger;

            NetworkPackageAdapter = InitializeAdapter();

            ClientConnections = new List<NetworkConnection>();

            UdpConnection = udpConnection;
            UdpConnection.InitializeConnection();
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

        protected abstract PackageAdapter InitializeAdapter();

        public void Disconnect()
        {
            foreach (NetworkConnection clientCon in ClientConnections)
                clientCon.CloseConnection();

            UdpConnection.Disconnect();
        }

        protected List<PackageInterface> GetAllPackagesOfTCPSession(int session)
        {
            List <PackageInterface> allPackages = new List<PackageInterface>();
            byte[] data = ClientConnections[session].ReadTCP();
            while(data != null)
            {
                allPackages.Add(NetworkPackageAdapter.CreatePackageFromNetworkData(data));
                data = ClientConnections[session].ReadTCP();                                   
            }
            return allPackages;

        }


        protected PackageInterface GetDataTCP(int session)
        {
            byte[] data = ClientConnections[session].ReadTCP();
            return NetworkPackageAdapter.CreatePackageFromNetworkData(data);
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
            byte[] data = ClientConnections[session].ReadUDP();
            return NetworkPackageAdapter.CreatePackageFromNetworkData(data);
        }

        protected void SendDataTCP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            ClientConnections[session].SendTCP(data);
        }

        protected void SendDataUDP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            ClientConnections[session].SendUDP(data);
        }

        protected void BroadCastTCP(PackageInterface package)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            foreach(NetworkConnection clientCon in ClientConnections)
            {
                clientCon.SendTCP(data);
            }
        }

        protected void BroadCastUDP(PackageInterface package)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            foreach (NetworkConnection clientCon in ClientConnections)
            {
                clientCon.SendUDP(data);
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