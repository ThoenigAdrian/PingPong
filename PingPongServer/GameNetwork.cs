using System;
using System.Collections.Generic;
using System.Linq;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;

namespace PingPongServer
{
    
    class GameNetwork
    {
        public int GameID;
        private ServerNetwork ServerNetwork;
        public GameNetwork(ServerNetwork ServerNetwork)
        {
            this.ServerNetwork = ServerNetwork;
        }

        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            ServerNetwork.BroadcastFramesToClients(Frame, GameID);
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
