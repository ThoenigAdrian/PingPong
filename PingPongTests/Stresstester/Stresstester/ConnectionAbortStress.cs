using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester
{
    class ConnectionEstablishAbortStresser
    {
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
            for (int waitTime = 0; waitTime <= max; waitTime += step)
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
    }
}
