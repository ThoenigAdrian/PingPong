using NetworkLibrary.ConnectionImplementations.NetworkImplementations;
using NetworkLibrary.DataStructs;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;

namespace NetworkLibrary.ConnectionImplementations
{
    public abstract class NetworkInterface
    {
        TCPConnection TCPNetwork;
        UDPConnection UDPNetwork;

        protected PackageAdapterInterface UDPInAdapter;
        protected PackageAdapterInterface UDPOutAdapter;
        protected PackageAdapterInterface TCPInAdapter;
        protected PackageAdapterInterface TCPOutAdapter;

        protected NetworkInterface(TCPConnection tcpConnection, UDPConnection udpConnection, LogWriter logger)
        {
            TCPNetwork = tcpConnection;
            TCPNetwork.Logger = logger;

            UDPNetwork = udpConnection;
            UDPNetwork.Logger = logger;

            TCPNetwork.Initialize();
            UDPNetwork.Initialize();
        }

        public void Disconnect()
        {
            TCPNetwork.Disconnect();
            UDPNetwork.Disconnect();
        }

        protected PackageInterface GetServerDataTCP()
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