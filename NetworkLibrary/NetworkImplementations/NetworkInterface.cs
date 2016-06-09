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
        List<TCPConnection> TcpConnections { get; set; }
        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }

        protected PackageAdapter NetworkPackageAdapter { get; private set; }

        public int TCPConnected
        {
            get
            {
                int connectedCount = 0;
                foreach (TCPConnection tcpCon in TcpConnections)
                {
                    if (tcpCon.Connected)
                        connectedCount++;
                }

                return connectedCount;
            }
        }

        private bool CanSend()
        {
            return TCPConnected > 0;
        }

        protected NetworkInterface(int udpListeningPort, LogWriter logger)
        {
            Logger = logger;

            NetworkPackageAdapter = InitializeAdapter();

            TcpConnections = new List<TCPConnection>();

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, udpListeningPort);
            UdpConnection = new UDPConnection(localEndPoint);
            UdpConnection.Logger = Logger;
            UdpConnection.InitializeConnection();
        }

        public bool AddTCPConnection(Socket connectionSocket)
        {
            try
            {
                TCPConnection TcpConnection = new TCPConnection(connectionSocket);
                TcpConnection.Logger = Logger;
                TcpConnection.InitializeConnection();

                if(TcpConnection.Connected)
                {
                    TcpConnections.Add(TcpConnection);
                    UdpConnection.AddEndpoint(connectionSocket.RemoteEndPoint as IPEndPoint);
                    return true;
                }

                Log("Could not add connection!\nNot connected after initialiazation.");
            }
            catch (ConnectionException ex)
            {
                Log("Could not add connection!\nException message: " + ex.Message);
            }

            return false;
        }

        protected abstract PackageAdapter InitializeAdapter();

        public void Disconnect()
        {
            foreach(TCPConnection tcpCon in TcpConnections)
                tcpCon.Disconnect();

            UdpConnection.Disconnect();
        }

        protected PackageInterface GetDataTCP(int session)
        {
            byte[] data = TcpConnections[session].Receive();
            return NetworkPackageAdapter.CreatePackageFromNetworkData(data);
        }

        protected PackageInterface[] GetAllDataTCP()
        {
            PackageInterface[] packages = new PackageInterface[TcpConnections.Count];
            
            for(int session = 0; session < TcpConnections.Count; session++)
            {
                packages[0] = GetDataTCP(session);
                session++;
            }

            return packages;
        }

        protected PackageInterface GetDataUDP()
        {
            byte[] data = UdpConnection.Receive();
            return NetworkPackageAdapter.CreatePackageFromNetworkData(data);
        }

        protected void SendDataTCP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            TcpConnections[session].Send(data);
        }

        protected void SendDataUDP(PackageInterface package, int session)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            UdpConnection.Send(data, session);
        }

        protected void BroadCastTCP(PackageInterface package)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            foreach(TCPConnection tcpCon in TcpConnections)
            {
                tcpCon.Send(data);
            }
        }

        protected void BroadCastUDP(PackageInterface package)
        {
            if (!CanSend())
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            UdpConnection.Broadcast(data);
        }

        protected void Log(string text)
        {
            if (Logger == null)
                return;

            Logger.Log(text);
        }
    }
}