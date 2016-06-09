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

        protected PackageAdapter m_packageAdapter;

        protected NetworkInterface(TCPConnection tcpConnection, UDPConnection udpConnection , LogWriter logger)
        {
            TcpConnection = tcpConnection;
            TcpConnection.Logger = logger;

            UdpConnection = udpConnection;
            UdpConnection.Logger = logger;

            TcpConnection.Initialize();
            UdpConnection.Initialize();
        }

        public void Disconnect()
        {
            TcpConnection.Disconnect();
            UdpConnection.Disconnect();
        }

        protected PackageInterface GetDataTCP()
        {
            if (m_packageAdapter == null)
                return null;

            byte[] data = TcpConnection.Receive();
            return m_packageAdapter.CreatePackageFromNetworkData(data);
        }

        protected PackageInterface GetDataUDP()
        {
            if (m_packageAdapter == null)
                return null;

            byte[] data = UdpConnection.Receive();
            return m_packageAdapter.CreatePackageFromNetworkData(data);
        }

        protected void SendDataTCP(PackageInterface package)
        {
            if (m_packageAdapter == null)
                return;

            byte[] data = m_packageAdapter.CreateNetworkDataFromPackage(package);
            TcpConnection.Send(data);
        }

        protected void SendDataUDP(PackageInterface package)
        {
            if (m_packageAdapter == null)
                return;

            byte[] data = m_packageAdapter.CreateNetworkDataFromPackage(package);
            UdpConnection.Send(data);
        }
    }
}