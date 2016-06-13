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

        DataContainer<PackageInterface> TcpPackages { get; set; }
        DataContainer<DataWrapper<byte[]>> UdpData { get; set; }

        PackageAdapter Adapter { get; set; }

        IPEndPoint RemoteEndPoint { get { return TcpConnection.GetEndPoint; } }

        public bool Connected { get { return TcpConnection.Connected; } }

        public NetworkConnection(TCPConnection tcpConnection)
        {
            Adapter = new PackageAdapter();

            TcpPackages = new SafeStack<PackageInterface>();

            ClientSession = new Session(-1);

            TcpConnection = tcpConnection;
            TcpConnection.DataReceivedEvent += ReceiveTCP;
            TcpConnection.InitializeReceiving();
        }

        public void SetUDPConnection(UDPConnection udpConnection)
        {
            UdpData = new SingleBuffer<DataWrapper<byte[]>>();

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
            return TcpPackages.Read();
        }

        public PackageInterface ReadUDP()
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            DataWrapper<byte[]> dataWrapper = dataWrapper = UdpData.Read();
            if (dataWrapper != null && !dataWrapper.Read)
                return Adapter.CreatePackageFromNetworkData(dataWrapper.Data);

            return null;
        }

        private void ReceiveUDP(byte[] data, IPEndPoint source)
        {
            if (RemoteEndPoint.Port == source.Port)
                UdpData.Write(new DataWrapper<byte[]>(data));
        }

        private void ReceiveTCP(byte[] data)
        {
            PackageInterface[] packages = Adapter.CreatePackagesFromStream(data);
            foreach (PackageInterface package in packages)
            {
                TcpPackages.Write(package);
            }
        }

        void IDisposable.Dispose()
        {
            CloseConnection();
        }
    }
}
