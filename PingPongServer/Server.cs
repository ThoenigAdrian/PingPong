using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NetworkLibrary.Utility;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;

using XSLibrary.Network.Connections;
using XSLibrary.Network.Accepters;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer
{
    public class Server : IDisposable
    {

        private ClientRegistration Registration { get; set; }
        private MatchmakingManager MatchManager = new MatchmakingManager();
        private GamesManager GamesManager;

        // Logging
        private LogWriterConsole Logger { get; set; } = new LogWriterConsole();
        public bool ServerStopping { get; private set; }

        // Configuration 
        private ServerConfiguration ServerConfiguration;

        class QueueRequest
        {
            public NetworkConnection Connection { get; private set; }
            public ClientInitializeGamePackage InitData { get; private set; }

            public QueueRequest(NetworkConnection connection, ClientInitializeGamePackage initData)
            {
                Connection = connection;
                InitData = initData;
            }
        }
        SafeStack<QueueRequest> MatchmakingRequests = new SafeStack<QueueRequest>();

        public Server()
        {
            ServerConfiguration = new ServerConfiguration();

            Registration = new ClientRegistration(ServerConfiguration, Logger);
            Registration.OnMatchmakingRequest += HandleMatchmakingRequest;
            Registration.OnObserverRequest += HandleObserveRequest;

            UDPConnection MasterUDPSocket = new UDPConnection(new IPEndPoint(IPAddress.Any, ServerConfiguration.ServerPort));
            MasterUDPSocket.OnDisconnect += MasterUDPSocket_OnDisconnect;
            MasterUDPSocket.Logger = Logger;
            
            GamesManager = new GamesManager(MasterUDPSocket);
            MatchManager.OnMatchFound += GamesManager.OnMatchmadeGameFound;
        }

        private void MasterUDPSocket_OnDisconnect(object sender, EventArgs e)
        {
            if (!ServerStopping)
                throw new Exception("UDP Socket got disconnected");
        }

        public void Run()
        {
            Logger.Log("\n\n");
            Logger.ServerLog("Entering Server Run Method");
            Logger.ServerLog("Starting Connection Accepter");
            Registration.Run();
            Logger.ServerLog("Starting Games Manager");
            GamesManager.Run();

            Logger.ServerLog("Server is now entering it's main Loop\n");

            while (!ServerStopping)
            {
                MatchManager.TotalPlayersOnline = NumberOfPlayersOnline();
                MatchManager.Update();

                QueueRequest request;
                while ((request = MatchmakingRequests.Read()) != null)
                {
                    MatchManager.AddClientToQueue(request.Connection, request.InitData);
                }

                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }
        }

        public void Stop()
        {
            Logger.ServerLog("Server Stop has been requested");
            ServerStopping = true;
            Logger.ServerLog("Stopping Games Manager");
            GamesManager.Stop();
        }

        private void HandleObserveRequest(object sender, NetworkConnection connection)
        {
            GamesManager.AddObserver(connection);
        }

        private void HandleMatchmakingRequest(object sender, NetworkConnection connection, PackageInterface request)
        {
            MatchmakingRequests.Write(new QueueRequest(connection, request as ClientInitializeGamePackage));
        }

        private int NumberOfPlayersOnline()
        {
            return GamesManager.PlayersCurrentlyInGames() + MatchManager.TotalPlayersSearching() + Registration.RegisteredPlayersCount;
        }

        public void Dispose()
        {
            Stop();
        }             
    }
}