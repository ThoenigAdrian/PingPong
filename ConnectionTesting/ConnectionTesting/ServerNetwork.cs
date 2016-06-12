using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;

namespace ConnectionTesting
{
    class ServerNetwork : NetworkInterface 
    {
        public ServerNetwork(UDPConnection udpConnection) : base(udpConnection, null)
        {
            
        }

        public void SendPositionData(ServerDataPackage package)
        {
            BroadCastUDP(package);
        }

        public PackageInterface[] ReceivePackageTCP(int session)
        {
            return GetAllDataTCP(session);
        }

        public PackageInterface ReceivePackageUDP(int session)
        {
            return GetDataUDP(session);
        }
    }
}
