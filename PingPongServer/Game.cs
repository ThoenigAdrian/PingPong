using System;
using GameLogicLibrary;
using System.Collections.Generic;
using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPongServer
{
    class Game
    {
        List<Player> Players = new List<Player>();
        
        public Game(ServerNetwork Network)
        {
            
        }
        public int StartGame()
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            CalculateFrame();
            Network.BroadcastUDP();
            return 0;
        }
        private class Player
        {
            public ClientMovement PlayerMovement {get; set;}
            public TCPConnection ClientConnection;
            private Player(TCPConnection ClientConnection)
            {
                this.ClientConnection = ClientConnection;
            }
            private void PackageReceivedEvent(object sender, EventArgs e)
            {
                PackageInterface getPackageType = (PackageInterface)sender;
                PlayerMovement = (ClientMovement)sender;
            }
        }
    }
}
