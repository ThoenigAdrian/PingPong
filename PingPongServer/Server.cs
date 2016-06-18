using System;
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

using GameLogicLibrary;


namespace PingPongServer
{

    public class Server
    {

        private Socket MasterListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private UDPConnection MasterUDPSocket;
        private SafeList<NetworkConnection> IncomingConnections = new SafeList<NetworkConnection>();
        private SafeList<NetworkConnection> ConnectionsReadyForJoingAndStartingGames = new SafeList<NetworkConnection>();
        private SafeList<NetworkConnection> AcceptedConnections = new SafeList<NetworkConnection>();

        private LogWriterConsole Logger = new LogWriterConsole();
        private SafeList<ServerGame> PendingGames = new SafeList<ServerGame>();
        private SafeList<ServerGame> RunningGames = new SafeList<ServerGame>();
        private static List<bool> StateOfRunningGames = new List<bool>();


        public Server()
        {
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1);

            MasterUDPSocket = new UDPConnection(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT), Logger);
            MasterUDPSocket.DataReceivedEvent += MasterUDPSocket_DataReceivedEvent;
        }

        private void MasterUDPSocket_DataReceivedEvent(UDPConnection sender, byte[] data, IPEndPoint endPoint)
        {
            Logger.Log("[Debug] Received a UDP Package on the Master UDP Socket");
        }

        public void Run()
        {
            Logger.Log("Starting Connection Acceptor Thread");
            Thread CollectIncomingConnectionsThread = new Thread(new ThreadStart(CollectIncomingConnections));
            CollectIncomingConnectionsThread.Start();
            Logger.Log("Starting Game Manager Thread");
            Thread GameManagerThread = new Thread(new ThreadStart(ManageGames));
            GameManagerThread.Start();
            Logger.Log("Server is now running\n");

            while (true)
            {
                foreach (NetworkConnection networkConnection in AcceptedConnections.Entries)
                {
                    PackageInterface newPacket = networkConnection.ReadTCP();
                    if (newPacket == null)
                        continue;
                    
                    if (newPacket.PackageType == PackageType.ClientSessionRequest)
                    {
                        ClientSessionRequest packet = (ClientSessionRequest)newPacket;
                        if (packet.Reconnect)
                        {
                            // still need to handle if client requests a session which is already in useLogger.NetworkLog("Network Connection ");
                            Logger.NetworkLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") want's to reconnect with Session ID " + networkConnection.ClientSession.SessionID.ToString());
                            networkConnection.ClientSession = new Session(packet.ReconnectSessionID);
                            ConnectionsReadyForJoingAndStartingGames.Add(networkConnection);
                            AcceptedConnections.Remove(networkConnection);
                        }
                        else
                        {
                            Logger.NetworkLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") wants to connect ");
                            networkConnection.ClientSession = new Session(new Random().Next());
                            Logger.NetworkLog("Assigned Session " + networkConnection.ClientSession.SessionID + "to Client " + networkConnection.RemoteEndPoint.ToString());
                            ConnectionsReadyForJoingAndStartingGames.Add(networkConnection);
                            AcceptedConnections.Remove(networkConnection);
                        }
                        
                        ServerSessionResponse response = new ServerSessionResponse();
                        response.ClientSessionID = networkConnection.ClientSession.SessionID;
                        networkConnection.SendTCP(response);

                    }
                }                


                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }

            
        }
    
        
        

        private void ManageGames()
        {
            while (true)
            {
                foreach(ServerGame game in PendingGames.Entries)
                {
                    if (game.GameState == GameStates.Ready)
                    {
                        Logger.GameLog("Found a Game which is ready to start \nStarting the Game with index: ");
                        ThreadPool.QueueUserWorkItem(game.StartGame, new object());
                        RunningGames.Add(game);                        
                        PendingGames.Remove(game);
                    }
                }

                ServeClientGameRequests();
                    
                
                Thread.Sleep(10);
            }
        }

        private void CollectIncomingConnections()
        {
            while (true)
            {
                Socket newSocket = MasterListeningSocket.Accept();
                Logger.NetworkLog("Client connected " + newSocket.RemoteEndPoint.ToString());
                TCPConnection tcp = new TCPConnection(newSocket, null);
                NetworkConnection newNetworkConnection = new NetworkConnection(tcp);                

                AcceptedConnections.Add(newNetworkConnection);
            }
        }

        private void ServeClientGameRequests()
        {
            foreach (NetworkConnection conn in ConnectionsReadyForJoingAndStartingGames.Entries)
            {
                PackageInterface packet = conn.ReadTCP();
                if (packet == null)
                    continue;

                switch (packet.PackageType)
                {
                    case PackageType.ClientInitalizeGamePackage:
                        Logger.NetworkLog("Received a Client Initialize Game Package from : " + conn.RemoteEndPoint.ToString());
                        Logger.GameLog("Creating a new game");
                        CreateNewGame(conn, packet);
                        break;

                    /*case PackageType.ClientSessionReconnect:
                        if (!RejoinClientToGame(conn))
                            throw new NotImplementedException(); // Need to have an error handling package for client
                        break;*/

                    case PackageType.ClientJoinGameRequest:
                        if (!JoinClientToGame(conn, packet))
                            throw new NotImplementedException(); // Need to have an error handling package for client
                        break;

                }
            }
            RemoveDeadConnections();


        }

        private void CreateNewGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)(packet);
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket);
            ServerGame newGame = new ServerGame(newGameNetwork, initPackage.GamePlayerCount);
            newGame.AddClient(conn, initPackage.PlayerTeamwish.Length);
            PendingGames.Add(newGame);
            
                
        }

        // Return true if client could rejoin the game
        private bool RejoinClientToGame(NetworkConnection conn)
        {
            
           foreach(ServerGame game in RunningGames.Entries)
           {
                if (game.Network.DiedSessions.Contains(conn.ClientSession.SessionID))
                {
                    game.RejoinClient(conn);
                    return false;
                }
            }

            return false;           
        }

        // Returns true if client could be added to a game
        private bool JoinClientToGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientJoinGameRequest pack = (ClientJoinGameRequest)packet;

            foreach(ServerGame game in PendingGames.Entries)
            {
                if(game.AddClient(conn, pack.PlayerTeamwish.Length))
                    return true;                             
            }
            return false;
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
                
        private void RemoveFinishedGames()
        {   // We need to do cleanups otherwise the server will run for a few days and be out of memory
            foreach(ServerGame game in RunningGames.Entries)
            {
                if (game.GameState == GameStates.Finished)
                {
                   Logger.GameLog("Found a finished Game removing it now" + RunningGames.ToString());
                   RunningGames.Remove(game);
                }
                                        
            }
        }
        
    }
}
