using System;
using System.Collections.Generic;

using PingPongServer.ServerGame.ServerGameObjects;

using GameLogicLibrary;
using GameLogicLibrary.GameObjects;

using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using XSLibrary.Utility;
using System.Diagnostics;

namespace PingPongServer.ServerGame
{
    public class Game
    {
        public GameStates GameState { get; private set; }
        public int GameID = 0;
        public int NumberOfPlayers;

        private HashSet<Player> DisconnectedPlayers = new HashSet<Player>();
        private GameNetwork Network;
        private List<Client> Clients = new List<Client>();
        private ServerDataPackage NextFrame;
        private Dictionary<int, PackageInterface[]> packagesForNextFrame = new Dictionary<int, PackageInterface[]>();
        private GameStructure GameStructure;
        private GameEngine GameEngine;
        private LogWriterConsole Logger = new LogWriterConsole();
        private const int TeardownDelaySeconds = 60;
        private object GameStateLock = new object();
        private OneShotTimer CloseTimer = new OneShotTimer(TeardownDelaySeconds * 1000 * 1000, false);
        private UniqueIDGenerator GamesIDGenerator;
        private Stopwatch FrameDistanceWatch = new Stopwatch();

        public Game(GameNetwork Network, int NeededNumberOfPlayersForGameToStart, UniqueIDGenerator gamesIDGenerator)
        {
            GamesIDGenerator = gamesIDGenerator;
            GameID = GamesIDGenerator.GetID();
            Logger.GameID = GameID;
            Logger.GameLog("Initialising a new Game with " + Convert.ToString(NeededNumberOfPlayersForGameToStart) + " Players");
            this.Network = Network;
            this.Network.ClientLost += OnClientLost;
            GameState = GameStates.Initializing;
            GameStructure = new GameStructure(NeededNumberOfPlayersForGameToStart);
            GameEngine = new GameEngine(GameStructure);
            NumberOfPlayers = NeededNumberOfPlayersForGameToStart;
            GameEngine.TeamScoredHandler += OnTeamScored;
            GameEngine.OnGameFinished += HandleFinishedGame;
        }

        public void StartGame()
        {
            lock(GameStateLock)
            {
                if (GameState == GameStates.Ready)
                    GameState = GameStates.Running;
            }

            foreach (Client client in Clients)
                SendServerInitResponse(client);

            Logger.GameLog("Game started");
            GameEngine.PauseBall(3000);

        }

        public void CalculateNextFrame()
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            GetNetworkDataForNextFrame();
            long timePassedInTenthOfAMilliseconds = (FrameDistanceWatch.ElapsedTicks * 10000) / Stopwatch.Frequency;
            FrameDistanceWatch.Restart();
            ServerPackage = NextFramePackage(timePassedInTenthOfAMilliseconds);
            Network.BroadcastFramesToClients(ServerPackage);
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
            {
                GameState = GameStates.Ready;
            }
                

            Network.AddClientConnection(client);
            return true;
        }

