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
        Stopwatch watch = new Stopwatch();
        public int GamesCount { get { return Games.Count; } set { } }
        bool StopExecutor = false;

        public GamesExecutor(int ID, UnleashSignal frameTimer)
        {
            Logger.GamesExecutorID = ID;
            Logger.GamesExecutorLog("Initilaising Games Executor");
            FrameTimer = frameTimer;
        }

        public void Run()
        {
            watch.Start();
            while(!StopExecutor)
            {
                FrameTimer.Lock();
                watch.Stop();
                int elapsedTime = (int)watch.ElapsedMilliseconds;
                watch.Reset();
                watch.Start();
                foreach(Game game in Games.Entries)
                {
                    if (game.GameState == GameLogicLibrary.GameStates.Finished)
                        Games.Remove(game);
                    else
                        game.CalculateNextFrame(elapsedTime);
                }
            }
        }

        public void Stop()
        {
            StopExecutor = true;
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
    }
}
