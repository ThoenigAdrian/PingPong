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
        public ServerNetwork ServerNetwork = new ServerNetwork();

        public object ClientInitializeGamePackage { get; private set; }

        public Servers()
        {
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1);
        }

        public void Run()
        {
            LogWriter Logger = new LogWriterConsole();

            List<Socket> connectionList = new List<Socket>();

            Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ThreadPool.QueueUserWorkItem(AcceptNewConnection, newSocket);

            while (true)
            {
                if(newSocket.Connected)
                {
                    connectionList.Add(newSocket);
                    InitializeClient(newSocket);
                    TCPConnection tcp = new TCPConnection(newSocket, null);
                    NetworkConnection news = new NetworkConnection(tcp);
                    PackageInterface packet = news.ReadTCP();
                    if(packet.PackageType==PackageType.ClientInitalizeGamePackage)
                    {
                        ClientInitializeGamePackage initPackage = (ClientInitializeGamePackage)news.ReadTCP();
                        GameNetwork newGameNetwork = new GameNetwork();
                        Game newGame = new Game(newGameNetwork, initPackage.PlayerCount);
                    }
                    newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    
                    
                    GameNetwork asdf()
                    GameNetwork(news);
                    ThreadPool.QueueUserWorkItem(AcceptNewConnection, newSocket);
                }
            }
        }

        private void InitializeClient(Socket socket)
        {
            ServerNetwork.
            if (genericPackage.PackageType == PackageType.ClientInitalizeGamePackage)
            {
                GameNetwork newGameNetwork = new GameNetwork();
            }
        }

        public void AcceptNewConnection(object socket)
        {
            Socket sock = (Socket)socket;
            socket = MasterListeningSocket.Accept();
        }
    }
}
