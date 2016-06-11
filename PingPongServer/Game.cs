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
    public class Game
    {
        List<Player> Players = new List<Player>();
        List<Client> Clients = new List<Client>();
        public GameNetwork Network;
        public PingPongBall Ball;
        private int maxPlayers;
        public ServerDataPackage NextFrame;

        GameStates GameState;
        public bool isReady { get; private set; }

        public enum GameStates
        {
            Initializing,
            Running,
            Aborted,
            Finished,
            Ready
        }

        public Game(GameNetwork Network, int PlayerCount)
        {
            this.Network = Network;
            this.GameState = GameStates.Initializing;
            InitGame();

        }

        private void AcceptNewPlayersFromConnectedClients()
        {
            for(int ClientID = 0; ClientID < Clients.Count; ClientID++)
            {
                Clients[ClientID].AddPlayer(ClientID);
                if (Players.Count >= maxPlayers)
                {
                    GameState = GameStates.Ready;
                }
            }
        }

        private void AddPlayer(int PlayerID)
        {
            
        }

        public void AddClient(NetworkConnection client)
        {
            Network.AddClientConnection(client);
        }

        private void InitGame()
        {
            
        }

        public int StartGame()
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();


            while (GameState == GameStates.Running)
            {
                PrepareNextFrame();
                Network.BroadcastFramesToClients(ServerPackage);
            }

            return 0;
        }

        public void PrepareNextFrame()
        {
            CalculateFrame();
            this.GameState = GameStates.Running;
        }

        public void CalculateFrame()
        {
            NextFrame = new ServerDataPackage();

            Ball.PositionX = (Ball.PositionX + 3F) % 300 + 100;
            NextFrame.Ball.PositionX = Ball.PositionX;

            Ball.PositionY = (Ball.PositionX + 3F) % 300 + 100;
            NextFrame.Ball.PositionY = Ball.PositionY;

            foreach (Player player in Players)
            {
                player.PositionX = (player.PositionX + 0.5F) % GameInitializers.BORDER_WIDTH;
                player.PositionY = (player.PositionY + 0.5F) % GameInitializers.BORDER_HEIGHT;
            }


        }

        public class Client
        {
            GameNetwork GameNetwork;
            public int ClientID;
            List<Player> Players = new List<Player>();

            public Client(GameNetwork GameNetwork, int ClientID)
            {
                this.ClientID = ClientID;
                this.GameNetwork = GameNetwork;
            }

            public void AddPlayer(int PlayerID)
            {
                Players.Add(new Player(this, PlayerID));
            }

            public ClientControls ReceiveLastPlayerControl(int PlayerID)
            {
                throw new NotImplementedException();
            }

            public ClientMovement ReceiveLastPlayerMovement(int PlayerID)
            {
                throw new NotImplementedException();
            }
        }
        
        public class Player : ServerDataPackage.Player
        {
            public ClientMovement PlayerMovement { get; set; }
            Client Client;
            public int PlayerID;

            public Player(Client Client, int PlayerID)
            {
                this.Client = Client;
                this.PlayerID = PlayerID;
            }

            public ClientControls ReceiveLastPlayerControl()
            {
                return Client.ReceiveLastPlayerControl(this.PlayerID);
            }
            public ClientMovement ReceiveLastPlayerMovement()
            {
                return Client.ReceiveLastPlayerMovement(this.PlayerID);
            }


        }

        public class PingPongBall : ServerDataPackage.PingPongBall
        {

        }
    }
}
