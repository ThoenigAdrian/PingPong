using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using PingPongServer.Matchmaking;
using System;
using System.Collections.Generic;
using System.Net;
using XSLibrary.ThreadSafety.Containers;
using XSLibrary.Utility;

namespace PingPongServer
{
    public class MatchmakingManager
    {
        public delegate void MatchFoundHandler(object sender, MatchData matchData);
        public event MatchFoundHandler OnMatchFound;

        public SafeList<NetworkConnection> m_waitingClientConnections = new SafeList<NetworkConnection>();
        public int TotalPlayersOnline = 0;

        private int MatchmakingRefreshIntervalInSeconds = 5;
        private OneShotTimer UpdateMatchmakingQueue;
        private Matchmaker Matchmaking { get; set; } = new Matchmaker();
        private UniqueIDGenerator SessionIDGenerator;
        public Func<int> TotalPlayersOnlineCallback { get; internal set; }

        const string WAITING_IN_QUEUE = "Waiting for additional players to start the game... \nPlayers online: {0} \nPlayers searching: {1} ";
        const string INVALID_REQUEST = "Invalid game options. Check your configuration!";

        public class MatchData
        {
            public int MaxPlayerCount { get; set; }
            public List<ClientData> Clients { get; set; } = new List<ClientData>();
        }

        public class ClientData
        {
            public NetworkConnection m_clientConnection;
            public Request m_request;
        }

        public MatchmakingManager(UniqueIDGenerator sessionIDGenerator)
        {
            SessionIDGenerator = sessionIDGenerator;
            UpdateMatchmakingQueue = new OneShotTimer(MatchmakingRefreshIntervalInSeconds * 1000000, true);
        }

        public void AddClientToQueue(NetworkConnection clientConnection, ClientInitializeGamePackage initData)
        {
            if (Matchmaking.AddRequestToQueue(clientConnection.ClientSession.SessionID, initData.GamePlayerCount, initData.PlayerTeamwish))
            {
                m_waitingClientConnections.Add(clientConnection);
                SendMatchmakingStatus(clientConnection, string.Format(WAITING_IN_QUEUE, TotalPlayersOnlineCallback(), TotalPlayersSearching()));
                clientConnection.ConnectionDiedEvent += RemoveConnection;
            }
            else
                SendMatchmakingError(clientConnection, INVALID_REQUEST);
        }

        public int TotalPlayersSearching()
        {
            return Matchmaking.TotalPlayersSearching();
        }

        public void Update()
        {
            Matchmaking.FindMatches();

            CombineConnectionsWithMatches();

            foreach (NetworkConnection connection in m_waitingClientConnections.Entries)
            {
                if (!connection.Connected)
                    RemoveClientFromQueue(connection);
            }

            if (UpdateMatchmakingQueue == true)
            {
                BroadcastMatchmakingStatus();
                UpdateMatchmakingQueue.Restart();
            }

            m_waitingClientConnections.TrimExcess();    // Helps debugging Memory Leaks 
        }

        private bool IsRequestValid(Request request)
        {
            return Matchmaking.IsRequestValid(request);
        }

        private void CombineConnectionsWithMatches()
        {
            Request[] combination;
            while ((combination = Matchmaking.Matches.Read()) != null)
            {
                MatchData matchData = GenerateMatchData(combination);
                if (matchData == null)
                    continue;

                RemoveConnectionsFromQueue(matchData);
                OnMatchFound?.Invoke(this, matchData);
            }
        }

        private MatchData GenerateMatchData(Request[] combination)
        {
            MatchData matchData = new MatchData();
            matchData.MaxPlayerCount = combination[0].MaxPlayerCount;

            foreach (Request request in combination)
            {
                NetworkConnection clientConnection = FindClientConnection(request.ID);
                if (clientConnection == null)
                {
                    HandleConnectionNotFound(combination, request.ID);
                    return null;
                }

                ClientData clientData = new ClientData()
                {
                    m_clientConnection = clientConnection,
                    m_request = request
                };

                matchData.Clients.Add(clientData);
            }

            return matchData;
        }

        private void BroadcastMatchmakingStatus()
        {
            foreach (NetworkConnection clientConnection in m_waitingClientConnections.Entries)
                SendMatchmakingStatus(clientConnection, string.Format(WAITING_IN_QUEUE, TotalPlayersOnlineCallback(), TotalPlayersSearching()));
        }

        private void SendMatchmakingStatus(NetworkConnection clientConnection, string statusMessage)
        {
            ServerMatchmakingStatusResponse response = new ServerMatchmakingStatusResponse();
            response.Error = false;
            response.GameFound = false;
            response.Status = statusMessage;

            clientConnection.SendTCP(response);
        }

        private void SendMatchmakingError(NetworkConnection clientConnection, string errorMessage)
        {
            ServerMatchmakingStatusResponse response = new ServerMatchmakingStatusResponse();
            response.Error = true;
            response.GameFound = false;
            response.Status = errorMessage;

            clientConnection.SendTCP(response);
        }

        private void RemoveConnectionsFromQueue(MatchData matchData)
        {
            foreach (ClientData client in matchData.Clients)
                RemoveConnection(client.m_clientConnection);
        }

        private void RemoveClientFromQueue(NetworkConnection client)
        {
            RemoveConnection(client);
            Matchmaking.RemoveSearchingClient(client.ClientSession.SessionID);
        }

        private void RemoveConnection(int clientID)
        {
            NetworkConnection connection = FindClientConnection(clientID);
            if (connection != null)
                RemoveConnection(connection);
        }

        private void RemoveConnection(NetworkConnection client)
        {
            client.ConnectionDiedEvent -= RemoveConnection;
            m_waitingClientConnections.Remove(client);
            SessionIDGenerator.FreeID(client.ClientSession.SessionID);
        }

        private void RemoveConnection(NetworkConnection connection, EndPoint remote)
        {
            RemoveConnection(connection);
        }

        private NetworkConnection FindClientConnection(int clientID)
        {
            foreach (NetworkConnection connection in m_waitingClientConnections.Entries)
            {
                if (connection.ClientSession.SessionID == clientID)
                    return connection;
            }

            return null;
        }

        private void HandleConnectionNotFound(Request[] combination, int invalidID)
        {
            foreach (Request request in combination)
            {
                if(request.ID != invalidID)
                    Requeue(request);
            }
        }

        private void Requeue(Request request)
        {
            if (!Matchmaking.AddRequestToQueue(request))
                RemoveConnection(request.ID);
        }
    }
}
