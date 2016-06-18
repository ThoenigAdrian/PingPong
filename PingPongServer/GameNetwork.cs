﻿using System;
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
        public List<int> DiedSessions = new List<int>();

        public GameNetwork(UDPConnection UDPGameData, LogWriter Logger) : base (UDPGameData, Logger)
        {
            SessionDied += SessionDiedHandler;
        }

        private void SessionDiedHandler(NetworkInterface sender, int sessionID)
        {
            DiedSessions.Add(sessionID);
        }

        public void receiveUDPTest()
        {
            if(In.GetDataFromEverySessionUDP()!=null)
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
            return In.GetDataFromEverySessionTCP();            
        }

        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            Out.BroadCastUDP(Frame);
        }

        private PackageInterface[] GetAllOfPackagesOfClient(int sessionID)
        {
            return In.GetAllDataTCP(sessionID);
        }

        public void Close()
        {
            Disconnect();
        }
                
        public void BroadcastGenericPackage(PackageInterface package, SocketType type)
        {
            if (type == SocketType.Dgram)
                Out.BroadCastTCP(package);
            if (type == SocketType.Stream)
                Out.BroadCastUDP(package);
        }

    }
}
