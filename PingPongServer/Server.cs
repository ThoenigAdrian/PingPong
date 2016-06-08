using System;
using System.Collections.Generic;
using System.Linq;
using NetworkLibrary.ConnectionImplementations.NetworkImplementations;
using NetworkLibrary;
using System.Net.Sockets;
using System.Net;
using NetworkLibrary.Utility;
using System.Threading;
using NetworkLibrary.DataStructs;
using NetworkLibrary.PackageAdapters;
using GameLogicLibrary;
using System.Text;
using System.Threading.Tasks;

namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            List<TCPServerConnection> ClientConnections = new List<TCPServerConnection>();
            Socket MasterListeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1); // Allow two Clients for now
            LogWriter Logger = new LogWriterConsole();

            ServerUDPAdapter udp = new ServerUDPAdapter();


            while (true)
            {
                ClientConnections.Add(new TCPServerConnection(MasterListeningSocket.Accept()));
                Logger.Log("Client connected");
                ServerDataPackage package = new ServerDataPackage();
                package.BallPosX = 50;
                package.BallPosY = 50;
                int turn = 1;
                while (true)
                {

                    package.BallPosX += 1 * turn;

                    if (GameInitializers.BORDER_HEIGHT - 4 <= package.BallPosX)
                        turn *= -1;
                    if (package.BallPosX <= 4)
                        turn *= -1;



                    ClientConnections[0].Send(udp.PackageToByte(package));
                    Thread.Sleep(16);
                }

                foreach (TCPServerConnection Server in ClientConnections)
                {
                    try
                    {
                        Server.Send(udp.PackageToByte(package));
                    }

                    catch (SocketException)
                    {

                    }
                }

                Thread.Sleep(500);
            }
        }


    }
}

}
