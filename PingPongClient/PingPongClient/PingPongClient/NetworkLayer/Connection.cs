using NetworkLibrary.PackageAdapters;

namespace PingPongClient.NetworkLayer
{
    class Connection
    {
        NetworkTCP TCPNetwork;
        NetworkUDP UDPNetwork;

        PackageAdapterInterface UDPInAdapter = new ClientControlAdapter();
        PackageAdapterInterface UDPOutAdapter;
        PackageAdapterInterface TCPInAdapter;
        PackageAdapterInterface TCPOutAdapter;

        public ServerDataPackage GetServerDataUDP()
        {
            byte[] data = TCPNetwork.Receive();
            return (TCPInAdapter.ByteToPackage(data) as ServerDataPackage);
        }

        public ServerDataPackage GetServerDataUDP()
        {
            byte[] data = UDPNetwork.Receive();
            return (UDPInAdapter.ByteToPackage(data) as ServerDataPackage);
        }

        public void SendClientDataTCP(ClientControlPackage package)
        {
            byte[] data = TCPOutAdapter.PackageToByte(package);
            TCPNetwork.Send(data);
        }

        public void SendClientDataUDP(ClientControlPackage package)
        {
            byte[] data = UDPOutAdapter.PackageToByte(package);
            UDPNetwork.Send(data);
        }
    }
}