using NetworkLibrary;
using System.Net.Sockets;
using System.Net;
using NetworkLibrary.Utility;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataPackages;
using System.Threading;
using System.Collections.Generic;

namespace PingPongServer
{


    public class Servers
    {

        public Socket MasterListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ServerNetwork ServerNetwork = new ServerNetwork();

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
                    newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ThreadPool.QueueUserWorkItem(AcceptNewConnection, newSocket);
                }
            }
        }

        private void InitializeClient(Socket socket)
        {
            

            ServerNetwork.
            if (genericPackage.PackageType == PackageType.ClientInitalizeGamePackage)
                

        }

        public void AcceptNewConnection(object socket)
        {
            Socket sock = (Socket)socket;
            socket = MasterListeningSocket.Accept();
        }
    }
}
