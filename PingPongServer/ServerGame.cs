using System;
using System.Threading;
using System.Collections.Generic;

using GameLogicLibrary;
using GameLogicLibrary.GameObjects;

using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;

namespace PingPongServer
{
    public class ServerGame
    {
        public GameStates GameState { get; private set; }
        List<Client> Clients = new List<Client>();

        private int maxPlayers;
        private int NeededNumberOfPlayersForGameToStart;

        public GameNetwork Network;
        public ServerDataPackage NextFrame;
        public Dictionary<int, PackageInterface[]> packagesForNextFrame = new Dictionary<int, PackageInterface[]>();
        public GameStructure GameStructure;
        private LogWriterConsole Logger = new LogWriterConsole();



        public ServerGame(GameNetwork Network, int NeededNumberOfPlayersForGameToStart)
        {
            Logger.GameLog("Initialising a new Game with " + Convert.ToString(NeededNumberOfPlayersForGameToStart) + "");
            this.Network = Network;            
            GameState = GameStates.Initializing;
            GameStructure = new GameStructure(NeededNumberOfPlayersForGameToStart);
            this.NeededNumberOfPlayersForGameToStart = NeededNumberOfPlayersForGameToStart;
            maxPlayers = NeededNumberOfPlayersForGameToStart;            

        }

        public override string ToString()
        {
            // Build a nice custom string in the future
            return "Game with " + GameStructure.PlayersCount.ToString() + " Players " + "and Score" + GameStructure.GameTeams.ToString();
        }

        public void StartGame(object justToMatchSignatureForThreadPool)
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            GameState = GameStates.Running;
            Logger.GameLog("Game started");
            int i = 0;
            while (GameState == GameStates.Running)
            {
                i++;
                if (i == 1000)
                    break;
                ServerPackage = CalculateFrame();
                Network.BroadcastFramesToClients(ServerPackage);
                Thread.Sleep(10);
            }
            Logger.GameLog("Game finished");
            Logger.NetworkLog("Tearing Down Network");
            Network.Close();
            GameState = GameStates.Finished; // Move to somewhere else game logic e.g score reached
        }

        public bool AddClient(NetworkConnection client, int playersToAdd)
        {
            if (GameStructure.MissingPlayers < playersToAdd)
                return false; 

            Network.AddClientConnection(client);

            Client newClient = new Client(client.ClientSession.SessionID, GameStructure.PlayersCount, GameStructure.GetFreeTeam());
            for (int index = 0; index < playersToAdd; index++)
            {
                float playerPosition = 0;

                if (GameStructure.GetFreeTeam() == 0)
                    playerPosition = GameInitializers.PLAYER_1_X + GameStructure.GameTeams.Count * 30F;
                if (GameStructure.GetFreeTeam() == 1)
                    playerPosition = GameInitializers.PLAYER_2_X - GameStructure.GameTeams.Count * 30F;

                Player newPlayer = new Player(GameStructure.PlayersCount, GameStructure.GetFreeTeam(), playerPosition);

                newClient.Players.Add(new Player(GameStructure.PlayersCount, GameStructure.GetFreeTeam(), 50F));
                GameStructure.AddPlayer(newPlayer, GameStructure.GetFreeTeam());
            }

            Clients.Add(newClient);

            ReserveEntryInPackagesForNextFrameForClient(newClient);
            
            
            if (GameStructure.PlayersCount == maxPlayers)
                GameState = GameStates.Ready;

            return true;
        }
        
        public void RejoinClient(NetworkConnection client)
        {
            Network.AddClientConnection(client);
        }
        
        private void GetAllThe()
        {
            packagesForNextFrame = Network.GrabAllNetworkDataForNextFrame();
        }
                              
        private void ReserveEntryInPackagesForNextFrameForClient(Client client)
        {
            packagesForNextFrame.Add(client.session, new PackageInterface[0]);
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
        
        

        public ServerDataPackage CalculateFrame()
        {
            NextFrame = new ServerDataPackage();

            GameStructure.CalculateFrame(10);
            NextFrame.Ball.PositionX = GameStructure.Ball.PositionX;
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
