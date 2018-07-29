using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using PingPongServer.GameExecution;
using PingPongServer.ServerGame;
using System.Threading;
using XSLibrary.Network.Connections;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer
{
    public class GamesManager
    {
        private LogWriterConsole Logger { get; set; } = new LogWriterConsole();
        private SafeList<Game> GamesReadyToBeStarted = new SafeList<Game>();
        private SafeList<Game> RunningGames = new SafeList<Game>();
        private SafeList<NetworkConnection> WaitingObservers = new SafeList<NetworkConnection>();
        private UDPConnection MasterUDPSocket;
        private UniqueIDGenerator GamesIDGenerator = new UniqueIDGenerator();
        private UniqueIDGenerator SessionManager;
        private GamesExecutorLoadBalancer LoadBalancer = new GamesExecutorLoadBalancer();
        private bool shutdownGameManager = false;
        

        public GamesManager(UDPConnection MasterUDPSocket, UniqueIDGenerator sessionManager)
        {
            this.MasterUDPSocket = MasterUDPSocket;
            SessionManager = sessionManager;
        }

        public void Run()
        {
            StartGameManagerThread();
        }

        private void StartGameManagerThread()
        {
            Logger.GamesManagerLog("Starting Thread which takes Care of the Games");
            Thread ManageGamesThread = new Thread(new ThreadStart(ManageGames));
            ManageGamesThread.Name = "Games Manager";
            ManageGamesThread.Start();
        }

        public void Stop()
        {
            Logger.GamesManagerLog("GamesManager Stop has been requested");
            shutdownGameManager = true;
            LoadBalancer.Dispose();
            Logger.GamesManagerLog("GamesManager stopped");
        }

        private void StartGame(object game)
        {
            Game gameToBeStarted = (Game)game;
            Logger.GamesManagerLog("Found a Game which is ready to start ID: " + gameToBeStarted.GameID);
            for (int counter = 5; counter > 0; counter--)
            {
                ServerMatchmakingStatusResponse GameFoundPackage = new ServerMatchmakingStatusResponse();
                GameFoundPackage.GameFound = true;
                GameFoundPackage.Status = "Game will start in " + counter.ToString() + " seconds...";
                GameFoundPackage.Error = false;
                gameToBeStarted.BroadcastStartGamePackage(GameFoundPackage);
                Thread.Sleep(1000);                
            }
            LoadBalancer.AddGame(gameToBeStarted);
        }

        // Return true if client could rejoin the game
        public bool RejoinClientToGame(NetworkConnection conn)
        {
            Logger.GamesManagerLog("Trying to rejoin a client with the session: " + conn.ClientSession.SessionID + " to the game.");
            if(LoadBalancer.RejoinClientToGame(conn))
            {
                return true;
            }
            return false;
        }

        private void ManageGames()
        {
            while (!shutdownGameManager)
            {
                StartGamesWhichAreReady();
                AddObserversToGame();
                Thread.Sleep(100);
            }
        }

        public int PlayersCurrentlyInGames()
        {
            return LoadBalancer.PlayersCurrentlyInGames();
        }

        public void AddObserver(NetworkConnection observerConnection)
        {
            WaitingObservers.Add(observerConnection);
        }

        private void AddObserversToGame()
        {
            foreach (NetworkConnection observer in WaitingObservers.Entries)
            {
                if (LoadBalancer.AddObserver(observer))
                {
                    WaitingObservers.Remove(observer);
                }
            }
        }

        private void StartGamesWhichAreReady()
        {
            foreach(Game readyGame in GamesReadyToBeStarted.Entries)
            {
                Thread GameStarting = new Thread(new ParameterizedThreadStart(StartGame));
                GameStarting.Name = "Temporary StartGame Thread ID: " + readyGame.GameID;
                GameStarting.Start(readyGame);
                GamesReadyToBeStarted.Remove(readyGame);
            }
            
        }

        public void OnMatchmadeGameFound(object sender, MatchmakingManager.MatchData match)
        {
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket, SessionManager);
            Game newGame = new Game(newGameNetwork, match.MaxPlayerCount, GamesIDGenerator);

            foreach (MatchmakingManager.ClientData client in match.Clients)
            {
                newGame.AddClient(client.m_clientConnection, client.m_request.GetPlayerPlacements());
            }

            GamesReadyToBeStarted.Add(newGame);
            
        }
    }
}
