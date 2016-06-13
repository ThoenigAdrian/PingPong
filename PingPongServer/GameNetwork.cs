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
        // This Object is state dependent call GrabAllNetworkDataForNextFrame() , then work with the the methods (e.g GetLastPlayerMovement)
        List<PackageInterface[]> packagesOfAllClients = new List<PackageInterface[]>();

        public GameNetwork(UDPConnection UDPGameData,  NetworkConnection Host) : this(UDPGameData, Host, null) { }

        public GameNetwork(UDPConnection UDPGameData, NetworkConnection Host, LogWriter Logger) : base (UDPGameData, Logger)
        {
            AddClient(Host);
            UpdateClientConnections();
        }

        public void AddClient(NetworkConnection connection)
        {
            AddClientConnection(connection); // from Inherited Class
            UpdateClientConnections(); // so we can use this new connection for GrabAllDataForTheNExtFram
        }

        
        private void UpdateClientConnections()
        {
            foreach (int sessionID in GetSessionIDs)
            {
                packagesOfAllClients.Add(GetAllDataTCP(sessionID));
            }
        }

        public void GrabAllNetworkDataForNextFrame()
        {
            foreach(int sessionID in GetSessionIDs)
            {
                packagesOfAllClients[sessionID] = GetAllDataTCP(sessionID);
            }
        }

        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            BroadCastUDP(Frame);
        }

        public ClientControls GetLastPlayerControl(int ClientID, int playerID)
        {

            ClientControls lastControl = ClientControls.NoInput;
            foreach (PackageInterface package in packagesOfAllClients[ClientID])
            {
                if (package.PackageType == PackageType.ClientControl)
                    lastControl = ((ClientControlPackage)package).ControlInput;
            }

            return lastControl;
        }

        public ClientMovement GetLastPlayerMovement(int ClientID, int playerID)
        {
            PackageInterface[] allPackages = GetAllDataTCP(ClientID);
            ClientMovement lastMovement = ClientMovement.NoInput;
            foreach (PackageInterface package in allPackages)
            {
                if (package.PackageType == PackageType.ClientPlayerMovement)
                    lastMovement = ((PlayerMovementPackage)package).PlayerMovement;
            }

            return lastMovement;
        }

        public ClientAddPlayerRequest GetLastAddPlayerRequest(int ClientID)
        {
            ClientAddPlayerRequest bla = null;
            foreach(PackageInterface pack in packagesOfAllClients[ClientID])
            {
                if(pack.PackageType == PackageType.ClientAddPlayerRequest)
                    bla = (ClientAddPlayerRequest)pack;
            }
            return bla;
        }

        public PackageInterface[] GetAllPackages(int ClientID)
        {
            return GetAllDataTCP(ClientID);
        }

        public PackageInterface GetLastPackage(int ClientID)
        {
            PackageInterface[] allPackages = GetAllDataTCP(ClientID);
            return allPackages[allPackages.Length - 1];
        }

        // Maybe for future use
        public void BroadcastGenericPackage(PackageInterface package, SocketType type)
        {
            if (type == SocketType.Dgram)
                BroadCastTCP(package);
            if (type == SocketType.Stream)
                BroadCastUDP(package);
        }

    }
}
