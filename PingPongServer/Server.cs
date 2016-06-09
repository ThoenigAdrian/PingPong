using NetworkLibrary;
using System.Net.Sockets;
using System.Net;
using NetworkLibrary.Utility;
using System.Threading;
using GameLogicLibrary;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.DataPackages;

namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            Socket MasterListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MasterListeningSocket.Bind(new IPEndPoint(IPAddress.Any, NetworkConstants.SERVER_PORT));
            MasterListeningSocket.Listen(1); // Allow two Clients for now
            LogWriter Logger = new LogWriterConsole();

            PackageAdapter adapter = new PackageAdapter();

            Socket acceptSocket = MasterListeningSocket.Accept();

            ServerNetwork serverNetwork = new ServerNetwork(acceptSocket, Logger);

            //Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Logger.Log("Client connected");

            SpamUDPPositionData(serverNetwork);
        }

        static void SpamUDPSocket(Socket udpSocket, PackageAdapter adapter, IPEndPoint client, LogWriter Logger)
        {
            ServerDataPackage package = new ServerDataPackage();
            package.BallPosX = 50;
            package.BallPosY = 50;
            int turn = 1;

            udpSocket.Connect(client);

            while (true)
            {

                package.BallPosX += 1 * turn;

                if (GameInitializers.BORDER_HEIGHT - 4 <= package.BallPosX)
                    turn *= -1;
                if (package.BallPosX <= 4)
                    turn *= -1;

                udpSocket.Send(adapter.CreateNetworkDataFromPackage(package));

                Logger.Log("Sent package to " + client.Address + ":" + client.Port);

                Thread.Sleep(1000);
            }
        }

        static void SpamUDPPositionData(ServerNetwork spamNetwork)
        {
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

                spamNetwork.SendObjectPositions(package);
                Thread.Sleep(16);
            }
        }
    }
}
