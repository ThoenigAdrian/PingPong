using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NetworkLibrary;
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
        // Network 
        private TCPAccepter ConnectionAccepter;
        private UDPConnection MasterUDPSocket;
        private SafeList<NetworkConnection> ConnectionsReadyForQueingUpToMatchmaking = new SafeList<NetworkConnection>();
        private SafeList<NetworkConnection> AcceptedConnections = new SafeList<NetworkConnection>();
        private MatchmakingManager MatchManager = new MatchmakingManager();

        // Game
        private GamesManager GamesManager;

        // Logging
        private LogWriterConsole Logger { get; set; } = new LogWriterConsole();
        private bool m_stopServer;
        public bool ServerStopping { get { return m_stopServer; } }

        // Configuration 
        private ServerConfiguration ServerConfiguration;


        public Server()
        {
            ServerConfiguration = new ServerConfiguration();

            ConnectionAccepter = new TCPAccepter(ServerConfiguration.ServerPort, ServerConfiguration.MaximumNumberOfIncomingConnections);
            ConnectionAccepter.ClientConnected += OnSocketAccept;

            MasterUDPSocket = new UDPConnection(new IPEndPoint(IPAddress.Any, ServerConfiguration.ServerPort));
            MasterUDPSocket.OnDisconnect += MasterUDPSocket_OnDisconnect;
            MasterUDPSocket.Logger = Logger;
            
            GamesManager = new GamesManager(MasterUDPSocket);
            MatchManager.OnMatchFound += GamesManager.StartMatchmadeGame;
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
            ConnectionAccepter.Run();
            Logger.ServerLog("Starting Games Manager");
            GamesManager.Run();

            Logger.ServerLog("Server is now entering it's main Loop\n");

            while (!m_stopServer)
            {
                foreach (NetworkConnection networkConnection in AcceptedConnections.Entries)
                    ProcessClientSessionRequest(networkConnection);

                ServeClientGameRequests();

                RemoveDeadConnections();
                MatchManager.TotalPlayersOnline = NumberOfPlayersOnline();
                MatchManager.Update();
                Thread.Sleep(1000); // Sleep so we don't hog CPU Resources 
            }
        }

        public void Stop()
        {
            Logger.ServerLog("Server Stop has been requested");
            m_stopServer = true;
            Logger.ServerLog("Stopping Connection Accepter");
            ConnectionAccepter.Stop();
            Logger.ServerLog("Stopping Games Manager");
            GamesManager.Stop();
        }

        private void OnSocketAccept(object sender, Socket acceptedSocket)
        {
            Logger.NetworkLog("Client connected " + acceptedSocket.RemoteEndPoint.ToString());
            TCPPacketConnection tcp = new TCPPacketConnection(acceptedSocket);
            NetworkConnection newNetworkConnection = new NetworkConnection(tcp);
            Logger.ServerLog("Adding Client to Accepted Connections");
            AcceptedConnections.Add(newNetworkConnection);
        }


        private void ProcessClientSessionRequest(NetworkConnection networkConnection)
        {
            PackageInterface newPacket = networkConnection.ReadTCP();
            if (newPacket == null)
                return;

            if (newPacket.PackageType == PackageType.ClientSessionRequest)
            {
                ClientSessionRequest packet = (ClientSessionRequest)newPacket;

                if (packet.Reconnect)
                    ReconnectClientWithPreviousSession(networkConnection, packet);

                else
                    ConnectClientWithNewSession(networkConnection);

                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = networkConnection.ClientSession.SessionID;
                networkConnection.SendTCP(response);
            }
        }

        private int NumberOfPlayersOnline()
        {
            return GamesManager.PlayersCurrentlyInGames() + MatchManager.TotalPlayersSearching() + ConnectionsReadyForQueingUpToMatchmaking.Count;
        }

        private void ServeClientGameRequests()
        {
            foreach (NetworkConnection conn in ConnectionsReadyForQueingUpToMatchmaking.Entries)
            {
                PackageInterface packet = conn.ReadTCP();
                if (packet == null)
                    continue;

                if (packet.PackageType == PackageType.ClientInitalizeGamePackage)
                {
                    ClientInitializeGamePackage initPackage = packet as ClientInitializeGamePackage;
                    switch (initPackage.Request)
                    {
                        case ClientInitializeGamePackage.RequestType.Matchmaking:
                            MatchManager.AddClientToQueue(conn, initPackage);
                            ConnectionsReadyForQueingUpToMatchmaking.Remove(conn);
                            break;
                    }
                }
            }
        }

        private void ConnectClientWithNewSession(NetworkConnection networkConnection)
        {
            Logger.NetworkLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") wants to connect ");
            networkConnection.ClientSession = new Session(new Random().Next());
            Logger.NetworkLog("Assigned Session " + networkConnection.ClientSession.SessionID + " to Client " + networkConnection.RemoteEndPoint.ToString());
            ConnectionsReadyForQueingUpToMatchmaking.Add(networkConnection);
            AcceptedConnections.Remove(networkConnection);
        }

        private void ReconnectClientWithPreviousSession(NetworkConnection networkConnection, ClientSessionRequest packet)
        {
            // still need to handle if client requests a session which is already in use
            Logger.NetworkLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") want's to reconnect with Session ID " + packet.ReconnectSessionID.ToString());
            networkConnection.ClientSession = new Session(packet.ReconnectSessionID);
            ConnectionsReadyForQueingUpToMatchmaking.Add(networkConnection);
            AcceptedConnections.Remove(networkConnection);
            if (GamesManager.RejoinClientToGame(networkConnection))
            {
                ConnectionsReadyForQueingUpToMatchmaking.Remove(networkConnection);
            }
        }
        

        private void RemoveDeadConnections()
        {
            foreach(NetworkConnection networkConnection in AcceptedConnections.Entries)
            {
                if (!networkConnection.Connected)
                {
                    Logger.NetworkLog("Removing disconnected connection from Accepted Connection ( Client :  " + networkConnection.RemoteEndPoint.ToString() + ")");
                    AcceptedConnections.Remove(networkConnection);
                }
            }
            foreach (NetworkConnection networkConnection in ConnectionsReadyForQueingUpToMatchmaking.Entries)
            {
                if (!networkConnection.Connected)
                {
                    Logger.NetworkLog("Removing disconnected connection from ConnectionsReadyForJoiningAndStarting Games Connection ( Client :  " + networkConnection.RemoteEndPoint.ToString() + ")");
                    ConnectionsReadyForQueingUpToMatchmaking.Remove(networkConnection);
                }
            }
        }
        
        public void Dispose()
        {
            Stop();
        }
                
    }
}
