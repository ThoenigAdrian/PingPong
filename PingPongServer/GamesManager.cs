﻿using GameLogicLibrary;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using PingPongServer.ServerGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private UDPConnection MasterUDPSocket;
        private bool shutdownGameManager = false;

        public GamesManager(UDPConnection MasterUDPSocket)
        {
            this.MasterUDPSocket = MasterUDPSocket;
        }

        public void Run()
        {
            StartGameManagerThread();
        }

        public void Stop()
        {
            Logger.GamesManagerLog("GamesManager Stop has been requested");
        }

        private void StartGame(object game)
        {
            Game gameToBeStarted = (Game)game;
            Logger.GamesManagerLog("Found a Game which is ready to start ID: " + gameToBeStarted.GameID);
            for (int counter = 0; counter < 5; counter++)
            {
                ServerMatchmakingStatusResponse GameFoundPackage = new ServerMatchmakingStatusResponse();
                GameFoundPackage.GameFound = true;
                GameFoundPackage.Status = "Game will start in " + counter.ToString() + "seconds ...";
                GameFoundPackage.Error = false;
                Thread.Sleep(1);
                
            }
            ThreadPool.QueueUserWorkItem(gameToBeStarted.StartGame, this);
            RunningGames.Add(gameToBeStarted);
        }

        public void OnGameFinished(object sender, EventArgs e)
        {
            RemoveFinishedGames();
        }

        // Return true if client could rejoin the game
        public bool RejoinClientToGame(NetworkConnection conn)
        {
            bool couldRejoin = false;

            foreach (Game game in RunningGames.Entries)
            {
                if (game.Network.DiedSessions.Contains(conn.ClientSession.SessionID))
                {
                    game.RejoinClient(conn);
                    
                    couldRejoin = true;
                    break;
                }
            }
            return couldRejoin;
        }

        private void ManageGames()
        {
            while (!shutdownGameManager)
            {
                RemoveFinishedGames();
                Thread.Sleep(10);
            }
        }

        public int PlayersCurrentlyInGames()
        {
            int playersCurrentlyInGame = 0;
            foreach (Game game in RunningGames.Entries)
            {
                playersCurrentlyInGame += game.maxPlayers; // Change this to active players in Game max_players - disconnected ? Or is this already accurate enough ? 
            }
            return playersCurrentlyInGame;
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

        private void StartGamesWhichAreReady()
        {
            foreach(Game readyGame in GamesReadyToBeStarted.Entries)
            {
                Thread GameStarting = new Thread(new ParameterizedThreadStart(StartGame));
                GameStarting.Start(readyGame);
                GamesReadyToBeStarted.Remove(readyGame);
            }
            
            
        }

        public void OnMatchmadeGameFound(object sender, MatchmakingManager.MatchData match)
        {
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket);
            Game newGame = new Game(newGameNetwork, match.MaxPlayerCount);
            newGame.GameID = new Random().Next();

            foreach (MatchmakingManager.ClientData client in match.Clients)
            {
                newGame.AddClient(client.m_clientConnection, client.m_request.GetPlayerPlacements());
            }

            GamesReadyToBeStarted.Add(newGame);
            
        }

        private void StartGameManagerThread()
        {
            Logger.GamesManagerLog("Starting Thread which takes Care of the Games");
            Thread ManageGamesThread = new Thread(new ThreadStart(ManageGames));
            ManageGamesThread.Start();
        }
    }
}
