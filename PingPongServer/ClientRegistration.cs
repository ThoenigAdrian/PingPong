using System.Net.Sockets;
using System.Threading;
using XSLibrary.Network.Accepters;
using XSLibrary.Network.Connections;
using XSLibrary.ThreadSafety.Containers;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Collections.Generic;
using System;

namespace PingPongServer
{
    class ClientRegistration
    {
        public delegate void ConnectionStateChangedHandler(object sender, NetworkConnection connection);

        public delegate void ClientRequestHandler(object sender, NetworkConnection connection, PackageInterface request);
        public event ClientRequestHandler OnMatchmakingRequest;

        public delegate void ObserverRequestHandler(object sender, NetworkConnection connection);
        public event ObserverRequestHandler OnObserverRequest;

        public delegate bool RejoinClientToGame(NetworkConnection connection);
        private RejoinClientToGame RejoinCallback;

        private TCPAccepter ConnectionAccepter;
        private SafeList<NetworkConnection> ConnectionsReadyForQueingUpToMatchmaking = new SafeList<NetworkConnection>();
        private SafeList<NetworkConnection> AcceptedConnections = new SafeList<NetworkConnection>();
        
        
        UniqueIDGenerator SessionIDGenerator;
        GameLogger Logger;

        Thread m_registrationThread;
        volatile private bool m_abort = false;

        public int RegisteredPlayersCount { get { return ConnectionsReadyForQueingUpToMatchmaking.Count + AcceptedConnections.Count; } }

        public ClientRegistration(ServerConfiguration config, GameLogger log, RejoinClientToGame rejoinCallback, UniqueIDGenerator sessionIDGenerator)
        {
            Logger = log;
            InitRegistration(config);
            RejoinCallback = rejoinCallback;
            SessionIDGenerator = sessionIDGenerator;
        }

        private void InitRegistration(ServerConfiguration config)
        {
            ConnectionAccepter = new TCPAccepter(config.ServerPort, config.MaximumNumberOfIncomingConnections);
            ConnectionAccepter.ClientConnected += OnSocketAccept;
        }

        public void Run()
        {
            m_abort = false;
            m_registrationThread = new Thread(RegistrationLoop);
            m_registrationThread.Name = "Registration";
            m_registrationThread.Start();
            Logger.RegistrationLog("Starting Connection Accepter");
            ConnectionAccepter.Run();
        }

        private void RegistrationLoop()
        {
            while(!m_abort)
            {
                foreach (NetworkConnection connection in AcceptedConnections.Entries)
                    ReadSessionRequests(connection);

                foreach (NetworkConnection connection in ConnectionsReadyForQueingUpToMatchmaking.Entries)
                    ReadGameRequests(connection);

                RemoveDeadConnections();

                // helps debugging memory issues
                AcceptedConnections.TrimExcess();   
                ConnectionsReadyForQueingUpToMatchmaking.TrimExcess();
                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            m_abort = true;
            Logger.RegistrationLog("Stopping Connection Accepter");
            ConnectionAccepter.Stop();
            m_registrationThread.Join();
        }

        private void OnSocketAccept(object sender, Socket acceptedSocket)
        {
            Logger.RegistrationLog("Client connected " + acceptedSocket.RemoteEndPoint.ToString());
            TCPPacketConnection tcp = new TCPPacketConnection(acceptedSocket);
            NetworkConnection newNetworkConnection = new NetworkConnection(tcp);
            Logger.ServerLog("Adding Client to Accepted Connections");
            AcceptedConnections.Add(newNetworkConnection);

        }

        private void ReadSessionRequests(NetworkConnection networkConnection)
        {
            PackageInterface newPacket = networkConnection.ReadTCP();
            if (newPacket == null)
                return;

            if (newPacket.PackageType == PackageType.ClientSessionRequest)
                HandleSessionRequest(networkConnection, newPacket);

        }

        private void HandleSessionRequest(NetworkConnection connection, PackageInterface request)
        {
            ClientSessionRequest packet = (ClientSessionRequest)request;

            if (packet.Reconnect)
                ReconnectClientWithPreviousSession(connection, packet);
            else
                ConnectClientWithNewSession(connection);

        }

        private void ConnectClientWithNewSession(NetworkConnection networkConnection)
        {
            Logger.RegistrationLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") wants to connect ");
            networkConnection.ClientSession = new Session(SessionIDGenerator.GetID());
            Logger.RegistrationLog("Assigned Session " + networkConnection.ClientSession.SessionID + " to Client " + networkConnection.RemoteEndPoint.ToString());
            SendSessionResponse(networkConnection);
            ConnectionsReadyForQueingUpToMatchmaking.Add(networkConnection);
            AcceptedConnections.Remove(networkConnection);
        }

