using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using NetworkLibrary;
using NetworkLibrary.Utility;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using XSLibrary.Network.Connections;

using GameLogicLibrary;
using PingPongServer.ServerGame;

using Newtonsoft.Json.Linq;
using XSLibrary.Network.Accepters;
using PingPongServer.Matchmaking;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer
{

    public class Server : IDisposable
    {
        // Network 
        private TCPAccepter ConnectionAccepter;
        private UDPConnection MasterUDPSocket;
        private SafeList<NetworkConnection> ConnectionsReadyForJoingAndStartingGames = new SafeList<NetworkConnection>();
        private SafeList<NetworkConnection> AcceptedConnections = new SafeList<NetworkConnection>();
        private MatchmakingManager MatchManager = new MatchmakingManager();

        // Game
        private SafeList<Game> PendingGames = new SafeList<Game>();
        private SafeList<Game> RunningGames = new SafeList<Game>();
        private static List<bool> StateOfRunningGames = new List<bool>();
        // Logging
        private LogWriterConsole Logger { get; set; } = new LogWriterConsole();

        

        public bool ServerStopping { get { return m_stopServer; } }
        volatile bool m_stopServer = false;

        private int maximumNumberOfIncomingConnections = 1000;

        public Server()
        {
            ConnectionAccepter = new TCPAccepter(NetworkConstants.SERVER_PORT, maximumNumberOfIncomingConnections);
            ConnectionAccepter.ClientConnected += OnSocketAccept;

            MasterUDPSocket = new UDPConnection(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterUDPSocket.OnDisconnect += MasterUDPSocket_OnDisconnect;
            MasterUDPSocket.Logger = Logger;
            ReadConfigurationFromConfigurationFile();

            MatchManager.OnMatchFound += StartMatchmadeGame;
        }

        private void MasterUDPSocket_OnDisconnect(object sender, EventArgs e)
        {
            if (!ServerStopping)
                throw new Exception("y u du dis");
        }

        public void Run()
        {
            ConnectionAccepter.Run();
            StartGameManagerThread();

            Logger.Log("Server is now running\n");

            while (!m_stopServer)
            {
                foreach (NetworkConnection networkConnection in AcceptedConnections.Entries)
                    ProcessClientSessionRequest(networkConnection);

                RemoveDeadConnections();
                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }
        }

        public void Stop()
        {
            m_stopServer = true;

            ConnectionAccepter.Stop();
        }

        private void OnSocketAccept(object sender, Socket acceptedSocket)
        {
            Logger.NetworkLog("Client connected " + acceptedSocket.RemoteEndPoint.ToString());
            TCPPacketConnection tcp = new TCPPacketConnection(acceptedSocket);
            NetworkConnection newNetworkConnection = new NetworkConnection(tcp);

            AcceptedConnections.Add(newNetworkConnection);
        }

        private void StartGameManagerThread()
        {
            Logger.Log("Starting Thread which takes Care of the Games");
            Thread ManageGamesThread = new Thread(new ThreadStart(ManageGames));
            ManageGamesThread.Start();
        }

        private void ProcessClientSessionRequest(NetworkConnection networkConnection)
        {
            PackageInterface newPacket = networkConnection.ReadTCP();
            if (newPacket == null)
                return;

            if (newPacket.PackageType == PackageType.ClientSessionRequest)
            {
                ClientSessionRequest packet = (ClientSessionRequest)newPacket;

                if (packet.Reconnect)
                    ReconnectClientWithPreviousSession(networkConnection, packet);

                else
                    ConnectClientWithNewSession(networkConnection);

                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = networkConnection.ClientSession.SessionID;
                networkConnection.SendTCP(response);
            }
        }

        private void ManageGames()
        {
            while (!m_stopServer)
            {
                foreach (Game game in PendingGames.Entries)
                {
                    if (game.GameState == GameStates.Ready)
                        StartGame(game);
                }
                ServeClientGameRequests();
                MatchManager.Update();


                Thread.Sleep(10);
            }
        }

        private void StartMatchmadeGame(object sender, MatchmakingManager.MatchData match)
        {
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket);
            Game newGame = new Game(newGameNetwork, match.MaxPlayerCount);
            newGame.GameID = new Random().Next();

            foreach (MatchmakingManager.ClientData client in match.Clients)
            {
                ServerMatchmakingStatusResponse GameFoundPackage = new ServerMatchmakingStatusResponse();
                GameFoundPackage.GameFound = true;
                GameFoundPackage.Status = "Game will start soon...";
                GameFoundPackage.Error = false;
                client.m_clientConnection.SendTCP(GameFoundPackage);
                newGame.AddClient(client.m_clientConnection, client.m_request.GetPlayerPlacements());
            }

            newGame.StartGame(this);
        }

        private void ServeClientGameRequests()
        {
            foreach (NetworkConnection conn in ConnectionsReadyForJoingAndStartingGames.Entries)
            {
                PackageInterface packet = conn.ReadTCP();
                if (packet == null)
                    continue;

                if (packet.PackageType == PackageType.ClientInitalizeGamePackage)
                {
                    ClientInitializeGamePackage initPackage = packet as ClientInitializeGamePackage;
                    switch (initPackage.Request)
                    {
                        case ClientInitializeGamePackage.RequestType.StartNew:
                            Logger.NetworkLog("Received a Client Initialize Game Package from : " + conn.RemoteEndPoint.ToString());
                            Logger.GameLog("Creating a new game");
                            CreateNewGame(conn, packet);
                            break;
                        case ClientInitializeGamePackage.RequestType.Join:
                            if (!JoinClientToGame(conn, packet))
                                throw new NotImplementedException(); // Need to have an error handling package for client
                            break;

                        case ClientInitializeGamePackage.RequestType.Matchmaking:
                            MatchManager.AddClientToQueue(conn, initPackage);
                            SendMatchmakingInitResponse(conn, initPackage);
                            break;
                    }
                }
            }
        }

        private void SendMatchmakingInitResponse(NetworkConnection clientConnection, ClientInitializeGamePackage initPackage)
        {
            ServerMatchmakingStatusResponse response = new ServerMatchmakingStatusResponse();
            Request clientRequest = new Request(clientConnection.ClientSession.SessionID, initPackage.GamePlayerCount, initPackage.PlayerTeamwish);
            if (MatchManager.IsRequestValid(clientRequest))
            {
                
                response.Error = false;
                response.Status = "Waiting for additional players to start the game";
                response.GameFound = false;
                clientConnection.SendTCP(response);
            }
            else
            {
                response.Error = true;
                response.GameFound = false;
                response.Status = "Could not start matchmaking because of an invalid request\n";
                response.Status += "Received Request: PlayerCount(" + initPackage.GamePlayerCount.ToString() + ") TeamWishes (" + initPackage.PlayerTeamwish.ToString() + ")";
            }
        }

        private void ConnectClientWithNewSession(NetworkConnection networkConnection)
        {
            Logger.NetworkLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") wants to connect ");
            networkConnection.ClientSession = new Session(new Random().Next());
            Logger.NetworkLog("Assigned Session " + networkConnection.ClientSession.SessionID + " to Client " + networkConnection.RemoteEndPoint.ToString());
            ConnectionsReadyForJoingAndStartingGames.Add(networkConnection);
            AcceptedConnections.Remove(networkConnection);
        }

        private void ReconnectClientWithPreviousSession(NetworkConnection networkConnection, ClientSessionRequest packet)
        {
            // still need to handle if client requests a session which is already in use
            Logger.NetworkLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") want's to reconnect with Session ID " + packet.ReconnectSessionID.ToString());
            networkConnection.ClientSession = new Session(packet.ReconnectSessionID);
            ConnectionsReadyForJoingAndStartingGames.Add(networkConnection);
            AcceptedConnections.Remove(networkConnection);
        }

        private void StartGame(Game game)
        {
            Logger.GameLog("Found a Game which is ready to start ID: " + game.GameID);
            ThreadPool.QueueUserWorkItem(game.StartGame, this);
            RunningGames.Add(game);
            PendingGames.Remove(game);
        }
        
        // Returns true if the Game was valid and could be created
        private bool CreateNewGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)(packet);
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket);
            Game newGame = new Game(newGameNetwork, initPackage.GamePlayerCount);
            newGame.GameID = new Random().Next();
            newGame.AddClient(conn, initPackage.PlayerTeamwish);
            ConnectionsReadyForJoingAndStartingGames.Remove(conn);
            PendingGames.Add(newGame);
            Logger.Log("Created a new Game with the following details: " + newGame.ToString());
            return true;
                
        }

        // Returns true if client could be added to a game
        private bool JoinClientToGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientInitializeGamePackage pack = (ClientInitializeGamePackage)packet;

            foreach (Game game in PendingGames.Entries)
            {
                if (game.AddClient(conn, pack.PlayerTeamwish))
                {
                    ConnectionsReadyForJoingAndStartingGames.Remove(conn);
                    return true;
                }
                    
            }
            return false;
        }

        // Return true if client could rejoin the game
        private bool RejoinClientToGame(NetworkConnection conn)
        {
            bool couldRejoin = false;

           foreach(Game game in RunningGames.Entries)
           {
                if (game.Network.DiedSessions.Contains(conn.ClientSession.SessionID))
                {
                    game.RejoinClient(conn);
                    ConnectionsReadyForJoingAndStartingGames.Remove(conn);
                    couldRejoin = true;
                    break;
                }
                 
            }

            return couldRejoin;           
        }
        

        private void RemoveDeadConnections()
        {
            foreach(NetworkConnection networkConnection in AcceptedConnections.Entries)
            {
                if (!networkConnection.Connected)
                {
                    Logger.NetworkLog("Removing disconnected connection from Accepted Connection ( Client :  " + networkConnection.RemoteEndPoint.ToString() + ")");
                    AcceptedConnections.Remove(networkConnection);
                }
            }
        }

        public void OnGameFinished(object sender, EventArgs e)
        {
            RemoveFinishedGames();
        }

        public void ReadConfigurationFromConfigurationFile()
        {
            // Configuration filename must be server_config.json
            string filename = "server_config.json";
            ReadConfigurationFromConfigurationFile(filename);

        }

        public void ReadConfigurationFromConfigurationFile(string filename)
        {
            string serverConfig = "";
            try
            {
                using (StreamReader serverConfigReadStream = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)))
                {
                    serverConfig = serverConfigReadStream.ReadToEnd();
                }
                JObject parsedServerConfiguration = JObject.Parse(serverConfig);
                try
                {
                    maximumNumberOfIncomingConnections = (int)parsedServerConfiguration["maximumNumberOfIncomingConnections"];
                }
                    
                catch(Exception exception)
                {
                    Logger.Log("Couldn't read maximumNumberOfConnections from configuration file , details : ");
                    Logger.Log(exception.Message);
                }
                    
            }
            catch (FileNotFoundException)
            {
                Logger.Log("No configuration file for server found using default configuration");
            }
            catch (Newtonsoft.Json.JsonReaderException exception)
            {
                Logger.Log("Server configuration file is invalid, Additional Information : ");
                Logger.Log(exception.Message);
                Logger.Log("[Warning] Default Configuraiton will be used instead !\n");
            }
            
        }

        private void RemoveFinishedGames()
        {   
            foreach(Game game in RunningGames.Entries)
            {
                if (game.GameState == GameStates.Finished)
                {
                   Logger.GameLog("Found a finished Game removing it now\n" + game.ToString());
                   RunningGames.Remove(game);
                }                                        
            }
        }

        public void Dispose()
        {
            Stop();
        }
                
    }
}
