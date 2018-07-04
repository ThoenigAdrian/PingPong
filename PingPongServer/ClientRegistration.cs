using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.Network.Accepters;
using XSLibrary.Network.Connections;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer
{
    class ClientRegistration
    {
        public delegate void ConnectionStateChangedHandler(object sender, NetworkConnection connection);
        public event ConnectionStateChangedHandler OnConnectionAccept;
        public event ConnectionStateChangedHandler OnSessionAssigned;

        public delegate void ClientRequestHandler(object sender, NetworkConnection connection, PackageInterface request);
        public event ClientRequestHandler OnSessionRequest;
        public event ClientRequestHandler OnGameRequest;
        public event ClientRequestHandler OnMatchmakingRequest;

        public delegate void ObserverRequestHandler(object sender, NetworkConnection connection);
        public event ObserverRequestHandler OnObserverRequest;

        private TCPAccepter ConnectionAccepter;
        private SafeList<NetworkConnection> ConnectionsReadyForQueingUpToMatchmaking = new SafeList<NetworkConnection>();
        private SafeList<NetworkConnection> AcceptedConnections = new SafeList<NetworkConnection>();

        GameLogger Logger;

        Thread m_registrationThread;
        volatile private bool m_abort = false;

        public int RegisteredPlayersCount { get { return ConnectionsReadyForQueingUpToMatchmaking.Count; } }

        public ClientRegistration(ServerConfiguration config, GameLogger log)
        {
            Logger = log;
            InitRegistration(config);
        }

        private void InitRegistration(ServerConfiguration config)
        {
            OnSessionRequest += HandleSessionRequest;
            OnGameRequest += HandleGameRequest;

            ConnectionAccepter = new TCPAccepter(config.ServerPort, config.MaximumNumberOfIncomingConnections);
            ConnectionAccepter.ClientConnected += OnSocketAccept;
        }

        public void Run()
        {
            m_abort = false;
            m_registrationThread = new Thread(RegistrationLoop);
            m_registrationThread.Name = "Registration";
            m_registrationThread.Start();

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

                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            m_abort = true;
            Logger.ServerLog("Stopping Connection Accepter");
            ConnectionAccepter.Stop();
            m_registrationThread.Join();
        }

        private void OnSocketAccept(object sender, Socket acceptedSocket)
        {
            Logger.NetworkLog("Client connected " + acceptedSocket.RemoteEndPoint.ToString());
            TCPPacketConnection tcp = new TCPPacketConnection(acceptedSocket);
            NetworkConnection newNetworkConnection = new NetworkConnection(tcp);
            Logger.ServerLog("Adding Client to Accepted Connections");
            AcceptedConnections.Add(newNetworkConnection);

            OnConnectionAccept?.Invoke(this, newNetworkConnection);
        }

        private void ReadSessionRequests(NetworkConnection networkConnection)
        {
            PackageInterface newPacket = networkConnection.ReadTCP();
            if (newPacket == null)
                return;

            if (newPacket.PackageType == PackageType.ClientSessionRequest)
                OnSessionRequest?.Invoke(this, networkConnection, newPacket);
        }

        private void HandleSessionRequest(object sender, NetworkConnection connection, PackageInterface request)
        {
            ClientSessionRequest packet = (ClientSessionRequest)request;

            if (packet.Reconnect)
                ReconnectClientWithPreviousSession(connection, packet);
            else
                ConnectClientWithNewSession(connection);

            ServerSessionResponse response = new ServerSessionResponse();
            response.ClientSessionID = connection.ClientSession.SessionID;
            connection.SendTCP(response);

            OnSessionAssigned?.Invoke(this, connection);
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
            AcceptedConnections.Remove(networkConnection);
            //if (GamesManager.RejoinClientToGame(networkConnection))
            //    return;

            ConnectionsReadyForQueingUpToMatchmaking.Add(networkConnection);
        }


        private void ReadGameRequests(NetworkConnection networkConnection)
        {
            PackageInterface newPacket = networkConnection.ReadTCP();
            if (newPacket == null)
                return;

            if (newPacket.PackageType == PackageType.ClientInitalizeGamePackage)
                OnGameRequest?.Invoke(this, networkConnection, newPacket);
        }


        private void HandleGameRequest(object sender, NetworkConnection connection, PackageInterface request)
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
    }
}
