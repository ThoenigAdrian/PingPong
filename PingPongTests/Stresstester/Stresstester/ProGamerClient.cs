using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester
{
    class ProGamerClient
    {
        static void proGamerClient()
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectionSocket.Connect(server);
            TCPPacketConnection conn = new TCPPacketConnection(connectionSocket);
            connections.Add(conn);
            open8PlayersGame(conn);
            NetworkConnection networkConn = new NetworkConnection(conn);
            UDPConnection c = new UDPConnection(conn.Local);
            c.InitializeReceiving();
            networkConn.SetUDPConnection(c);
            List<int> playerIDs = new List<int>();
            while (true)
            {
                PackageInterface p = networkConn.ReadTCP();
                if (p != null && p.PackageType == PackageType.ServerPlayerIDResponse)
                {
                    ServerInitializeGameResponse ps = p as ServerInitializeGameResponse;
                    foreach (Player pl in ps.m_players)
                    {
                        if (pl.Controllable)
                            playerIDs.Add(pl.ID);
                    }
                    break;

                }
                else
                {
                    Thread.Sleep(100);
                }


            }

            while (true)
            {
                foreach (int playerID in playerIDs)
                {
                    sendMovementToMatchBall(networkConn, playerID);
                }
                Thread.Sleep(100);

            }
        }

        static void proGamerClient()
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectionSocket.Connect(server);
            TCPPacketConnection conn = new TCPPacketConnection(connectionSocket);
            connections.Add(conn);
            open8PlayersGame(conn);
            NetworkConnection networkConn = new NetworkConnection(conn);
            UDPConnection c = new UDPConnection(conn.Local);
            c.InitializeReceiving();
            networkConn.SetUDPConnection(c);
            List<int> playerIDs = new List<int>();
            while (true)
            {
                PackageInterface p = networkConn.ReadTCP();
                if (p != null && p.PackageType == PackageType.ServerPlayerIDResponse)
                {
                    ServerInitializeGameResponse ps = p as ServerInitializeGameResponse;
                    foreach (Player pl in ps.m_players)
                    {
                        if (pl.Controllable)
                            playerIDs.Add(pl.ID);
                    }
                    break;

                }
                else
                {
                    Thread.Sleep(100);
                }


            }

            while (true)
            {
                foreach (int playerID in playerIDs)
                {
                    sendMovementToMatchBall(networkConn, playerID);
                }
                Thread.Sleep(100);

            }
        }

        static void sendMovementToMatchBall(NetworkConnection conn, int playerID)
        {
            ServerDataPackage s = conn.ReadUDP() as ServerDataPackage;
            PlayerMovementPackage movementPackage = new PlayerMovementPackage();
            RawPlayer correctPlayer = null;
            movementPackage.PlayerID = playerID;
            if (s == null)
                return;
            foreach (RawPlayer p in s.Players)
            {
                if (p.ID == playerID)
                {
                    correctPlayer = p;
                    break;
                }

            }
            if (s.Ball.PositionY < correctPlayer.PositionY + GameInitializers.GetPlayerHeight(8) / 2)
            {
                movementPackage.PlayerMovement = ClientMovement.Up;
            }
            else
            {
                movementPackage.PlayerMovement = ClientMovement.Down;
            }
            conn.SendTCP(movementPackage);
        }
    }
}
