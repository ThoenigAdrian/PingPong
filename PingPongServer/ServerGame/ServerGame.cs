using System;
using System.Threading;
using System.Collections.Generic;

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
        public int Tickrate = 100;
        public int GameID = 0;
        public delegate void GameFinishedEventHandler(object sender, EventArgs e);
        public event GameFinishedEventHandler GameFinished;
        public int NumberOfPlayers;

        private GameNetwork Network;
        private List<Client> Clients = new List<Client>();
        private ServerDataPackage NextFrame;
        private Dictionary<int, PackageInterface[]> packagesForNextFrame = new Dictionary<int, PackageInterface[]>();
        private GameStructure GameStructure;
        private GameEngine GameEngine;
        private LogWriterConsole Logger = new LogWriterConsole();
        private int SleepTimeMillisecondsBetweenTicks { get { return 1000 / Tickrate; } set { } }
        private int TeardownDelaySeconds = 60;



        public Game(GameNetwork Network, int NeededNumberOfPlayersForGameToStart)
        {
            GameID = new Random().Next();
            Log("Initialising a new Game with " + Convert.ToString(NeededNumberOfPlayersForGameToStart) + " Players");
            this.Network = Network;
            this.Network.ClientLost += OnClientLost;
            GameState = GameStates.Initializing;
            GameStructure = new GameStructure(NeededNumberOfPlayersForGameToStart);
            GameEngine = new GameEngine(GameStructure);
            this.NumberOfPlayers = NeededNumberOfPlayersForGameToStart;
            GameEngine.TeamScoredHandler += OnTeamScored;
            GameEngine.OnGameFinished += HandleFinishedGame;
        }

        public void StartGame(object caller)
        {
            GameFinished += (caller as GamesManager).OnGameFinished;
            GameState = GameStates.Running;

            foreach (Client client in Clients)
                SendServerInitResponse(client);

            Log("Game started");
            GameEngine.PauseBall(3000);
            GameLoop();
            Log("has exited it's game loop. Therefore the Thread started this game should finish as well");

        }

        private void GameLoop()
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();

            Log("Entering the main loop");

            while (GameState == GameStates.Running)
            {
                GetNetworkDataForNextFrame();
                ServerPackage = NextFramePackage();
                Network.BroadcastFramesToClients(ServerPackage);
                Thread.Sleep(SleepTimeMillisecondsBetweenTicks);
            }

            Log("Exiting the main loop");
        }

        public bool AddClient(NetworkConnection client, int[] playerTeamWish)
        {
            if (GameStructure.MissingPlayersCount < playerTeamWish.Length)
                return false;

            Client newClient = new Client(GameStructure, client.ClientSession.SessionID);
            Clients.Add(newClient);
           

            int maxTeamSize = GameStructure.maxPlayers / 2;

            foreach (int team in playerTeamWish)
            {
                int index = GameStructure.PlayersCount;
                int teamIndex = GameStructure.GameTeams[team].PlayerList.Count;
                int teamOpenSpots = maxTeamSize - teamIndex;

                if (teamOpenSpots < 1)
                    continue;

                Player newPlayer = new Player(index, team, GameInitializers.GetPlayerX(team, teamIndex));
                newPlayer.Height = GameInitializers.GetPlayerHeight(maxTeamSize);
                newPlayer.PositionY = GameInitializers.GetPlayerY(maxTeamSize);

                newClient.AddPlayer(newPlayer);
                GameStructure.AddPlayer(newPlayer);
            }

            if (GameStructure.PlayersCount == NumberOfPlayers)
                GameState = GameStates.Ready;

            Network.AddClientConnection(client);
            return true;
        }

        public bool RejoinClient(NetworkConnection client)
        {
            bool couldRejoin = false;
            // rejoin is only justified when the client connection died. If it's still connection we want to avoid rejoin since this would get messy
            bool rejoinJustified = !Network.ClientStillConnected(client.ClientSession.SessionID);
            Client correctClient = null;
            foreach (Client c in Clients)
            {
                if (c.SessionID == client.ClientSession.SessionID)
                {
                    correctClient = c;
                }
            }
            Log("Client rejoin was requested: " + client.ClientSession.SessionID.ToString());
            if (GameState != GameStates.Finished && rejoinJustified && correctClient != null)
            {
                ServerInitializeGameResponse packet = new ServerInitializeGameResponse();
                packet.m_field = GameStructure.GameField;
                packet.m_ball = GameStructure.Ball;
                packet.m_players = new Player[GameStructure.PlayersCount];
                Array.Copy(GameStructure.GetAllPlayers(), packet.m_players, GameStructure.PlayersCount);

                foreach (Player p in packet.m_players)
                {
                    foreach (Player player in correctClient.Players)
                    {
                        p.Controllable = (player.ID == p.ID);
                    }
                }
                Log("Rejoin succeeded sending the ServerSessionResponse with GameReconnect Flag set to true to the Client");
                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = client.ClientSession.SessionID;
                response.GameReconnect = true;
                client.SendTCP(response);
                Log("Rejoin succeeded sending the ServerInitializeGameResponse to the Client");
                client.SendTCP(packet);
                couldRejoin = true;
                Log("Since Client just rejoined he isn't aware of the current score, therefore sending a score package");
                client.SendTCP(GenerateScorePackage());
                Network.AddClientConnection(client);

            }
            return couldRejoin;

        }

        public bool AddObserver(NetworkConnection connection)
        {
            bool added = false;
            if (GameState != GameStates.Finished)
            {
                ServerInitializeGameResponse packet = new ServerInitializeGameResponse();
                packet.m_field = GameStructure.GameField;
                packet.m_ball = GameStructure.Ball;
                packet.m_players = new Player[GameStructure.PlayersCount];
                Array.Copy(GameStructure.GetAllPlayers(), packet.m_players, GameStructure.PlayersCount);
                foreach (Player player in packet.m_players)
                    player.Controllable = false;
                connection.SendTCP(packet);
                Network.AddObserver(connection);
                added = true;
            }
            return added;
        }

        ~Game()
        {
            // IDisposable ? Dispose 
            Log("Destructor for Game with Game ID: " + GameID.ToString() + " has been called. Game is being cleaned up nicely");
            Network.Close();
        }

        public void BroadcastStartGamePackage(ServerMatchmakingStatusResponse GameFoundPackage)
        {
            Network.BroadcastStartGamePackage(GameFoundPackage);
        }

        private void OnTeamScored(object sender, EventArgs e)
        {
            Log("Team scored\t Team 1: " + GameStructure.GameTeams[0].score.ToString() + "\tTeam 2: " + GameStructure.GameTeams[1].score.ToString());
            Network.BroadcastScore(GenerateScorePackage());
        }

        private ServerGameControlPackage GenerateScorePackage()
        {
            ServerGameControlPackage scoreData = new ServerGameControlPackage();
            scoreData.Score.Team1 = GameStructure.GameTeams[0].score;
            scoreData.Score.Team2 = GameStructure.GameTeams[1].score;
            return scoreData;
        }

        private void GameFinishedCleanup()
        {
            Log("This was the final point");
            Log("Final Score: Team Red: " + GameStructure.GameTeams[0].score.ToString() + "\tTeam Blue: " + GameStructure.GameTeams[1].score.ToString());
            Log("Game finished");
            Log("Sending final Game finished to the Clients");
            BroadcastGameFinishedPackage();
            Log("Game Finished Package has been sent waiting " + TeardownDelaySeconds.ToString() + " seconds before tearing down the network");
            Thread.Sleep(TeardownDelaySeconds * 1000);
            Logger.NetworkLog("Tearing Down Network");
            Network.Close();
            GameState = GameStates.Finished;
            GameFinished?.Invoke(this, EventArgs.Empty);
        }

        private void BroadcastGameFinishedPackage()
        {
            ServerGameControlPackage gameFinishedPackage = GameFinishedPackage();
            Network.BroadcastGenericTCPPackage(gameFinishedPackage);
        }

        private ServerGameControlPackage GameFinishedPackage()
        {
            ServerGameControlPackage gameFinishedPackage = new ServerGameControlPackage();

            gameFinishedPackage.Command = ServerControls.GameFinished;
            gameFinishedPackage.Score.Team1 = GameStructure.GameTeams[0].score;
            gameFinishedPackage.Score.Team2 = GameStructure.GameTeams[1].score;
            if (GameStructure.GameTeams[0].score > GameStructure.GameTeams[1].score)
                gameFinishedPackage.Winner = Teams.Team1;
            else
                gameFinishedPackage.Winner = Teams.Team2;

            return gameFinishedPackage;
        }

        private void HandleFinishedGame(object sender, EventArgs e)
        {
            GameFinishedCleanup();
        }

        private void OnClientLost(object sender, EventArgs e)
        {
            Log("Client Lost Event called");
            bool gameOver = true;
            List<Player> DisconnectedPlayers = new List<Player>();

            foreach (Client c in Clients)
            {
                Log("Iterating over clients");
                if (Network.DiedSessions.Contains(c.SessionID))
                {
                    Log("Adding Client to disconnected Clients, Session ID:" + c.SessionID);
                    DisconnectedPlayers.AddRange(c.Players);
                }
                    
            }
                

            foreach(GameStructure.GameTeam team in GameStructure.GameTeams.Values)
            {
                Log("Iterating over players");
                foreach (Player p in team.PlayerList)
                {
                    if (!DisconnectedPlayers.Contains(p))
                    {
                        Log("Player not in disconnected Players");
                        gameOver = false;
                        break;
                    }

                }
            }

            if (gameOver)
            {
                Log("Calling Game finished cleanup because Client Lost was the last client of the game");
                GameFinishedCleanup();
            }
                
        }

        public override string ToString()
        {
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

        private void SendServerInitResponse(Client client)
        {
            ServerInitializeGameResponse packet = new ServerInitializeGameResponse();
            packet.m_field = new GameField();
            packet.m_ball = new Ball();

            packet.m_players = new Player[GameStructure.PlayersCount];
            Array.Copy(GameStructure.GetAllPlayers(), packet.m_players, GameStructure.PlayersCount);

            foreach(Player player in packet.m_players)
                player.Controllable = client.Players.Contains(player);

            Network.SendTCPPackageToClient(packet, client.SessionID);
        }
        
        private void GetNetworkDataForNextFrame()
        {
            packagesForNextFrame = Network.GrabAllNetworkDataForNextFrame();
        }
        
        private PackageInterface[] GetClientData(int sessionID)
        {
            List<PackageInterface> clientRelatedPackets = new List<PackageInterface>();

            if (!packagesForNextFrame.ContainsKey(sessionID))
                return new PackageInterface[0];

            foreach (PackageInterface packet in packagesForNextFrame[sessionID])
                clientRelatedPackets.Add(packet);
            
            return clientRelatedPackets.ToArray();
        }
        
        private ServerDataPackage NextFramePackage()
        {
            NextFrame = new ServerDataPackage();

            foreach (KeyValuePair<int, GameStructure.GameTeam> Team in GameStructure.GameTeams)
            {
                foreach (Player player in Team.Value.PlayerList)
                {
                    GameEngine.SetPlayerMovement(player, GetLastPlayerMovement(player.ID));
                    NextFrame.Players.Add(player);
                }
            }

            GameEngine.CalculateFrame(SleepTimeMillisecondsBetweenTicks);
            NextFrame.Ball.PositionX = GameStructure.Ball.PositionX;
            NextFrame.Ball.PositionY = GameStructure.Ball.PositionY;

            return NextFrame;
        }
        
        private ClientControls GetLastPlayerControl(int playerID)
        {
            List<ClientControlPackage> cc = new List<ClientControlPackage>();
            foreach (Client c in Clients)
            {
                PackageInterface[] ps = GetClientData(c.SessionID);
                foreach(PackageInterface p in ps)
                {
                    if (p == null || p.PackageType != PackageType.ClientControl)
                        continue;
                    cc.Add((ClientControlPackage)p);
                }
            }
            return cc[cc.Count - 1].ControlInput;
        }

        private void Log(string text)
        {
            Logger.GameLog("ID: " + GameID.ToString() + "  " + text);
        }

        private ClientMovement GetLastPlayerMovement(int playerID)
        {
            List<PlayerMovementPackage> cc = new List<PlayerMovementPackage>();
            foreach (Client c in Clients)
            {
                PackageInterface[] ps = GetClientData(c.SessionID);
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
