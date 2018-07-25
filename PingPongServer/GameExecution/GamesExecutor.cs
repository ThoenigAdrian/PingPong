using NetworkLibrary.Utility;
using System;
using PingPongServer.ServerGame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using XSLibrary.ThreadSafety.Containers;
using System.Threading;

namespace PingPongServer.GameExecution
{
    class GamesExecutor
    {
        LogWriterConsole Logger = new LogWriterConsole();
        SafeList<Game> Games = new SafeList<Game>();
        DateTime TimeOfPreviousFrame;
        DateTime TimeOfCurrentFrame;
        AutoResetEvent FrameTimer;
        Stopwatch watch = new Stopwatch();
        public int ID;
        public int GamesCount { get { return Games.Count; } set { } }
        bool StopExecutor = false;

        public GamesExecutor(int ID, AutoResetEvent frameTimer)
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
                FrameTimer.WaitOne();
                watch.Stop();
                int elapsedTime = (int)watch.ElapsedMilliseconds;
                Logger.GamesExecutorLog("Time passed since last frame: " + elapsedTime.ToString());
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
