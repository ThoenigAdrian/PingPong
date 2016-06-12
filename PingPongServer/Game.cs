using System;
using System.Threading;
using GameLogicLibrary;
using System.Collections.Generic;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.DataPackages;

namespace PingPongServer
{
    public class Game
    {
        List<Player> Players = new List<Player>();
        List<Client> Clients = new List<Client>();
        public GameNetwork Network;
        public PingPongBall Ball;
        public int maxPlayers;        
        public ServerDataPackage NextFrame;
        public List<GameTeam> ListOfTeams = new List<GameTeam>();

        public GameStates GameState { get; private set; }

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

            GameTeam g = new GameTeam(Teams.Team1);
            ListOfTeams.Add(g);
            g = new GameTeam(Teams.Team2);
            ListOfTeams.Add(g);

            AddClient(Network.ClientConnections[0]);
            maxPlayers = PlayerCount;

        }

        private void Update()
        {
            foreach(Client c in Clients)
                Players.AddRange(c.Players);

        }
        
        private void AcceptNewPlayersFromConnectedClients()
        {
            for(int ClientID = 0; ClientID < Clients.Count; ClientID++)
            {
                ClientAddPlayerRequest Packet = Network.GetLastAddPlayerRequest(ClientID);
                    
                Clients[ClientID].AddPlayer(Players.Count, Packet.RequestedTeam);
                if (Players.Count >= maxPlayers)
                {
                    GameState = GameStates.Ready;
                    break;
                }
            }
            Update();
        }
        
        public void AddClient(NetworkConnection client)
        {
            Network.AddClientConnection(client);
            Client newClient = new Client(Network, Clients.Count - 1, Players.Count - 1, GetFreeTeam());
            if (Players.Count == maxPlayers)
                GameState = GameStates.Ready;
            Update();

        }

        public Teams GetFreeTeam()
        {
            if (maxPlayers/2 == ListOfTeams[0].PlayerCount)
                return Teams.Team1;
            return Teams.Team2;           
                
        }


        public void StartGame(object justToMatchSignature)
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            while (GameState == GameStates.Running)
            {
                ServerPackage = PrepareNextFrame();
                Network.BroadcastFramesToClients(ServerPackage);
                Thread.Sleep(20);
            }
            GameState = GameStates.Finished;
            
        }

        public ServerDataPackage PrepareNextFrame()
        {
            this.GameState = GameStates.Running;
            return CalculateFrame();
        }

        public ServerDataPackage CalculateFrame()
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

            return NextFrame;
        }

        public class Client
        {
            GameNetwork GameNetwork;
            public int ClientID;
            public List<Player> Players = new List<Player>();

            public Client(GameNetwork GameNetwork, int ClientID, int FirstPlayerID, Teams FirstPlayerTeam)
            {
                this.ClientID = ClientID;
                this.GameNetwork = GameNetwork;
                AddPlayer(FirstPlayerID, FirstPlayerTeam);

            }

            public void AddPlayer(int PlayerID, Teams Team)
            {
                Players.Add(new Player(this, PlayerID, Team));
            }

            public ClientControls ReceiveLastPlayerControl(int PlayerID)
            {
                return GameNetwork.GetLastPlayerControl(ClientID, PlayerID);
            }

            public ClientMovement ReceiveLastPlayerMovement(int PlayerID)
            {
                return GameNetwork.GetLastPlayerMovement(ClientID, PlayerID);
            }
        }
        
        public class Player : ServerDataPackage.Player
        {
            public ClientMovement PlayerMovement { get; set; }
            Teams Team;
            Client Client;
            public int PlayerID;

            public Player(Client Client, int PlayerID, Teams Team)
            {
                this.Client = Client;
                this.PlayerID = PlayerID;
                this.Team = Team;
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

        public class GameTeam
        {
            public Teams Team;
            public List<Player> PlayersOfTeam = new List<Player>();
            public int PlayerCount { get { return PlayersOfTeam.Count; }  set { } }

            public GameTeam(Teams Team)
            {

                this.Team = Team;
            }

            public void AddPlayer(Player playerToAdd)
            {
                PlayersOfTeam.Add(playerToAdd);
            }
        }

        public class PingPongBall : ServerDataPackage.PingPongBall
        {

        }
    }
}
