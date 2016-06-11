using System;
using System.Collections.Generic;
using System.Linq;
using NetworkLibrary.Utility;
using GameLogicLibrary;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;

namespace PingPongServer
{
    
    public class GameNetwork : NetworkInterface
    {
        
        public GameNetwork(UDPConnection UDPGameData, TCPConnection Host) : this(UDPGameData, Host, null)
        {
            
        }

        public GameNetwork(UDPConnection UDPGameData, TCPConnection Host, LogWriter Logger) : base (UDPGameData, Logger)
        {

        }
        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            SendAllUDP();
        }

        public void AddClient(NetworkConnection connection)
        {
            AddClientConnection(connection);
        }

        private void SendAllUDP()
        {

        }

        public List<ClientControls> GetAllClientControls()
        {

            List<ClientControls> allClientControls = new List<ClientControls>();
            foreach (PackageInterface package in allPackages)
            {
                if (package.PackageType == PackageType.ClientControl)
                    allClientControls.Add(((ClientControlPackage)package).ControlInput);
            }

            return allClientControls;
        }

        public ClientControls GetLastClientControl(int ID)
        {
            ClientControls lastControl = ClientControls.NoInput;
            foreach (PackageInterface package in allPackages)
            {
                if (package.PackageType == PackageType.ClientControl)
                    lastControl = ((ClientControlPackage)package).ControlInput;
            }

            return lastControl;
        }

        public ClientMovement GetLastPlayerMovement(int session)
        {
            List<PackageInterface> allPackages = GetAllPackagesOfTCPSession(session);
            ClientMovement lastMovement = ClientMovement.NoInput;
            foreach (PackageInterface package in allPackages)
            {
                if (package.PackageType == PackageType.ClientPlayerMovement)
                    lastMovement = ((PlayerMovementPackage)package).PlayerMovement;
            }

            return lastMovement;
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
