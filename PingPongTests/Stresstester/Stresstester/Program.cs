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

namespace Stresstester
{
    class Program
    {
        static JSONAdapter PacketAdapter = new JSONAdapter();
        static LogWriterConsole Logger = new LogWriterConsole();
        static List<TCPPacketConnection> connections = new List<TCPPacketConnection>();
        static void Main(string[] args)
        {
            
            Logger.Log("Test");
            openMultipleGames(5);
        }

        static void openMultipleGames(int numberOfGames)
        {
            do
            {
                Logger.Log("Opening Multiple Games");
                for (int i = 0; i < numberOfGames; i++)
                {
                    IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);
                    Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    connectionSocket.Connect(server);
                    TCPPacketConnection conn = new TCPPacketConnection(connectionSocket);
                    connections.Add(conn);
                    openGame(conn);
                    //Thread.Sleep(1000); // If this line get's removed the server isn't able to start multiple Games !! Bug Found

                }
                Logger.Log("Started " + numberOfGames.ToString() + "  Games now waiting to see what will happen");
                Logger.Log("Press enter to disconnect...");
                Console.In.ReadLine();
                Disconnect();

                Logger.Log("Press enter to do it again - type \"exit\" to exit.");
            } while (Console.In.ReadLine() != "exit");
        }

        static void openGame(TCPPacketConnection conn)
        {
            ClientSessionRequest sessionRequest = new ClientSessionRequest();
            sessionRequest.Reconnect = false;
            sessionRequest.ReconnectSessionID = 0;

            sendWithAdapter(conn, sessionRequest);

            ClientInitializeGamePackage initGame = new ClientInitializeGamePackage();
            initGame.GamePlayerCount = 2;
            initGame.PlayerTeamwish = new int[initGame.GamePlayerCount];
            initGame.PlayerTeamwish[0] = 0;
            initGame.PlayerTeamwish[1] = 1;
            sendWithAdapter(conn, initGame);
           
        }

        static void sendWithAdapter(TCPPacketConnection conn, PackageInterface packet)
        {
            conn.Send(PacketAdapter.CreateNetworkDataFromPackage(packet));
        }

        static void Disconnect()
        {
            foreach (ConnectionInterface connection in connections)
                connection.Disconnect();

            connections.Clear();

            Logger.Log("Disconnected.");
        }

        // Notizen:
        // Wenn da Server grad beim aufräumen isch, also wenn an Haufen Games gleichzeitig aufhören dann hat da Matchmaking Open Request ziemlich lang gedauert beim echtne CLient.
    }
}