        public bool RejoinClient(NetworkConnection client)
        {
            bool couldRejoin = false;
            // rejoin is only justified if the client connection died. If it's still connected we want to avoid rejoin since this would get messy
            bool rejoinJustified = !Network.ClientStillConnected(client.ClientSession.SessionID);
            Client correctClient = null;
            foreach (Client c in Clients)
            {
                if (c.SessionID == client.ClientSession.SessionID)
                {
                    correctClient = c;
                }
            }
            Logger.GameLog("Client rejoin was requested: " + client.ClientSession.SessionID.ToString());
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
                Logger.GameLog("Rejoin succeeded sending the ServerSessionResponse with GameReconnect Flag set to true to the Client");
                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = client.ClientSession.SessionID;
                response.GameReconnect = true;
                client.SendTCP(response);
                Logger.GameLog("Rejoin succeeded sending the ServerInitializeGameResponse to the Client");
                client.SendTCP(packet);
                couldRejoin = true;
                Logger.GameLog("Since Client just rejoined he isn't aware of the current score, therefore sending a score package");
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
                connection.SendTCP(GenerateScorePackage());
                Network.AddObserver(connection);
                added = true;
            }
            return added;
        }

        ~Game()
        {
            // IDisposable ? Dispose 
            Logger.GameLog("Destructor for Game with Game ID: " + GameID.ToString() + " has been called. Game is being cleaned up nicely");
            Network.Close();
            GamesIDGenerator.FreeID(GameID);
        }

        public void BroadcastStartGamePackage(ServerMatchmakingStatusResponse GameFoundPackage)
        {
            Network.BroadcastStartGamePackage(GameFoundPackage);
        }

        private void OnTeamScored(object sender, EventArgs e)
        {
            // Logger.GameLog("Team scored\t Team 1: " + GameStructure.GameTeams[0].score.ToString() + "\tTeam 2: " + GameStructure.GameTeams[1].score.ToString());
            Network.BroadcastScore(GenerateScorePackage());
        }

        private ServerGameControlPackage GenerateScorePackage()
        {
            ServerGameControlPackage scoreData = new ServerGameControlPackage();
            scoreData.Score.Team1 = GameStructure.GameTeams[0].score;
            scoreData.Score.Team2 = GameStructure.GameTeams[1].score;
            return scoreData;
        }

        public void StopGame()
        {
            lock (GameStateLock)
            {
                GameState = GameStates.Aborted;
            }
            CloseTimer.Restart();

            Logger.GameLog("This was the final point");
            Logger.GameLog("Final Score: Team Red: " + GameStructure.GameTeams[0].score.ToString() + "\tTeam Blue: " + GameStructure.GameTeams[1].score.ToString());
            Logger.GameLog("Game finished");
            Logger.GameLog("Sending final Game finished to the Clients");
            BroadcastGameFinishedPackage();
            Logger.GameLog("Game Finished Package has been sent waiting " + TeardownDelaySeconds.ToString() + " seconds before tearing down the network");
        }

        public void FinalizeAbortedGame()
        {
            if (CloseTimer)
            {
                Network.Close();
                GameState = GameStates.Finished;
            }
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
            StopGame();
        }

        private void OnClientLost(object sender, EventArgs e)
        {
            Logger.GameLog("Client Lost Event called");

            foreach (Client client in Clients)
            {
                Logger.GameLog("Iterating over clients");
                if (Network.DeadSessions.Contains(client.SessionID))
                {
                    Logger.GameLog("Adding Client to disconnected Clients, Session ID:" + client.SessionID);
                    DisconnectedPlayers.UnionWith(client.Players);
                }
            }
            if(AllPlayersOfOneTeamDisconnected())
            {
                Logger.GameLog("Calling Game finished cleanup because Client Lost was the last client of the team");
                StopGame();
            }

        }

        private bool AllPlayersOfOneTeamDisconnected()
        {
            bool gameOver = true;
            foreach (GameStructure.GameTeam team in GameStructure.GameTeams.Values)
            {
                Logger.GameLog("Iterating over players");
                foreach (Player player in team.PlayerList)
                {
                    if (!DisconnectedPlayers.Contains(player))
                    {
                        Logger.GameLog("Player not in disconnected Players");
                        gameOver = false;
                        break;
                    }
                }
            }

            return gameOver;
        }

        public override string ToString()
        {
            string str = "Game [" + GameID + "] \n\t" + "Players : " + GameStructure.PlayersCount.ToString();
            foreach (KeyValuePair<int, GameStructure.GameTeam> team in GameStructure.GameTeams)
            {
                str += "\n\tTeam :" + team.Key;
                str += team.Value.ToString();
            }
            foreach(Client client in Clients)
            {
                str += client.ToString();
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
        
        private ServerDataPackage NextFramePackage(long timePassedInTenthOfAMilliseconds)
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

            GameEngine.CalculateFrame(timePassedInTenthOfAMilliseconds);
            NextFrame.Ball.PositionX = GameStructure.Ball.PositionX;
            NextFrame.Ball.PositionY = GameStructure.Ball.PositionY;

            return NextFrame;
        }

        private ClientMovement GetLastPlayerMovement(int playerID)
        {
            Client clientMatchingPlayerID = null;
            List<PlayerMovementPackage> allPlayerMovementPackagesSinceLastFrame = new List<PlayerMovementPackage>();
            foreach (Client clientCandidate in Clients)
            {
                foreach (Player player in clientCandidate.Players)
                {
                    if(player.ID == playerID)
                        clientMatchingPlayerID = clientCandidate;
                }
            }
            PackageInterface[] allClientPackages = GetClientData(clientMatchingPlayerID.SessionID);
            foreach (PackageInterface receivedPackage in allClientPackages)
            {
                if (receivedPackage == null || receivedPackage.PackageType != PackageType.ClientPlayerMovement)
                    continue;   // Ignore irrelevant packages either it's null because there was something invalid or it's not a ClientPlayerMovement Package but something else instead

                PlayerMovementPackage movementPackage = receivedPackage as PlayerMovementPackage;

                if(movementPackage.PlayerID == playerID)
                    allPlayerMovementPackagesSinceLastFrame.Add(movementPackage);
            }
            if (allPlayerMovementPackagesSinceLastFrame.Count == 0)
                return ClientMovement.NoInput;
            return allPlayerMovementPackagesSinceLastFrame[allPlayerMovementPackagesSinceLastFrame.Count - 1].PlayerMovement;
        }              
                        
    }
}
