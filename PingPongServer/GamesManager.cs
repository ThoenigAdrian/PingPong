using NetworkLibrary.Utility;
using PingPongServer.ServerGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer
{
    public class GamesManager
    {
        private LogWriterConsole Logger { get; set; } = new LogWriterConsole();
        private SafeList<Game> RunningGames = new SafeList<Game>();
        public void Run()
        {
            StartGameManagerThread();
        }


        private void ManageGames()
        {
            while (!m_stopServer)
            {
                ServeClientGameRequests();
                
                Thread.Sleep(10);
            }
        }

        private void RemoveFinishedGames()
        {
            foreach (Game game in RunningGames.Entries)
            {
                if (game.GameState == GameStates.Finished)
                {
                    Logger.GamesManagerLog("Found a finished Game removing it now\n " + game.ToString());
                    RunningGames.Remove(game);
                }
            }
        }

        

        private void StartGameManagerThread()
        {
            Logger.Log("Starting Thread which takes Care of the Games");
            Thread ManageGamesThread = new Thread(new ThreadStart(ManageGames));
            ManageGamesThread.Start();
        }
    }
}
