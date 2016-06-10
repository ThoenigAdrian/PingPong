using System;
using GameLogicLibrary;
using System.Collections.Generic;
using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using GameLogicLibrary;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPongServer
{
    class Game
    {
        List<Player> Players = new List<Player>();
        //private int PlayerCount;
        ServerNetwork Network;
        GameStates GameState;

        public enum GameStates
        {
            Initializing,
            Running,
            Aborted,
            Finished
        }
        
        public Game(ServerNetwork Network)
        {
            this.Network = Network;
            this.GameState = GameStates.Initializing;
            InitGame();

        }

        private void InitGame()
        {
            foreach (TCPConnection ClientConnection in Network.TCPConnections)
            {
                Players.Add(new Player(ClientConnection));
            }
        }

        public int StartGame()
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            
            
            while (GameState == GameStates.Running)
            {
                PrepareNextFrame(ServerPackage);
                Network.BroadcastFramesToClients(ServerPackage);
            }
            
            return 0;
        }

        public void PrepareNextFrame(ServerDataPackage serverData)
        {

            CalculateFrame(serverData);
            this.GameState = GameStates.Running;
        }

        public void CalculateFrame(ServerDataPackage serverData)
        {
            // Do Calculations
            
            serverData.BallDirX = (serverData.BallDirX + 3F) % 300 + 100;
            serverData.BallDirY += (serverData.BallDirX + 3F) % 300 + 100;
            foreach (ServerDataPackage.Player player in serverData.PlayerList)
            {
                player.PositionX = player.PositionX + 0.5F;
                player.PositionX = player.PositionX % GameInitializers.BORDER_WIDTH;
            }

        }


        public class Player
        {
            public ClientMovement PlayerMovement {get; set;}
            ServerNetwork NetworkInterface;
            public int PlayerID;
            public int SessionID;
            public Player(ServerNetwork network)
            {
                this.NetworkInterface = network;
            }
            private void ReceiveControl()
            {
                PlayerMovementPackage package = NetworkInterface.GetPlayerMovement();
                PlayerMovement = (ClientMovement)sender;
            }
        }
    }
}
