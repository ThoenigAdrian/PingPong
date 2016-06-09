using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Packages;
using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {
        TCPConnection TcpConnection { get; set; }
        UDPConnection UdpConnection { get; set; }

        LogWriter Logger { get; set; }

        protected PackageAdapter NetworkPackageAdapter { get; private set; }

        public bool Connected { get { return TcpConnection.Connected && UdpConnection.Connected; } }

        protected NetworkInterface(Socket connectedTCPSocket, LogWriter logger)
        {
            Logger = logger;

            TcpConnection = new TCPConnection(connectedTCPSocket);
            TcpConnection.Logger = Logger;
            TcpConnection.Initialize();

            UdpConnection = new UDPConnection(connectedTCPSocket.RemoteEndPoint as IPEndPoint, connectedTCPSocket.LocalEndPoint as IPEndPoint);
            UdpConnection.Logger = Logger;
            UdpConnection.Initialize();

            NetworkPackageAdapter = InitializeAdapter();
        }

        protected abstract PackageAdapter InitializeAdapter();

        public void Disconnect()
        {
            TcpConnection.Disconnect();
            UdpConnection.Disconnect();
        }

        protected PackageInterface GetDataTCP()
        {
            if (NetworkPackageAdapter == null)
                return null;

            byte[] data = TcpConnection.Receive();
            return NetworkPackageAdapter.CreatePackageFromNetworkData(data);
        }

        protected PackageInterface GetDataUDP()
        {
            if (NetworkPackageAdapter == null)
                return null;

            byte[] data = UdpConnection.Receive();
            return NetworkPackageAdapter.CreatePackageFromNetworkData(data);
        }

        protected void SendDataTCP(PackageInterface package)
        {
            if (NetworkPackageAdapter == null)
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            TcpConnection.Send(data);
        }

        protected void SendDataUDP(PackageInterface package)
        {
            if (NetworkPackageAdapter == null)
                return;

            byte[] data = NetworkPackageAdapter.CreateNetworkDataFromPackage(package);
            UdpConnection.Send(data);
        }

        protected void Log(string text)
        {
            if (Logger == null)
                return;

            Logger.Log(text);
        }
    }
}