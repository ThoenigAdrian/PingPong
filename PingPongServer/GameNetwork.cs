using System;
using System.Collections.Generic;
using System.Linq;
using NetworkLibrary.Utility;
using GameLogicLibrary;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Net.Sockets;

namespace PingPongServer
{
    
    public class GameNetwork : NetworkInterface
    {
        
        public GameNetwork(UDPConnection UDPGameData,  NetworkConnection Host) : this(UDPGameData, Host, null) { }

        public GameNetwork(UDPConnection UDPGameData, NetworkConnection Host, LogWriter Logger) : base (UDPGameData, Logger)
        {
            AddClient(Host);
        }

        public void AddClient(NetworkConnection connection)
        {
            AddClientConnection(connection); // from Inherited Class
        }
                
        public Dictionary<int, PackageInterface[]> GrabAllNetworkDataForNextFrame()
        {
            Dictionary<int, PackageInterface[]> packagesOfAllClients = new Dictionary<int, PackageInterface[]>();
            foreach (int sessionID in GetSessionIDs)
            {
                packagesOfAllClients[sessionID] = GetAllOfPackagesOfClient(sessionID);
            }
            return packagesOfAllClients;
        }

        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            BroadCastUDP(Frame);
        }

        public PackageInterface[] GetAllOfPackagesOfClient(int sessionID)
        {
            return GetAllDataTCP(sessionID);
        }
                
        public void BroadcastGenericPackage(PackageInterface package, SocketType type)
        {
            if (type == SocketType.Dgram)
                BroadCastTCP(package);
            if (type == SocketType.Stream)
                BroadCastUDP(package);
        }

    }
}
