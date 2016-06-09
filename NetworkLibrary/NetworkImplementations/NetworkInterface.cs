using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.DataStructs;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract class NetworkInterface
    {
        TCPConnection TcpConnection { get; set; }
        UDPConnection UdpConnection { get; set; }

        protected PackageAdapter NetworkPackageAdapter { get; private set; }

        protected NetworkInterface(TCPConnection tcpConnection, UDPConnection udpConnection , LogWriter logger)
        {
            TcpConnection = tcpConnection;
            TcpConnection.Logger = logger;

            UdpConnection = udpConnection;
            UdpConnection.Logger = logger;
        }

        /// <summary>
        /// Initializes Network so data can be sent.
        /// </summary>
        public void Initialize()
        {
            NetworkPackageAdapter = InitializeAdapter();

            TcpConnection.Initialize();
            UdpConnection.Initialize();
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
    }
}