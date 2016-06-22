﻿using System;
using System.Threading;
using System.Collections.Generic;

using GameLogicLibrary;
using GameLogicLibrary.GameObjects;

using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;

namespace PingPongServer
{
    public class ServerGame
    {
        public GameStates GameState { get; private set; }
        List<Client> Clients = new List<Client>();

        private int maxPlayers;
        private int NeededNumberOfPlayersForGameToStart;

        public GameNetwork Network;
        public ServerDataPackage NextFrame;
        public Dictionary<int, PackageInterface[]> packagesForNextFrame = new Dictionary<int, PackageInterface[]>();
        public GameStructure GameStructure;
        private LogWriterConsole Logger = new LogWriterConsole();



        public ServerGame(GameNetwork Network, int NeededNumberOfPlayersForGameToStart)
        {
            Logger.GameLog("Initialising a new Game with " + Convert.ToString(NeededNumberOfPlayersForGameToStart) + "");
            this.Network = Network;            
            GameState = GameStates.Initializing;
            GameStructure = new GameStructure(NeededNumberOfPlayersForGameToStart);
            this.NeededNumberOfPlayersForGameToStart = NeededNumberOfPlayersForGameToStart;
            maxPlayers = NeededNumberOfPlayersForGameToStart;            

        }

        public override string ToString()
        {
            // Build a nice custom string in the future
            return "Game with " + GameStructure.PlayersCount.ToString() + " Players " + "and Score" + GameStructure.GameTeams.ToString();
        }

        public void StartGame(object justToMatchSignatureForThreadPool)
        {
            ServerDataPackage ServerPackage = new ServerDataPackage();
            GameState = GameStates.Running;
            Logger.GameLog("Game started");
            int i = 0;
            while (GameState == GameStates.Running)
            {
                i++;
                if (i == 10000)
                    break;
                GetAllThe();
                ServerPackage = CalculateFrame();
                Network.BroadcastFramesToClients(ServerPackage);
                Thread.Sleep(5);
            }
            Logger.GameLog("Game finished");
            Logger.NetworkLog("Tearing Down Network");
            Network.Close();
            GameState = GameStates.Finished; // Move to somewhere else game logic e.g score reached
        }

        public bool AddClient(NetworkConnection client, int[] playerTeamWish)
        {
            if (GameStructure.MissingPlayers < playerTeamWish.Length)
                return false; 

            Network.AddClientConnection(client);            
            Client newClient = new Client(GameStructure, client.ClientSession.SessionID);
            
            for (int index = 0; index < playerTeamWish.Length; index++)
            {
                float playerPosition = 0;
                                
                if (playerTeamWish[index] == 0 && GameStructure.maxPlayers / 2 - GameStructure.GameTeams[playerTeamWish[index]].Count >= 1)
                    playerPosition = GameInitializers.PLAYER_1_X + GameStructure.GameTeams.Count * 30F;
                else if (playerTeamWish[index] == 1 && GameStructure.maxPlayers / 2 - GameStructure.GameTeams[playerTeamWish[index]].Count>= 1)
                    playerPosition = GameInitializers.PLAYER_2_X - GameStructure.GameTeams.Count * 30F;
                                

                Player newPlayer = new Player(GameStructure.PlayersCount, GameStructure.GetFreeTeam(), playerPosition);

                newClient.AddPlayer(newPlayer, GameStructure);
            }
            ServerInitializeGameResponse packet = new ServerInitializeGameResponse();
            packet.m_field = new GameField();
            packet.m_ball = new Ball();
            packet.m_players = new Player[2];
            packet.m_players[0] = new Player(0, 0, GameInitializers.PLAYER_1_X);
            packet.m_players[1] = new Player(1, 1, GameInitializers.PLAYER_2_X);

            Network.SendTCPPackageToClient(packet, client.ClientSession.SessionID);
            Clients.Add(newClient);                       
            
            if (GameStructure.PlayersCount == maxPlayers)
                GameState = GameStates.Ready;

            return true;
        }
        
        public void RejoinClient(NetworkConnection client)
        {
            Network.AddClientConnection(client);
        }
        
        private void GetAllThe()
        {
            packagesForNextFrame = Network.GrabAllNetworkDataForNextFrame();
        }
        

        private PackageInterface[] getAllDataRelatedToClient(int sessionID)
        {
            List<PackageInterface> ps = new List<PackageInterface>();

            if (!packagesForNextFrame.ContainsKey(sessionID))
                return new PackageInterface[0];

            foreach (PackageInterface p in packagesForNextFrame[sessionID])
            {
                    ps.Add(p);
            }
            
            
            
            return ps.ToArray();
        }
        
        

        public ServerDataPackage CalculateFrame()
        {
            NextFrame = new ServerDataPackage();

            foreach (KeyValuePair<int, List<Player>> a in GameStructure.GameTeams)
            {
                foreach (Player p in a.Value)
                {
                    if ((ClientMovement)GetLastPlayerMovement(p.ID) == ClientMovement.Down)
                    {
                        p.DirectionY = p.Speed;
                        Logger.Log("Received DOWN");
                    }
                    else if ((ClientMovement)GetLastPlayerMovement(p.ID) == ClientMovement.Up)
                    {
                        p.DirectionY = -p.Speed;
                        Logger.Log("Received UP");
                    }
                    else if ((ClientMovement)GetLastPlayerMovement(p.ID) == ClientMovement.StopMoving)
                    {
                        p.DirectionY = 0;
                        Logger.Log("Received STOP");
                    }

                    NextFrame.Players.Add(p);
                }
            }

            GameStructure.CalculateFrame(10);
            NextFrame.Ball.PositionX = GameStructure.Ball.PositionX;
            NextFrame.Ball.PositionY = GameStructure.Ball.PositionY;

            return NextFrame;
        }

             
        public ClientControls GetLastPlayerControl(int playerID)
        {
            List<ClientControlPackage> cc = new List<ClientControlPackage>();
            foreach (Client c in Clients)
            {
                PackageInterface[] ps = getAllDataRelatedToClient(c.session);
                foreach(PackageInterface p in ps)
                {
                    if (p == null || p.PackageType != PackageType.ClientControl)
                        continue;
                    cc.Add((ClientControlPackage)p);
                }
            }
            return cc[cc.Count - 1].ControlInput;
        }

        public ClientMovement GetLastPlayerMovement(int playerID)
        {

            List<PlayerMovementPackage> cc = new List<PlayerMovementPackage>();
            foreach (Client c in Clients)
            {
                PackageInterface[] ps = getAllDataRelatedToClient(c.session);
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

        public class Client
        {
            public int ClientID;
            public int session;
            public List<Player> Players = new List<Player>();

            public Client(GameStructure gameStructure, int sessionID)
            {
                this.session = sessionID;

            }

            public void AddPlayer(Player player, GameStructure GameStructure)
            {
                Players.Add(player);
                GameStructure.AddPlayer(player, GameStructure.GetFreeTeam());
            }
            

            
        }        
                        
    }
}
