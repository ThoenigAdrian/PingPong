using NetworkLibrary.Utility;
using System;
using PingPongServer.ServerGame;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingPongServer.GameExecution
{
    class GamesExecutor
    {
        LogWriterConsole Logger = new LogWriterConsole();
        List<Game> Games = new List<Game>();
        public int ID;
        public int GamesCount;

        public GamesExecutor(int ID)
        {
            Logger.GamesExecutorID = ID;
            Logger.GamesExecutorLog("Initilaising Games Executor");
        }

        public void AddGame(Game game)
        {
            Games.Add(game);
        }
    }
}
