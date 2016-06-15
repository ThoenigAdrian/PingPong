using System;
using System.Threading;
using System.Collections.Generic;

using GameLogicLibrary;
using GameLogicLibrary.GameObjects;

using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;


namespace PingPongServer
{
    public class Game
    {
        public GameStates GameState { get; private set; }
        List<Client> Clients = new List<Client>();

        private int maxPlayers;
        private int NeededNumberOfPlayersForGameToStart;

        public GameNetwork Network;
        public ServerDataPackage NextFrame;
        public Dictionary<int, PackageInterface[]> packagesForNextFrame = new Dictionary<int, PackageInterface[]>();
        public GameStructure GameStructure;

                

        public Game(GameNetwork Network, int NeededNumberOfPlayersForGameToStart)
        {
            this.Network = Network;            
            GameState = GameStates.Initializing;
            GameStructure = new GameStructure();
            GameStructure.maxPlayers = NeededNumberOfPlayersForGameToStart;
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
            Client newClient = new Client(client.ClientSession.SessionID, GameStructure.PlayersCount, GameStructure.GetFreeTeam());
            Player p = new Player(GameStructure.PlayersCount, GameStructure.GetFreeTeam(), 50F);
            newClient.Players.Add(p);
            GameStructure.AddPlayer(p, GameStructure.GetFreeTeam());
            Clients.Add(newClient);
            packagesForNextFrame.Add(newClient.session, new PackageInterface[0]);
            
            AcceptNewPlayersFromConnectedClients();

            if (GameStructure.PlayersCount == maxPlayers)
                GameState = GameStates.Ready;
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

                    ClientAddPlayerRequest PlayerAddRequest = (ClientAddPlayerRequest)possibleRequest;
                    int playerID = GameStructure.PlayersCount;
                    Player newPlayer = new Player(GameStructure.PlayersCount - 1, PlayerAddRequest.RequestedTeam, GameInitializers.PLAYER_1_X);
                    GameStructure.AddPlayer(newPlayer, PlayerAddRequest.RequestedTeam);
                    c.Players.Add(newPlayer);
                    if (GameStructure.PlayersCount >= maxPlayers)
                    {
                        GameState = GameStates.Ready;
                        break;
                    }
                }
                
            }           
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
        
        
        
        private ServerDataPackage PrepareNextFrame()
        {
            this.GameState = GameStates.Running;
            return CalculateFrame();
        }

        public ServerDataPackage CalculateFrame()
        {
            NextFrame = new ServerDataPackage();

            GameStructure.Ball.PositionX = (GameStructure.Ball.PositionX + 0.2F) % 300;
            NextFrame.Ball.PositionX = GameStructure.Ball.PositionX;

            GameStructure.Ball.PositionY = (GameStructure.Ball.PositionX + 0.2F) % 300;
            NextFrame.Ball.PositionY = GameStructure.Ball.PositionY;

            foreach (KeyValuePair<int, List<Player>> a in GameStructure.GameTeams)
            {
                foreach(Player p in a.Value)
                {
                    p.PlayerBar.PositionX = (p.PlayerBar.PositionX + 0.2F) % GameInitializers.BORDER_WIDTH;
                    p.PlayerBar.PositionY = (p.PlayerBar.PositionY + 0.2F) % GameInitializers.BORDER_HEIGHT;
                }                
            }
            return NextFrame;
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

        public ClientMovement GetLastPlayerMovement(int playerID)
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
            public int ClientID;
            public int session;
            public List<Player> Players = new List<Player>();

            public Client(int sessionID, int FirstPlayerID, int FirstPlayerTeam)
            {
                this.session = sessionID;
                Players.Add(new Player(FirstPlayerID, FirstPlayerTeam, GameInitializers.PLAYER_1_X));

            }         

            
        }        
                        
    }
}
