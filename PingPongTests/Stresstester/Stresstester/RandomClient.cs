using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester
{
    class RandomMovementClient
    {
        static void randomClient()
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
                    Thread.Sleep(500);
                }


            }

            while (true)
            {
                foreach (int playerID in playerIDs)
                {
                    sendRandomMovemet(networkConn, playerID);
                    Thread.Sleep(RandomGenerator.Next(200));
                }

            }
        }


        static void sendRandomMovemet(NetworkConnection conn, int playerID)
        {
            List<ClientMovement> movements = new List<ClientMovement> { ClientMovement.Down, ClientMovement.Up, ClientMovement.StopMoving, ClientMovement.NoInput };
            ServerDataPackage s = conn.ReadUDP() as ServerDataPackage;
            PlayerMovementPackage movementPackage = new PlayerMovementPackage();
            movementPackage.PlayerID = playerID;
            movementPackage.PlayerMovement = movements[RandomGenerator.Next(4)];
            conn.SendTCP(movementPackage);

        }
    }
}