        private void ReconnectClientWithPreviousSession(NetworkConnection networkConnection, ClientSessionRequest packet)
        {
            Logger.RegistrationLog("Client  (" + networkConnection.RemoteEndPoint.ToString() + ") want's to reconnect with Session ID " + packet.ReconnectSessionID.ToString());
            networkConnection.ClientSession = new Session(packet.ReconnectSessionID);
            AcceptedConnections.Remove(networkConnection);
            if (RejoinCallback(networkConnection))
            {
                Logger.RegistrationLog("According to callback client could rejoin the game. Therefore Removing it from Accepted Connections List");
                AcceptedConnections.Remove(networkConnection);
            }
            else
            {
                Logger.RegistrationLog("According to callback client could NOT rejoin the game. ");
                Logger.RegistrationLog("Removing it from Accepted Connections List and adding it to Conections Ready for Matchmaking.");
                AcceptedConnections.Remove(networkConnection);
                Logger.RegistrationLog("Sending Normal Session Response. And assigning him a new Session ID");
                networkConnection.ClientSession = new Session(SessionIDGenerator.GetID());
                SendSessionResponse(networkConnection);
                ConnectionsReadyForQueingUpToMatchmaking.Add(networkConnection);
            }                
            
        }

        private void SendSessionResponse(NetworkConnection networkConnection)
        {
            ServerSessionResponse response = new ServerSessionResponse();
            response.ClientSessionID = networkConnection.ClientSession.SessionID;
            response.GameReconnect = false;
            networkConnection.SendTCP(response);
        }

        private void ReadGameRequests(NetworkConnection networkConnection)
        {
            PackageInterface newPacket = networkConnection.ReadTCP();
            if (newPacket == null)
                return;

            if (newPacket.PackageType == PackageType.ClientInitalizeGamePackage)
                HandleGameRequest(networkConnection, newPacket);
        }


        private void HandleGameRequest(NetworkConnection connection, PackageInterface request)
        {
            ClientInitializeGamePackage initPackage = request as ClientInitializeGamePackage;
            switch (initPackage.Request)
            {
                case ClientInitializeGamePackage.RequestType.Matchmaking:
                    ConnectionsReadyForQueingUpToMatchmaking.Remove(connection);
                    OnMatchmakingRequest?.Invoke(this, connection, request);
                    break;
                case ClientInitializeGamePackage.RequestType.Observe:
                    ConnectionsReadyForQueingUpToMatchmaking.Remove(connection);
                    OnObserverRequest?.Invoke(this, connection);
                    break;
            }
        }

        private void RemoveDeadConnections()
        {
            foreach (NetworkConnection networkConnection in AcceptedConnections.Entries)
            {
                if (!networkConnection.Connected)
                {
                    Logger.RegistrationLog("Removing disconnected connection from Accepted Connection ( Client :  " + networkConnection.RemoteEndPoint.ToString() + ")");
                    AcceptedConnections.Remove(networkConnection);
                }
            }

            foreach (NetworkConnection networkConnection in ConnectionsReadyForQueingUpToMatchmaking.Entries)
            {
                if (!networkConnection.Connected)
                {
                    Logger.RegistrationLog("Removing disconnected connection from ConnectionsReadyForJoiningAndStarting Games Connection ( Client :  " + networkConnection.RemoteEndPoint.ToString() + ")");
                    ConnectionsReadyForQueingUpToMatchmaking.Remove(networkConnection);
                    SessionIDGenerator.FreeID(networkConnection.ClientSession.SessionID);
                }
            }
        }
    }
}
