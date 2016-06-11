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

namespace PingPongServer
{


    public class Servers
    {

        public Socket MasterListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private UDPConnection MasterUDPSocket;
        private LogWriterConsole Logger = new LogWriterConsole();
        private List<Game> Games = new List<Game>();
        private List<Game> PendingGames = new List<Game>();


        public Servers()
        {
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1);
            
            MasterUDPSocket = new UDPConnection(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT), Logger);
        }

        public void Run()
        {
            
            Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ThreadPool.QueueUserWorkItem(AcceptNewConnection, newSocket);

            while(true)
            {
                
                if (newSocket.Connected)
                {
                    TCPConnection tcp = new TCPConnection(newSocket, null);
                    NetworkConnection news = new NetworkConnection(tcp);
                    PackageInterface packet = news.ReadTCP();
                    switch (packet.PackageType)
                    {
                        case PackageType.ClientInitalizeGamePackage:
                            {
                                ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)news.ReadTCP();
                                GameNetwork newGameNetwork = new GameNetwork(MasterUDPSocket, tcp);
                                Game newGame = new Game(newGameNetwork, initPackage.PlayerCount);
                                break;
                            }

                        case PackageType.ClientJoinGameRequest:
                            {
                                foreach (Game game in PendingGames)
                                {
                                    game.Network.AddClient(news);
                                }
                                break;
                            }                        
                    }

                    newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ThreadPool.QueueUserWorkItem(AcceptNewConnection, newSocket);

                }
                Thread.Sleep(50); // Sleep a little in order to save resources might need to be decreases if we need to handle more than 20 Client Requests per Second
            }
        }

        private void StartGamesWhichAreReady()
        {
            foreach (Game game in PendingGames)
            {
                if (game.isReady)
                    game.StartGame();
            }
        }

        private void InitializeClient(Socket socket)
        {
            /*
            if (genericPackage.PackageType == PackageType.ClientInitalizeGamePackage)
            {
                GameNetwork newGameNetwork = new GameNetwork();
            }*/
        }

        public void AcceptNewConnection(object socket)
        {
            Socket sock = (Socket)socket;
            socket = MasterListeningSocket.Accept();
        }
    }
}
