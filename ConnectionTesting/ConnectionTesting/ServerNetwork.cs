using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Generic;

namespace ConnectionTesting
{
    class ServerNetwork : NetworkInterface 
    {
        public ServerNetwork(UDPConnection udpConnection) : base(udpConnection, null)
        {
            
        }

        public void SendPositionDataUDP(ServerDataPackage package)
        {
            BroadCastUDP(package);
        }

        public void SendPositionDataTCP(ServerDataPackage package)
        {
            BroadCastTCP(package);
        }

        public PackageInterface[] ReceivePackageTCP(int session)
        {
            return GetAllDataTCP(session);
        }

        public PackageInterface ReceivePackageUDP(int session)
        {
            return GetDataUDP(session);
        }

        public Dictionary<int, PackageInterface[]> CollectDataTCP()
        {
            return GetDataFromEverySessionTCP();
        }

        public Dictionary<int, PackageInterface> CollectDataUDP()
        {
            return GetDataFromEverySessionUDP();
        }
    }
}
