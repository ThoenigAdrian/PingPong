using GameLogicLibrary;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using PingPongServer.ServerGame;
using System.Diagnostics;
using XSLibrary.ThreadSafety.Containers;
using XSLibrary.ThreadSafety.Locks;

namespace PingPongServer.GameExecution
{
    class GamesExecutor
    {
        LogWriterConsole Logger = new LogWriterConsole();
        SafeList<Game> Games = new SafeList<Game>();
        UnleashSignal FrameTimer;
        Stopwatch FrameRateReportWatch = new Stopwatch();
        Stopwatch FrameDistanceWatch = new Stopwatch();
        public int GamesCount { get { return Games.Count; } set { } }
        public uint ReportFrameRateIntervalInSeconds = 30;
        public uint FramesExecuted = 0;
        bool StopExecutor = false;

        public GamesExecutor(int ID, UnleashSignal frameTimer)
        {
            Logger.GamesExecutorID = ID;
            Logger.GamesExecutorLog("Initilaising Games Executor");
            FrameTimer = frameTimer;
        }

        public void Run()
        {
            FrameRateReportWatch.Start();
            FrameDistanceWatch.Start();
            while (!StopExecutor)
            {
                FrameTimer.Lock();
                FrameDistanceWatch.Stop();
                foreach(Game game in Games.Entries)
                {
                    if (game.GameState == GameStates.Finished)
                        Games.Remove(game);
                    else if(game.GameState == GameStates.Running)
                        game.CalculateNextFrame(((FrameDistanceWatch.ElapsedTicks*10000)/Stopwatch.Frequency));
                        
                }
                FramesExecuted++;
                FrameDistanceWatch.Restart();
                ReportFrameRate();
            }
        }

        public bool AddGame(Game game)
        {
            if (StopExecutor)
            {
                return false;
            }
            else
            {
                Games.Add(game);
                return true;
            }

        }

        public bool AddObserversToGame(NetworkConnection observerConnection)
        {
            foreach (Game game in Games.Entries)
            {
                if (game.AddObserver(observerConnection))
                    return true;
            }
            return false;
        }

        private void ReportFrameRate()
        {
            if (FrameRateReportWatch.ElapsedMilliseconds >= ReportFrameRateIntervalInSeconds * 1000)
            {
                uint FPS = FramesExecuted / ReportFrameRateIntervalInSeconds;
                Logger.GamesExecutorLog("Frame Rate: " + FPS.ToString() + " FPS");
                FramesExecuted = 0;
                FrameRateReportWatch.Restart();
            }   
        }

        private void RemoveFinishedGames()
        {
            foreach (Game game in Games.Entries)
            {
                if (game.GameState == GameStates.Finished)
                {
                    Logger.GamesManagerLog("Found a finished Game removing it now\n " + game.ToString());;
                    Games.Remove(game);
                }
            }
        }

        public void Stop()
        {
            StopAllGames();
            StopExecutor = true;
        }

        private void StopAllGames()
        {
            foreach(Game game in Games.Entries)
            {
                game.StopGame();
                Games.Remove(game);
            }
        }

        public bool RejoinClientToGame(NetworkConnection clientWantsRejoin)
        {
            foreach (Game game in Games.Entries)
            {
                if(game.RejoinClient(clientWantsRejoin))
                {
                    Logger.GamesExecutorLog("Could successfully rejoin the client: " + clientWantsRejoin.RemoteEndPoint.ToString() 
                        + " with Session ID: " + clientWantsRejoin.ClientSession.SessionID + "to the Game with ID: " + game.GameID.ToString());
                    return true;
                }
            }
            return false;
        }

        public int PlayersCurrentlyInGames()
        {
            int playersCurrentlyInGame = 0;
            foreach (Game game in Games.Entries)
                playersCurrentlyInGame += game.NumberOfPlayers;
            return playersCurrentlyInGame;

        }
    }
}
