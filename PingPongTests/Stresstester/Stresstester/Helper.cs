using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester
{
    class Helper
    {
        static void closeGame(TCPPacketConnection conn)
        {
            conn.Disconnect();
        }

        static void sendWithAdapter(TCPPacketConnection conn, PackageInterface packet)
        {
            conn.Send(PacketAdapter.CreateNetworkDataFromPackage(packet));
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
                //Thread.Sleep(50);

            }
            Logger.Log("Started " + numberOfGames.ToString() + "  Games");
        }



        static void open8PlayersGame(TCPPacketConnection conn)
        {
            ClientSessionRequest sessionRequest = new ClientSessionRequest();
            sessionRequest.Reconnect = false;
            sessionRequest.ReconnectSessionID = 0;

            sendWithAdapter(conn, sessionRequest);

            ClientInitializeGamePackage initGame = new ClientInitializeGamePackage();
            initGame.GamePlayerCount = 6;
            initGame.PlayerTeamwish = new int[3];
            initGame.PlayerTeamwish[0] = 0;
            initGame.PlayerTeamwish[1] = 1;
            initGame.PlayerTeamwish[2] = 0;
            sendWithAdapter(conn, initGame);

        }

        static void open8PlayersGame2(TCPPacketConnection conn)
        {
            ClientSessionRequest sessionRequest = new ClientSessionRequest();
            sessionRequest.Reconnect = false;
            sessionRequest.ReconnectSessionID = 0;

            sendWithAdapter(conn, sessionRequest);

            ClientInitializeGamePackage initGame = new ClientInitializeGamePackage();
            initGame.GamePlayerCount = 6;
            initGame.PlayerTeamwish = new int[3];
            initGame.PlayerTeamwish[0] = 1;
            initGame.PlayerTeamwish[1] = 0;
            initGame.PlayerTeamwish[2] = 1;
            sendWithAdapter(conn, initGame);

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
    }
}
