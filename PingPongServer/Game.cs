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
        public GameStates GameState { get; private set; }
        List<Player> Players = new List<Player>();
        List<Client> Clients = new List<Client>();
        public PingPongBall Ball;                
        private int maxPlayers;
        private int NeededNumberOfPlayersForGameToStart;

        public GameNetwork Network;
        public ServerDataPackage NextFrame;
        public Dictionary<int, PackageInterface[]> packagesForNextFrame = new Dictionary<int, PackageInterface[]>();
        public List<GameTeam> ListOfTeams = new List<GameTeam>();

                

        public Game(GameNetwork Network, int NeededNumberOfPlayersForGameToStart)
        {
            this.Network = Network;            
            GameState = GameStates.Initializing;
            InitializeTeams();
            Ball = new PingPongBall();
            Ball.PositionX = 50;
            Ball.PositionY = 50;
            this.NeededNumberOfPlayersForGameToStart = NeededNumberOfPlayersForGameToStart;
            maxPlayers = NeededNumberOfPlayersForGameToStart;            

        }

        
        public void StartGame(object justToMatchSignatureForThreadPool)
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            GameState = GameStates.Running;
            while (GameState == GameStates.Running)
            {
                ServerPackage = PrepareNextFrame();
                Network.BroadcastFramesToClients(ServerPackage);
                Thread.Sleep(3);
            }
            GameState = GameStates.Finished; // Move to somewhere else game logic e.g score reached
        }

        public void AddClient(NetworkConnection client)
        {
            Network.AddClientConnection(client);
            Client newClient = new Client(client.ClientSession.SessionID, Players.Count, GetFreeTeam());
            newClient.Players.Add(new Player(Players.Count, GetFreeTeam()));
            Clients.Add(newClient);
            packagesForNextFrame.Add(newClient.session, new PackageInterface[0]);
            
            AcceptNewPlayersFromConnectedClients();
            UpdatePlayersFromClients();

            if (Players.Count == maxPlayers)
                GameState = GameStates.Ready;
        }

        public void UpdatePlayersFromClients()
        {
            Players = new List<Player>();
            foreach(Client c in Clients)
                Players.AddRange(c.Players);
            foreach(Player p in Players)
            {
                p.DirectionX = 10;
                p.DirectionY = 10;
                p.PositionX = 10;
                p.PositionY = 10;
            }
        }

        private void GetAllThe()
        {
            packagesForNextFrame = Network.GrabAllNetworkDataForNextFrame();
        }

              
        public void AcceptNewPlayersFromConnectedClients()
        {
            GetAllThe();
            foreach(Client c in Clients)
            {                 
                PackageInterface[] AddPlayerRequests = getAllDataRelatedToClient(c.session);

                foreach(PackageInterface possibleRequest in AddPlayerRequests)
                {
                    if (possibleRequest == null || possibleRequest.PackageType != PackageType.ClientAddPlayerRequest)
                        continue;

                    ClientAddPlayerRequest a = (ClientAddPlayerRequest)possibleRequest;
                    c.Players.Add(new Player(Players.Count, a.RequestedTeam));
                    if (Players.Count >= maxPlayers)
                    {
                        GameState = GameStates.Ready;
                        break;
                    }
                }

                
            }
            UpdatePlayersFromClients();             
        }

        private PackageInterface[] getAllDataRelatedToClient(int sessionID)
        {
            List<PackageInterface> ps = new List<PackageInterface>();
            if (packagesForNextFrame[sessionID] == null)
                return new PackageInterface[0];
            foreach (PackageInterface p in packagesForNextFrame[sessionID])
            {
                ps.Add(p);
            }
            return ps.ToArray();
        }
        
        private Teams GetFreeTeam()
        {
            if (maxPlayers/2 == ListOfTeams[0].PlayerCount)
                return Teams.Team1;
            return Teams.Team2;               
        }
        
        private ServerDataPackage PrepareNextFrame()
        {
            this.GameState = GameStates.Running;
            return CalculateFrame();
        }

        public ServerDataPackage CalculateFrame()
        {
            NextFrame = new ServerDataPackage();

            Ball.PositionX = (Ball.PositionX + 0.2F) % 300;
            NextFrame.Ball.PositionX = Ball.PositionX;

            Ball.PositionY = (Ball.PositionX + 0.2F) % 300;
            NextFrame.Ball.PositionY = Ball.PositionY;

            foreach (Player player in Players)
            {
                player.PositionX = (player.PositionX + 0.2F) % GameInitializers.BORDER_WIDTH;
                player.PositionY = (player.PositionY + 0.2F) % GameInitializers.BORDER_HEIGHT;
            }

            return NextFrame;
        }

        private void InitializeTeams()
        {
            GameTeam Team1 = new GameTeam(Teams.Team1);
            ListOfTeams.Add(Team1);
            GameTeam Team2 = new GameTeam(Teams.Team2);
            ListOfTeams.Add(Team2);
        }

     
        public ClientControls GetLastPlayerControl(int playerID)
        {
            List<ClientControlPackage> cc = new List<ClientControlPackage>();
            foreach (Client c in Clients)
            {
                PackageInterface[] ps = getAllDataRelatedToClient(c.session);
                foreach(PackageInterface p in ps)
                {
                    if (p == null || p.PackageType == PackageType.ClientControl)
                        continue;
                    cc.Add((ClientControlPackage)p);
                }
            }
            return cc[cc.Count - 1].ControlInput;
        }

        public ClientMovement GetLastPlayerMovement(int ClientID, int playerID)
        {

            List<PlayerMovementPackage> cc = new List<PlayerMovementPackage>();
            foreach (Client c in Clients)
            {
                PackageInterface[] ps = getAllDataRelatedToClient(c.session);
                foreach (PackageInterface p in ps)
                {
                    if (p == null || p.PackageType == PackageType.ClientPlayerMovement)
                        continue;
                    cc.Add((PlayerMovementPackage)p);
                }
            }
            return cc[cc.Count - 1].PlayerMovement;
        }

        public class Client
        {
            Game Game;
            public int ClientID;
            public int session;
            public List<Player> Players = new List<Player>();

            public Client(int sessionID, int FirstPlayerID, Teams FirstPlayerTeam)
            {
                this.session = sessionID;
                Players.Add(new Player(FirstPlayerID, FirstPlayerTeam));

            }         

            
        }
        
        public class Player : ServerDataPackage.Player
        {
            public ClientMovement PlayerMovement { get; set; }

            Teams Team;
            public int PlayerID;

            public Player(int PlayerID, Teams Team)
            {
                this.PlayerID = PlayerID;
                this.Team = Team;
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
