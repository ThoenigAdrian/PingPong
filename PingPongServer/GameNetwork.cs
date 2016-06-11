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
        public Dictionary<int, PackageInterface []> allClients = new Dictionary<int, PackageInterface[]>();
        List<PackageInterface[]> packagesOfAllClients = new List<PackageInterface[]>();

        public GameNetwork(UDPConnection UDPGameData, TCPConnection Host) : this(UDPGameData, Host, null)
        {
            
        }

        public GameNetwork(UDPConnection UDPGameData, TCPConnection Host, LogWriter Logger) : base (UDPGameData, Logger)
        {
            UpdateClientConnections();
        }

        public void UpdateClientConnections()
        {
            for (int ClientID = 0; ClientID < ClientConnections.Count; ClientID++)
            {
                packagesOfAllClients.Add(GetAllDataTCP(ClientID));
            }
        }

        public void GrabAllNetworkDataForTheNextFrame()
        {
            for(int ClientID=0; ClientID < ClientConnections.Count; ClientID++)
            {
                packagesOfAllClients[ClientID] = GetAllDataTCP(ClientID);
            }
        }

        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            SendAllUDP();
        }

        public void AddClient(NetworkConnection connection)
        {
            AddClientConnection(connection); // from Inherited Class
            UpdateClientConnections(); // so we can use this new connection for GrabAllDataForTheNExtFram
        }

        private void SendAllUDP()
        {

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
