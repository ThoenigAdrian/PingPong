using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary;
using NetworkLibrary.Utility;
using System.Threading.Tasks;
using NetworkLibrary.ConnectionImplementations.NetworkImplementations;

namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            List<TCPServerConnection> ClientConnections = new List<TCPServerConnection>();
            Socket MasterListeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1); // Allow two Client for now
            LogWriter Logger = new LogWriterConsole();



            while(true)
            {
                ClientConnections.Add(new TCPServerConnection(MasterListeningSocket.Accept()));
                Logger.Log("Client connected");
                Thread.Sleep(500);                
            }
        }


    }
}
