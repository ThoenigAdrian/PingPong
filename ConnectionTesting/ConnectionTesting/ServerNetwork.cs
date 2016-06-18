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
            Out.BroadCastUDP(package);
        }

        public void SendPositionDataTCP(ServerDataPackage package)
        {
            Out.BroadCastTCP(package);
        }

        public PackageInterface[] ReceivePackageTCP(int session)
        {
            return In.GetAllDataTCP(session);
        }

        public PackageInterface ReceivePackageUDP(int session)
        {
            return In.GetDataUDP(session);
        }

        public Dictionary<int, PackageInterface[]> CollectDataTCP()
        {
            return In.GetDataFromEverySessionTCP();
        }

        public Dictionary<int, PackageInterface> CollectDataUDP()
        {
            return In.GetDataFromEverySessionUDP();
        }
    }
}
