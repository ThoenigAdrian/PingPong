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
        private List<NetworkConnection> IncomingConnections = new List<NetworkConnection>();
        private List<NetworkConnection> AcceptedConnections = new List<NetworkConnection>();

        private LogWriterConsole Logger = new LogWriterConsole();
        private List<ServerGame> PendingGames = new List<ServerGame>();
        private List<ServerGame> RunningGames = new List<ServerGame>();
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
            Thread CollectIncomingConnectionsThread = new Thread(new ThreadStart(CollectIncomingConnections) );
            CollectIncomingConnectionsThread.Start();
            Logger.Log("Starting Game Manager Thread");
            Thread GameManagerThread = new Thread(new ThreadStart(ManageGames) );
            GameManagerThread.Start();
            Logger.Log("Server is now running\n");

            while(true)
            {
                foreach(NetworkConnection networkConnection in IncomingConnections)
                {
                    PackageInterface newPacket = networkConnection.ReadTCP();
                    if (newPacket == null)
                        continue;
                    if (newPacket.PackageType == PackageType.ClientSessionRequest)
                    {
                        ClientSessionRequest packet = (ClientSessionRequest)newPacket;
                        if (packet.Reconnect)
                        {
                            // still need to handle if client requests a session which is already in use
                            networkConnection.ClientSession = new Session(packet.ReconnectSessionID);
                            lock(AcceptedConnections)
                                AcceptedConnections.Add(networkConnection);
                        }
                        else
                            networkConnection.ClientSession = new Session(new Random().Next());
                            
                    }
                        
                    ServerSessionResponse response = new ServerSessionResponse();
                    response.ClientSessionID = networkConnection.ClientSession.SessionID;
                    networkConnection.SendTCP(response);
                    
                }
                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }
        }
        

        private void ManageGames()
        {
            while (true)
            {
                for (int index = PendingGames.Count - 1; index >= 0; index--)
                {
                    if (PendingGames[index].GameState == GameStates.Ready)
                    {
                        Logger.Log("Found a Game which is ready to start \n Starting the Game with index: " + index.ToString());
                        RunningGames.Add(PendingGames[index]);
                        PendingGames.RemoveAt(index);
                    }
                }

                lock (AcceptedConnections)
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

                lock (IncomingConnections)
                    IncomingConnections.Add(newNetworkConnection);
            }
        }

        private void ServeClientGameRequests()
        {
            foreach (NetworkConnection conn in AcceptedConnections)
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

                    case PackageType.ClientSessionReconnect:
                        if (!RejoinClientToGame(conn))
                            throw new NotImplementedException(); // Need to have an error handling package for client
                        break;

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
            ServerGame newGame = new ServerGame(newGameNetwork, initPackage.PlayerTeamwish.Length);
            lock(PendingGames)
            {
                PendingGames.Add(newGame);
                PendingGames[PendingGames.Count - 1].AddClient(conn, initPackage.PlayerTeamwish.Length);
            }
                
        }

        private bool RejoinClientToGame(NetworkConnection conn)
        {
            bool result = false;

            lock(RunningGames)
            {
                for (int index = 0; index <= RunningGames.Count; index++)
                {
                    if (RunningGames[index].Network.DiedSessions.Contains(conn.ClientSession.SessionID))
                    {
                        RunningGames[index].RejoinClient(conn);
                        result = true;
                    }
                }
            }

            return result;            
        }

        private bool JoinClientToGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientJoinGameRequest pack = (ClientJoinGameRequest)packet;

            lock(PendingGames)
            {
                if (PendingGames.Count <= 0)
                    return false;

                for (int index = 0; index < PendingGames.Count; index++)
                {
                    PendingGames[index].AddClient(conn, pack.PlayerTeamwish.Length);
                }

                for (int index = PendingGames.Count - 1; index <= 0; index--)
                {
                    PendingGames[index].AddClient(conn, pack.PlayerTeamwish.Length);
                    if (PendingGames[index].GameState == GameStates.Ready)
                    {
                        RunningGames.Add(PendingGames[index]);
                        PendingGames.RemoveAt(index);
                        ThreadPool.QueueUserWorkItem(RunningGames[RunningGames.Count - 1].StartGame, new object());
                    }                    
                }
            }

            return true;
        }

        private void RemoveDeadConnections()
        {
            for (int index = IncomingConnections.Count - 1; index >= 0; index--)
            {
                if (!IncomingConnections[index].Connected)
                {
                    Logger.NetworkLog("Removing disconnected connection from Incoming Connection ( Client :  " + IncomingConnections[index].RemoteEndPoint.ToString() + ")");
                    IncomingConnections.RemoveAt(index);
                }
                    
            }
        }
                
        private void RemoveFinishedGames()
        {   // We need to do cleanups otherwise the server will run for a few days and be out of memory
            lock (RunningGames)
            {
                for (int index = RunningGames.Count - 1; index >= 0; index--)
                {
                    if (RunningGames[index].GameState == GameStates.Finished)
                        Logger.GameLog("Found a finished Game removing it now" + RunningGames.ToString());
                        RunningGames.RemoveAt(index);
                }
            }
        }
        
    }
}
