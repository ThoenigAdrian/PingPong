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
        private List<Game> PendingGames = new List<Game>();
        private List<Game> RunningGames = new List<Game>();
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
            Console.WriteLine("received");
        }

        public void Run()
        {
            Thread ConnectionAcceptorThread = new Thread(new ThreadStart(AcceptIncomingConnections) );
            ConnectionAcceptorThread.Start();

            Thread GameManagerThread = new Thread(new ThreadStart(ManageGames) );
            GameManagerThread.Start();

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
                Logger.Log("Client connected " + newSocket.RemoteEndPoint.ToString());
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
                        CreateNewGame(conn, packet);
                        break;

                    case PackageType.ClientJoinGameRequest:
                        JoinClientToGame(conn, packet);
                        break;

                }
            }
        }

        private void CreateNewGame(NetworkConnection conn, PackageInterface packet)
        {
            ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)(packet);
            GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket);
            Game newGame = new Game(newGameNetwork, initPackage.PlayerCount);
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
                
        private void RemoveFinishedGames()
        {   // We need to do cleanups otherwise the server will run for a few days and be out of memory
            lock (RunningGames)
            {
                for (int index = RunningGames.Count; index >= 0; index--)
                {
                    if (RunningGames[index].GameState == GameStates.Finished)
                        RunningGames.RemoveAt(index);
                }
            }
        }
        
    }
}
