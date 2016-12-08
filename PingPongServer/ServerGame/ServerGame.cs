using System;
using System.Threading;
using System.Collections.Generic;


using PingPongServer.ServerGame;
using PingPongServer.ServerGame.ServerGameObjects;

using GameLogicLibrary;
using GameLogicLibrary.GameObjects;

using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;

namespace PingPongServer.ServerGame
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
        public GameEngine GameEngine;
        private LogWriterConsole Logger = new LogWriterConsole();
        public int GameID = 0;
        public delegate void GameFinishedEventHandler(object sender, EventArgs e);
        public event GameFinishedEventHandler GameFinished;

        
        public Game(GameNetwork Network, int NeededNumberOfPlayersForGameToStart)
        {
            Logger.GameLog("Initialising a new Game with " + Convert.ToString(NeededNumberOfPlayersForGameToStart) + " Players");
            this.Network = Network;
            this.Network.ClientLost += OnClientLost;
            GameState = GameStates.Initializing;
            GameStructure = new GameStructure(NeededNumberOfPlayersForGameToStart);
            GameEngine = new GameEngine(GameStructure);
            this.NeededNumberOfPlayersForGameToStart = NeededNumberOfPlayersForGameToStart;
            maxPlayers = NeededNumberOfPlayersForGameToStart;
            GameEngine.TeamScored += OnTeamScored;

        }

        private void OnTeamScored(object sender, EventArgs e)
        {
            Logger.GameLog("Team Scored");
            Logger.GameLog("Team Red: " + GameStructure.GameTeams[0].score.ToString() + "\tTeam Blue: " + GameStructure.GameTeams[1].score.ToString());
        }

        private void OnClientLost(object sender, EventArgs e)
        {
            bool gameOver = true;
            List<Player> DisconnectedPlayers = new List<Player>();

            foreach (Client c in Clients)
                if (Network.DiedSessions.Contains(c.session))
                    DisconnectedPlayers.AddRange(c.Players);

            foreach(GameStructure.GameTeam team in GameStructure.GameTeams.Values)
            {
                foreach(Player p in team.PlayerList)
                {
                    if (!DisconnectedPlayers.Contains(p))
                    {
                        gameOver = false;
                        break;
                    }

                }
            }

            if (gameOver)
            {
                OnGameFinished();
            }
                
        }

        public override string ToString()
        {
            // Build a nice custom string in the future
            string str = "Game [" + GameID + "] \n\t" + "Players : " + GameStructure.PlayersCount.ToString();
            foreach (KeyValuePair<int, GameStructure.GameTeam> team in GameStructure.GameTeams)
            {
                str += "\n\tTeam :" + team.Key;
                str += team.Value.ToString();
            }
            foreach(Client c in Clients)
            {
                str += c.ToString();
            }
            return str;
        }

        // Caller will be notified via Event when Game is finished
        public void StartGame(object caller)
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();            
            GameFinished += (GameFinishedEventHandler)((Server)caller).OnGameFinished;
            GameState = GameStates.Running;
            Logger.GameLog("Game started");

            while (GameState == GameStates.Running)
            {
                GetNetworkDataForNextFrame();
                ServerPackage = CalculateFrame();
                Network.BroadcastFramesToClients(ServerPackage);
                Thread.Sleep(5);
            }
            OnGameFinished();
            Network.Close();
            GameState = GameStates.Finished; // Move to somewhere else game logic e.g score reached
        }

        public bool AddClient(NetworkConnection client, int[] playerTeamWish)
        {
            if (GameStructure.MissingPlayersCount < playerTeamWish.Length)
                return false; 

            Network.AddClientConnection(client);            
            Client newClient = new Client(GameStructure, client.ClientSession.SessionID);
            
            for (int index = 0; index < playerTeamWish.Length; index++)
            {
                float playerPosition = 0;
                                
                if (playerTeamWish[index] == 0 && GameStructure.maxPlayers / 2 - GameStructure.GameTeams[playerTeamWish[index]].PlayerList.Count >= 1)
                    playerPosition = GameInitializers.PLAYER_1_X + GameStructure.GameTeams.Count * 30F;
                else if (playerTeamWish[index] == 1 && GameStructure.maxPlayers / 2 - GameStructure.GameTeams[playerTeamWish[index]].PlayerList.Count >= 1)
                    playerPosition = GameInitializers.PLAYER_2_X - GameStructure.GameTeams.Count * 30F;
                                

                Player newPlayer = new Player(GameStructure.PlayersCount, GameStructure.GetFreeTeam(), playerPosition);

                newClient.AddPlayer(newPlayer, GameStructure);
            }
            ServerInitializeGameResponse packet = new ServerInitializeGameResponse();
            packet.m_field = new GameField();
            packet.m_ball = new Ball();
            packet.m_players = new Player[2];
            packet.m_players[0] = new Player(0, 0, GameInitializers.PLAYER_1_X);
            packet.m_players[1] = new Player(1, 1, GameInitializers.PLAYER_2_X);

            Network.SendTCPPackageToClient(packet, client.ClientSession.SessionID);
            Clients.Add(newClient);                       
            
            if (GameStructure.PlayersCount == maxPlayers)
                GameState = GameStates.Ready;

            return true;
        }
        
        public void RejoinClient(NetworkConnection client)
        {
            Network.AddClientConnection(client);
        }
        
        private void GetNetworkDataForNextFrame()
        {
            packagesForNextFrame = Network.GrabAllNetworkDataForNextFrame();
        }
        
        protected virtual void OnGameFinished()
        {
            GameState = GameStates.Finished;
            Logger.GameLog("Game finished");
            Logger.NetworkLog("Tearing Down Network");

            if (GameFinished != null)
                GameFinished(this, EventArgs.Empty);
        }


        private PackageInterface[] getAllDataRelatedToClient(int sessionID)
        {
            List<PackageInterface> ps = new List<PackageInterface>();

            if (!packagesForNextFrame.ContainsKey(sessionID))
                return new PackageInterface[0];

            foreach (PackageInterface p in packagesForNextFrame[sessionID])
                ps.Add(p);
            
            
            
            return ps.ToArray();
        }
        
        

        public ServerDataPackage CalculateFrame()
        {
            NextFrame = new ServerDataPackage();

            foreach (KeyValuePair<int, GameStructure.GameTeam> Team in GameStructure.GameTeams)
            {
                foreach (Player p in Team.Value.PlayerList)
                {
                    if ((ClientMovement)GetLastPlayerMovement(p.ID) == ClientMovement.Down)
                        p.DirectionY = p.Speed;
                    else if ((ClientMovement)GetLastPlayerMovement(p.ID) == ClientMovement.Up)
                        p.DirectionY = -p.Speed;
                    else if ((ClientMovement)GetLastPlayerMovement(p.ID) == ClientMovement.StopMoving)
                        p.DirectionY = 0;

                    NextFrame.Players.Add(p);
                }
            }

            GameEngine.CalculateFrame(10);
            NextFrame.Ball.PositionX = GameStructure.Ball.PositionX;
            NextFrame.Ball.PositionY = GameStructure.Ball.PositionY;

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
                    if (p == null || p.PackageType != PackageType.ClientControl)
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
                    PlayerMovementPackage movementPackage = p as PlayerMovementPackage;

                    if (movementPackage == null || movementPackage.PackageType != PackageType.ClientPlayerMovement)
                        continue;

                    if(movementPackage.PlayerID == playerID)
                        cc.Add((PlayerMovementPackage)p);
                }
            }
            if (cc.Count == 0)
                return ClientMovement.NoInput;
            return cc[cc.Count - 1].PlayerMovement;
        }              
                        
    }
}
