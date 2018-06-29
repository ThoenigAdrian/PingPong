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

            ServerGameControlPackage scoreData = new ServerGameControlPackage();
            scoreData.Score.Team1 = GameStructure.GameTeams[0].score;
            scoreData.Score.Team2 = GameStructure.GameTeams[1].score;
            Network.BroadcastScore(scoreData);
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            Logger.GameLog("Team Scored");
            Logger.GameLog("Team Red: " + GameStructure.GameTeams[0].score.ToString() + "\tTeam Blue: " + GameStructure.GameTeams[1].score.ToString());
            Logger.GameLog("This was the final point");

        }

        private void OnClientLost(object sender, EventArgs e)
        {
            bool gameOver = true;
            List<Player> DisconnectedPlayers = new List<Player>();

            foreach (Client c in Clients)
                if (Network.DiedSessions.Contains(c.SessionID))
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
            GameFinished += (caller as Server).OnGameFinished;
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

            int maxTeamSize = GameStructure.maxPlayers / 2;

            foreach (int team in playerTeamWish)
            {
                int index = GameStructure.PlayersCount;
                int teamOpenSpots = maxTeamSize - GameStructure.GameTeams[team].PlayerList.Count;

                if (teamOpenSpots < 1)
                    continue;
  
                Player newPlayer = new Player(index, team, GameInitializers.GetPlayerX(team, index));

                newClient.AddPlayer(newPlayer);
                GameStructure.AddPlayer(newPlayer, team);
            }

            SendServerInitResponse(newClient);
            Clients.Add(newClient);                       
            
            if (GameStructure.PlayersCount == maxPlayers)
                GameState = GameStates.Ready;

            return true;
        }

        private void SendServerInitResponse(Client client)
        {
            ServerInitializeGameResponse packet = new ServerInitializeGameResponse();
            packet.m_field = new GameField();
            packet.m_ball = new Ball();
            packet.m_players = new Player[GameStructure.PlayersCount];
            packet.m_players = client.Players.ToArray();

            Network.SendTCPPackageToClient(packet, client.SessionID);
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
                PackageInterface[] ps = getAllDataRelatedToClient(c.SessionID);
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
                PackageInterface[] ps = getAllDataRelatedToClient(c.SessionID);
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
