using System;
using System.Collections.Generic;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;


namespace PingPongServer
{

    public class GameNetwork : NetworkInterface
    {
        public GameNetwork(UDPConnection UDPGameData, UniqueIDGenerator sessionIDManager) : this(UDPGameData, new NoLog(), sessionIDManager) { }
        public List<int> DeadSessions = new List<int>();
        public delegate void ClientLostEventHandler(object sender, EventArgs e);
        public event ClientLostEventHandler ClientLost;
        private LogWriterConsole LogWriter = new LogWriterConsole();
        private UniqueIDGenerator SessionIDManager;

        public GameNetwork(UDPConnection UDPGameData, Logger Logger, UniqueIDGenerator sessionIDManager) : base (UDPGameData, Logger)
        {
            LogWriter = (LogWriterConsole)Logger;
            SessionDied += SessionDiedHandler;
            SessionIDManager = sessionIDManager;
        }

        private void SessionDiedHandler(NetworkInterface sender, int sessionID)
        {
            DeadSessions.Add(sessionID);
            ClientLost?.Invoke(this, EventArgs.Empty);
        }
        
        public void SendTCPPackageToClient(PackageInterface packet , int sessionID)
        {
            Out.SendDataTCP(packet, sessionID);
        }
        
        public void AddClient(NetworkConnection connection)
        {
            AddClientConnection(connection); // from Inherited Class
        }
        
        public void AddObserver(NetworkConnection connection)
        {
            AddClientConnection(connection);
        }

        public Dictionary<int, PackageInterface[]> GrabAllNetworkDataForNextFrame()
        {
            return In.GetDataFromEverySessionTCP();            
        }

        public void BroadcastFramesToClients(ServerDataPackage Frame)
        {
            Out.BroadCastUDP(Frame);
        }

        public void BroadcastScore(ServerGameControlPackage score)
        {
            Out.BroadCastTCP(score);
        }

        public void BroadcastStartGamePackage(ServerMatchmakingStatusResponse GameFoundPackage)
        {
            Out.BroadCastTCP(GameFoundPackage);
        }

        private PackageInterface[] GetAllOfPackagesOfClient(int sessionID)
        {
            return In.GetAllDataTCP(sessionID);
        }

        public void Close()
        {
            SessionDied -= SessionDiedHandler;
            ClientLost = null;
            int[] SessionIDs = GetSessionIDs;
            Disconnect();
            foreach (int sessionID in SessionIDs)
                SessionIDManager.FreeID(sessionID);
            LogWriter.NetworkLog("Tearing Down Network");

        }

        public void BroadcastGenericTCPPackage(PackageInterface package)
        {
            Out.BroadCastTCP(package);
        }

        public void BroadcastGenericUDPPackage(PackageInterface package)
        {
            Out.BroadCastUDP(package);
        }

        

    }
}
