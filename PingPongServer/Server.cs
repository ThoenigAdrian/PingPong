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
            Thread ConnectionAcceptorThread = new Thread(new ThreadStart(AcceptIncomingConnections) );
            ConnectionAcceptorThread.Start();
            Logger.Log("Starting Game Manager Thread");
            Thread GameManagerThread = new Thread(new ThreadStart(ManageGames) );
            GameManagerThread.Start();
            Logger.Log("Server is now running\n");
            while(true)
            {
                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }
        }
        

        private void ManageGames()
        {
            while (true)
            {
                for (int index = PendingGames.Count - 1; index >= 0; index--)
                {
                    PendingGames[index].Network.receiveUDPTest();
                    if (PendingGames[index].GameState == GameStates.Ready)
                    {
                        Logger.Log("Found a Game which is ready to start \n Starting the Game with index: " + index.ToString());
                        RunningGames.Add(PendingGames[index]);
                        PendingGames.RemoveAt(index);
                    }
                }

                lock (IncomingConnections)
                    ServeClientGameRequests();
                    
                
                Thread.Sleep(10);
            }
        }

        private void AcceptIncomingConnections()
        {
            while (true)
            {
                Socket newSocket = MasterListeningSocket.Accept();
                Logger.NetworkLog("Client connected " + newSocket.RemoteEndPoint.ToString());
                TCPConnection tcp = new TCPConnection(newSocket, null);
                NetworkConnection newNetworkConnection = new NetworkConnection(tcp, new Random().Next());
                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = newNetworkConnection.ClientSession.SessionID;
                newNetworkConnection.SendTCP(response);

                lock (IncomingConnections)
                    IncomingConnections.Add(newNetworkConnection);
            }
        }

        private void ServeClientGameRequests()
        {
            foreach (NetworkConnection conn in IncomingConnections)
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

                    case PackageType.ClientJoinGameRequest:
                        JoinClientToGame(conn, packet);
                        break;

                }
            }
            RemoveDeadConnections();


        }

        private void CreateNewGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)(packet);
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket);
            ServerGame newGame = new ServerGame(newGameNetwork, initPackage.PlayerCount);
            PendingGames.Add(newGame);
            PendingGames[0].AddClient(conn);
        }

        private void JoinClientToGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientJoinGameRequest pack = (ClientJoinGameRequest)packet;

            if (PendingGames.Count <= 0)
                return;                

            PendingGames[0].AddClient(conn);
            if (PendingGames[0].GameState == GameStates.Ready)
            {
                RunningGames.Add(PendingGames[0]);
                PendingGames.RemoveAt(0);

                ThreadPool.QueueUserWorkItem(RunningGames[RunningGames.Count - 1].StartGame, new object());
            }
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
