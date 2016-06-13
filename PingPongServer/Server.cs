using NetworkLibrary;
using System.Net.Sockets;
using System.Net;
using NetworkLibrary.Utility;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using System.Threading;
using System.Collections.Generic;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System;
using NetworkLibrary.DataPackages.ServerSourcePackages;

namespace PingPongServer
{


    public class Server
    {

        public Socket MasterListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private UDPConnection MasterUDPSocket;
        private LogWriterConsole Logger = new LogWriterConsole();
        private List<Game> PendingGames = new List<Game>();
        private List<Game> RunningGames = new List<Game>();
        private static List<bool> StateOfRunningGames = new List<bool>();
        public Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public List<NetworkConnection> IncomingConnections = new List<NetworkConnection>();
        Semaphore newConnectionAccepted = new Semaphore(0, 1);


        public Server()
        {
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1);
            
            MasterUDPSocket = new UDPConnection(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT), Logger);
        }

        public void Run()
        {
            Thread ConnectionAcceptorThread = new Thread(new ThreadStart(AcceptConnections) );
            ConnectionAcceptorThread.Start();

            Thread ClientToGameDistributor = new Thread(new ThreadStart(AcceptClients) );
            ClientToGameDistributor.Start();

            while(true)
            {
                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }
        }


        // We need to do cleanups otherwise the server will run for a few days and be out of memory
        private void RemoveFinishedGames()
        {
            lock (RunningGames)
            {
                for (int index = RunningGames.Count; index >= 0; index--)
                {
                    if (RunningGames[index].GameState == Game.GameStates.Finished)
                        RunningGames.RemoveAt(index);
                }
            }
        }

        
        public void AcceptConnections()
        {

            while (true)
            {
                newSocket = MasterListeningSocket.Accept();
                Logger.Log("Client connected " + newSocket.RemoteEndPoint.ToString());
                TCPConnection tcp = new TCPConnection(newSocket, null);
                NetworkConnection newNetworkConnection = new NetworkConnection(tcp, new Random().Next());
                ServerSessionResponse a = new ServerSessionResponse();
                a.ClientSessionID = newNetworkConnection.ClientSession.SessionID;
                newNetworkConnection.SendTCP(a);

                lock (IncomingConnections)
                    IncomingConnections.Add(newNetworkConnection);
                
            }

        }

        public void AcceptClients()
        {
            while(true)
            {
                lock(IncomingConnections)
                {
                    foreach (NetworkConnection conn in IncomingConnections)
                    {
                        PackageInterface packet = conn.ReadTCP();
                        if (packet != null)
                        {
                            switch (packet.PackageType)
                            {
                                case PackageType.ClientInitalizeGamePackage:
                                    {
                                        ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)(packet);
                                        GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket, conn);
                                        Game newGame = new Game(newGameNetwork, initPackage.PlayerCount);
                                        PendingGames.Add(newGame);
                                        break;
                                    }

                                case PackageType.ClientJoinGameRequest:
                                    {
                                        ClientJoinGameRequest pack = (ClientJoinGameRequest)packet;
                                        PendingGames[0].AddClient(conn);
                                        if (PendingGames[0].GameState == Game.GameStates.Ready)
                                        {
                                            RunningGames.Add(PendingGames[0]);
                                            PendingGames.RemoveAt(0);

                                            // Start each game which is ready in a new Thread. Communication will be done via the "StateOfRunningGames" which indicates if the Game is finished
                                            ThreadPool.QueueUserWorkItem(RunningGames[RunningGames.Count - 1].StartGame, new object());
                                        }
                                            
                                        break;
                                        
                                    }
                            }
                        }
                    }

                    Thread.Sleep(10);
                
                }
            
            }
        }
    }
}
