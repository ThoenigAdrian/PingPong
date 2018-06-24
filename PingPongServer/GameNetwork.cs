using System;
using System.Collections.Generic;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Net.Sockets;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace PingPongServer
{

    public class GameNetwork : NetworkInterface
    {
        public GameNetwork(UDPConnection UDPGameData) : this(UDPGameData, new NoLog()) { }
        public List<int> DiedSessions = new List<int>();
        public delegate void ClientLostEventHandler(object sender, EventArgs e);
        public event ClientLostEventHandler ClientLost;

        public GameNetwork(UDPConnection UDPGameData, Logger Logger) : base (UDPGameData, Logger)
        {
            SessionDied += SessionDiedHandler;
        }

        private void SessionDiedHandler(NetworkInterface sender, int sessionID)
        {
            DiedSessions.Add(sessionID);
            if (ClientLost != null)
                ClientLost(this, EventArgs.Empty);
        }
        
        public void SendTCPPackageToClient(PackageInterface packet , int sessionID)
        {
            Out.SendDataTCP(packet, sessionID);
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
