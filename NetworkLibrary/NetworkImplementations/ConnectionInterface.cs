using NetworkLibrary.DataStructs;
using NetworkLibrary.PackageAdapters;
using System.Net;

namespace NetworkLibrary.NetworkImplementations
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

        protected PackageInterface GetServerDataUDP()
        {
            if (TCPInAdapter == null)
                return null;

            byte[] data = TCPNetwork.Receive();
            return TCPInAdapter.ByteToPackage(data);
        }

        protected PackageInterface GetServerDataUDP()
        {
            if (UDPInAdapter == null)
                return null;

            byte[] data = UDPNetwork.Receive();
            return UDPInAdapter.ByteToPackage(data);
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