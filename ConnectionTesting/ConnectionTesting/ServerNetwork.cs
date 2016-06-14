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

        public Dictionary<int, PackageInterface[]> GetAllSessionDataTCP()
        {
            Dictionary<int, PackageInterface[]> packages = new Dictionary<int, PackageInterface[]>();

            foreach (int session in GetSessionIDs)
            {
                PackageInterface[] sessionPackages = GetAllDataTCP(session);
                if(sessionPackages != null)
                    packages.Add(session, sessionPackages);
            }

            if (packages.Count > 0)
                return packages;

            return null;
        }

        public Dictionary<int, PackageInterface> GetAllSessionDataUDP()
        {
            Dictionary<int, PackageInterface> packages = new Dictionary<int, PackageInterface>();

            foreach (int session in GetSessionIDs)
            {
                PackageInterface sessionPackage = GetDataUDP(session);
                if(sessionPackage != null)
                    packages.Add(session, sessionPackage);
            }

            if (packages.Count > 0)
                return packages;

            return null;
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
