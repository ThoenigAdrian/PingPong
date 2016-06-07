using NetworkLibrary.PackageAdapters;

namespace PingPongClient.NetworkLayer
{
    abstract class ConnectionInterface
    {
        NetworkTCP TCPNetwork;
        NetworkUDP UDPNetwork;

        protected PackageAdapterInterface UDPInAdapter;
        protected PackageAdapterInterface UDPOutAdapter;
        protected PackageAdapterInterface TCPInAdapter;
        protected PackageAdapterInterface TCPOutAdapter;

        protected ConnectionInterface(IPEndPoint server)
        {
            TCPNetwork = new NetworkTCP(server);
            UDPNetwork = new NetworkUDP(server);
        }

        protected T<T> GetServerDataUDP()
        {
            if (TCPInAdapter == null)
                return null;

            byte[] data = TCPNetwork.Receive();
            return (TCPInAdapter.ByteToPackage(data) as T);
        }

        protected T<T> GetServerDataUDP()
        {
            if (UDPinAdapter == null)
                return null;

            byte[] data = UDPNetwork.Receive();
            return (UDPInAdapter.ByteToPackage(data) as T);
        }

        protected void SendClientDataTCP(PackageInterface package)
        {
            if (TCPOutAdapter == null)
                return;

            byte[] data = TCPOutAdapter.PackageToByte(package);
            TCPNetwork.Send(data);
        }

        protected void SendClientDataUDP(PackageInterface package)
        {
            if (UDPOutAdapter == null)
                return;

            byte[] data = UDPOutAdapter.PackageToByte(package);
            UDPNetwork.Send(data);
        }
    }
}