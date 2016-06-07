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

        protected T GetServerDataUDP<T>() where T : class
        {
            if (TCPInAdapter == null)
                return default(T);

            byte[] data = TCPNetwork.Receive();
            return (TCPInAdapter.ByteToPackage(data) as T);
        }

        protected T GetServerDataUDP<T>() where T : class
        {
            if (UDPInAdapter == null)
                return default(T);

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