using NetworkLibrary.DataPackages.ClientSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using PingPongServer.Matchmaking;
using System;
using System.Collections.Generic;
using XSLibrary.Utility;

namespace PingPongServer
{
    class MatchmakingManager
    {
        public delegate void MatchFoundHandler(object sender, MatchData matchData);
        public event MatchFoundHandler OnMatchFound;
        public int TotalPlayersOnline = 0;
        private int MatchmakingRefreshIntervalInSeconds = 5;
        private OneShotTimer UpdateMatchmakingQueue;

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

        public MatchmakingManager()
        {
            UpdateMatchmakingQueue = new OneShotTimer(MatchmakingRefreshIntervalInSeconds * 1000000, true);
            
        }
        private Matchmaker Matchmaking { get; set; } = new Matchmaker();
        public List<NetworkConnection> m_waitingClientConnections = new List<NetworkConnection>();

        public void AddClientToQueue(NetworkConnection clientConnection, ClientInitializeGamePackage initData)
        {
            if (Matchmaking.AddRequestToQueue(clientConnection.ClientSession.SessionID, initData.GamePlayerCount, initData.PlayerTeamwish))
            {
                m_waitingClientConnections.Add(clientConnection);
                SendMatchmakingStatus(clientConnection, string.Format(WAITING_IN_QUEUE, TotalPlayersOnline, TotalPlayersSearching()));
                Matchmaking.TotalPlayersSearching();
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

            for (int i = 0; i < Matchmaking.Matches.Count; i++)
            {
                MatchData matchData = GenerateMatchData(Matchmaking.Matches[i]);
                if (matchData == null)
                    continue;

                RemoveClientsFromQueue(matchData);
                Matchmaking.Matches.RemoveAt(i);
                i--;

                OnMatchFound?.Invoke(this, matchData);
            }

            if (UpdateMatchmakingQueue == true)
            {
                BroadcastMatchmakingStatus();
                UpdateMatchmakingQueue.Restart();
            }

        }
        
        private void BroadcastMatchmakingStatus()
        {
            foreach (NetworkConnection clientConnection in m_waitingClientConnections)
                SendMatchmakingStatus(clientConnection, string.Format(WAITING_IN_QUEUE, TotalPlayersOnline, TotalPlayersSearching()));
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
        

        private MatchData GenerateMatchData(Request[] match)
        {
            MatchData matchData = new MatchData();
            matchData.MaxPlayerCount = match[0].MaxPlayerCount;

            foreach (Request request in match)
            {
                NetworkConnection clientConnection = FindClientConnection(request.ID);
                if (clientConnection == null) // todo
                {
                    HandleConnectionNotFound(match);
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

        private NetworkConnection FindClientConnection(int clientID)
        {
            foreach(NetworkConnection connection in m_waitingClientConnections)
            {
                if (connection.ClientSession.SessionID == clientID)
                    return connection;
            }

            return null;
        }

        private void RemoveClientsFromQueue(MatchData matchData)
        {
            foreach (ClientData client in matchData.Clients)
            {
                m_waitingClientConnections.Remove(client.m_clientConnection);
            }
        }

        public bool IsRequestValid(Request request)
        {
            return Matchmaking.IsRequestValid(request);
        }

        private void HandleConnectionNotFound(Request[] match)
        {
            // todo
        }
    }
}
