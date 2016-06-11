using System;
using GameLogicLibrary;
using System.Collections.Generic;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPongServer
{
    class Game
    {
        List<Player> Players = new List<Player>();
        //private int PlayerCount;
        GameNetwork Network;
        GameStates GameState;

        public enum GameStates
        {
            Initializing,
            Running,
            Aborted,
            Finished
        }

        public Game(GameNetwork Network)
        {
            this.Network = Network;
            this.GameState = GameStates.Initializing;
            InitGame();

        }

        public void AddPlayer()
        {

        }

        private void InitGame()
        {
            
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
            public ClientMovement PlayerMovement { get; set; }
            ServerNetwork NetworkInterface;
            public int PlayerID;
            public int SessionID;
            public Player(ServerNetwork network, int PlayerID, int SessionID)
            {
                this.NetworkInterface = network;
                this.SessionID = SessionID;
                this.PlayerID = PlayerID;
            }

            public ClientControls ReceiveLastControl()
            {
                return NetworkInterface.GetLastClientControl(SessionID);
            }
            public ClientMovement ReceiveLastMovement()
            {
                return NetworkInterface.GetLastPlayerMovement(SessionID);
            }


        }
    }
}
