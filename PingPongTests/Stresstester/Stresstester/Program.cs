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
        static Random RandomGenerator = new Random();

        static void Main(string[] args)
        {
            
            Logger.Log("Test");
            HighResolutionTest();
            MidResoultionTest();
        }

        static void HighResolutionTest()
        {
            int start = 0;
            int step = 2;
            int max = 2000;
            int numberOfConnectionsPerBatch = 10;
            delayTest(start, step, max, numberOfConnectionsPerBatch);
        }

        static void MidResoultionTest()
        {
            int start = 2000;
            int step = 100;
            int max = 8000;
            int numberOfConnectionsPerBatch = 10;
            delayTest(start, step, max, numberOfConnectionsPerBatch);
        }
        
        static void delayTest(int start, int step, int max, int numberOfConnections)
        {
            for(int waitTime=0; waitTime <= max; waitTime+=step)
            {
                openMultipleGames(numberOfConnections);
                Thread.Sleep(waitTime);
                Disconnect();
            }
        }

        static void openAndTheCloseGameWithRandomDelayMultiple(int numberOfGames)
        {
            for (int i = 0; i < numberOfGames; i++)
            {
                List<TCPPacketConnection> connections = new List<TCPPacketConnection>();
                IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);
                Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                connectionSocket.Connect(server);
                TCPPacketConnection conn = new TCPPacketConnection(connectionSocket);
                connections.Add(conn);
                openAndThenCloseGameWithRandomDelay(conn);
            }
        }
        static void openAndThenCloseGameWithRandomDelay(TCPPacketConnection conn)
        {
            int waitTimeMillisecs = RandomGenerator.Next(30 * 1000);
            Logger.Log("Waiting " + waitTimeMillisecs.ToString() + "Milliseconds");
            openAndThenCloseGameWithDelay(conn, waitTimeMillisecs);
        }
        static void openAndThenCloseGameWithDelay(TCPPacketConnection conn, int delay)
        {
            openGame(conn);
            Thread.Sleep(delay);
            closeGame(conn);
            openMultipleGames(5);
        }

        static void openMultipleGames(int numberOfGames)
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

            }
            Logger.Log("Started " + numberOfGames.ToString() + "  Games");
        }

        static void openMultipleGamesInteractive(int numberOfGames)
        {
            do
            {
                Disconnect();
                openMultipleGames(numberOfGames);
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

        static void closeGame(TCPPacketConnection conn)
        {
            conn.Disconnect();
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
