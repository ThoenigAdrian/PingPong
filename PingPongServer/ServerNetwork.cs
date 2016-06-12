using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.Utility;
using System.Net.Sockets;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataPackages;
using GameLogicLibrary;
using System.Collections.Generic;

namespace PingPongServer
{
    public class ServerNetwork : NetworkInterface
    {

        List<PackageInterface> allPackages;

        public ServerNetwork() : this(null)
        {

        }

        public ServerNetwork(LogWriter logger)
            : base(NetworkLibrary.NetworkConstants.SERVER_PORT, logger)
        {
            
        }

        protected override PackageAdapter InitializeAdapter()
        {
            return new PackageAdapter();
        }
        
        public void BroadcastFramesToClients(ServerDataPackage serverData, int GameID)
        {
            BroadCastUDP(serverData);
        }

        public void BroadcastGameControlsToClients(ServerGameControlPackage serverControls)
        {
            BroadCastTCP(serverControls);
        }

        private SendTCP()
        {
            this.SendDataTCP()
        }
                
        // Execute once per Frame then access the GetAllClientControls , GetLastPlayerMovement etc.
        public void ReceivePlayerTraffic(int session)
        {
            allPackages = GetAllPackagesOfTCPSession(session);
        }

        
    }
}
