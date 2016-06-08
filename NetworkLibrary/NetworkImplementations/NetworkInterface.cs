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

        protected PackageAdapterInterface UDPInAdapter;
        protected PackageAdapterInterface UDPOutAdapter;
        protected PackageAdapterInterface TCPInAdapter;
        protected PackageAdapterInterface TCPOutAdapter;


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
            if (TCPInAdapter == null)
                return null;

            byte[] data = TcpConnection.Receive();
            return TCPInAdapter.ByteToPackage(data);
        }

        protected PackageInterface GetDataUDP()
        {
            if (UDPInAdapter == null)
                return null;

            byte[] data = UdpConnection.Receive();
            return UDPInAdapter.ByteToPackage(data);
        }

        protected void SendDataTCP(PackageInterface package)
        {
            if (TCPOutAdapter == null)
                return;

            byte[] data = TCPOutAdapter.PackageToByte(package);
            TcpConnection.Send(data);
        }

        protected void SendDataUDP(PackageInterface package)
        {
            if (UDPOutAdapter == null)
                return;

            byte[] data = UDPOutAdapter.PackageToByte(package);
            UdpConnection.Send(data);
        }
    }
}