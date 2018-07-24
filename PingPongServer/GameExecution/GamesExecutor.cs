using NetworkLibrary.Utility;
using System;
using PingPongServer.ServerGame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer.GameExecution
{
    class GamesExecutor
    {
        LogWriterConsole Logger = new LogWriterConsole();
        SafeList<Game> Games = new SafeList<Game>();
        DateTime TimeOfPreviousFrame;
        DateTime TimeOfCurrentFrame;
        SingleFireWaitCondition FrameTimer;
        Stopwatch watch = new Stopwatch();
        public int ID;
        public int GamesCount { get { return Games.Count; } set { } }
        bool StopExecutor = false;

        public GamesExecutor(int ID, SingleFireWaitCondition frameTimer)
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
                FrameTimer.Wait();
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
