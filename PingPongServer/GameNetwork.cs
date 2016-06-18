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
        public GameNetwork(UDPConnection UDPGameData) : this(UDPGameData, null) { }

        public GameNetwork(UDPConnection UDPGameData, LogWriter Logger) : base (UDPGameData, Logger)
        {
            
        }
                        
        public void receiveUDPTest()
        {
            if(GetDataFromEverySessionUDP()!=null)
            {
                Console.Write("empfangen");
            }
            
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

        private PackageInterface[] GetAllOfPackagesOfClient(int sessionID)
        {
            return GetAllDataTCP(sessionID);
        }

        public void Close()
        {
            // hello my friend, it's 4:00AM right now so don't mind any grammatical errors.
            // Disconnect is essentially the cleanup for the network and is also threadsafe. 
            // well, at least that's what i believe for now. :P
            Disconnect();
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
