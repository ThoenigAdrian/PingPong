using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkLibrary;
using NetworkLibrary.PackageAdapters;
using XSLibrary;
using XSLibrary.Network.Connections;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkLibrary.Utility;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using GameLogicLibrary;
using GameLogicLibrary.GameObjects;

namespace Stresstester
{
    class Program
    {
        static JSONAdapter PacketAdapter = new JSONAdapter();
        static LogWriterConsole Logger = new LogWriterConsole();
        static List<TCPPacketConnection> connections = new List<TCPPacketConnection>();
        static Random RandomGenerator = new Random();

        static void Main(string[] args)
        {
            
            Logger.Log("Test");
            // CreateDemoClient();
            openMultipleGames(500);
            Thread.Sleep(10000);
            /*IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectionSocket.Connect(server);
            TCPPacketConnection conn = new TCPPacketConnection(connectionSocket);
            Thread.Sleep(5000);
            conn.Disconnect();
            //HighResolutionTest();
            //MidResoultionTest();*/
        }

        
        // Notizen:
        // Wenn da Server grad beim aufräumen isch, also wenn an Haufen Games gleichzeitig aufhören dann hat da Matchmaking Open Request ziemlich lang gedauert beim echtne CLient.
    }
}
