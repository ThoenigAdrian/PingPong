using NetworkLibrary.DataPackages;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using System;
using System.Net;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class NetworkConnection : IDisposable
    {
        public Session ClientSession { get; set; }

        TCPConnection TcpConnection { get; set; }
        UDPConnection UdpConnection { get; set; }

        DataContainer<byte[]> TcpData { get; set; }
        DataContainer<byte[]> UdpData { get; set; }

        PackageAdapter Adapter { get; set; }

        IPEndPoint RemoteEndPoint { get { return TcpConnection.GetEndPoint; } }

        public bool Connected { get { return TcpConnection.Connected; } }

        public NetworkConnection(TCPConnection tcpConnection)
        {
            Adapter = new PackageAdapter();

            TcpData = new SafeStack<byte[]>();

            ClientSession = new Session(-1);

            TcpConnection = tcpConnection;
            TcpConnection.DataReceivedEvent += ReceiveTCP;
            TcpConnection.InitializeReceiving();
        }

        public void SetUDPConnection(UDPConnection udpConnection)
        {
            UdpData = new DoubleBuffer<byte[]>();

            UdpConnection = udpConnection;
            UdpConnection.DataReceivedEvent += ReceiveUDP;
        }

        public void CloseConnection()
        {
            TcpConnection.DataReceivedEvent -= ReceiveTCP;
            UdpConnection.DataReceivedEvent -= ReceiveUDP;

            TcpConnection.Disconnect();
        }

        public void SendTCP(PackageInterface package)
        {
            TcpConnection.Send(Adapter.CreateNetworkDataFromPackage(package));
        }

        public void SendUDP(PackageInterface package)
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            UdpConnection.Send(Adapter.CreateNetworkDataFromPackage(package), RemoteEndPoint);
        }

        public PackageInterface ReadTCP()
        {
            return Adapter.CreatePackageFromNetworkData(TcpData.Read());
        }

        public PackageInterface ReadUDP()
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            return Adapter.CreatePackageFromNetworkData(UdpData.Read());
        }

        private void ReceiveUDP(byte[] data, IPEndPoint source)
        {
            if (RemoteEndPoint.Port == source.Port)
                UdpData.Write(data);
        }

        private void ReceiveTCP(byte[] data)
        {
            TcpData.Write(data);
        }

        void IDisposable.Dispose()
        {
            CloseConnection();
        }
    }
}
