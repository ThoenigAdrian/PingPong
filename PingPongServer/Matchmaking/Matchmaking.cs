using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        Dictionary<int, MatchingEntry> m_waitingForMatch = new Dictionary<int, MatchingEntry>();

        public bool AddRequestToQueue(NetworkConnection connection, int maxPlayerCount, int[] teamWishes)
        {
            ClientData request = new ClientData()
            {
                ClientConnection = connection,
                MaxPlayerCount = maxPlayerCount,
                TeamWishes = teamWishes
            };

            if (!IsRequestValid(request))
                return false;

            AddSearchingClient(request);
            return true;
        }
        
        private void AddSearchingClient(ClientData client)
        {
            int maxPlayerCount = client.MaxPlayerCount;

            MatchingEntry sameSizedEntry = null;

            if (GameSizeOpen(maxPlayerCount))
            {
                sameSizedEntry = m_waitingForMatch[maxPlayerCount];
            }
            else
            {
                sameSizedEntry = new MatchingEntry();
                sameSizedEntry.Game = new Game() { MaxPlayerCount = maxPlayerCount };
                m_waitingForMatch.Add(maxPlayerCount, sameSizedEntry);
            }

            sameSizedEntry.SearchingClients.Add(client);
        } 

        private bool GameSizeOpen(int maxPlayerCount)
        {
            return m_waitingForMatch.ContainsKey(maxPlayerCount);
        } 

        void FindMatch()
        {
            
        }

        bool IsRequestValid(ClientData request)
        {
            if (request.Team1Count > request.MaxTeamSize
                || request.Team1Count < request.MinTeamSize
                || request.Team2Count > request.MaxTeamSize
                || request.Team2Count < request.MinTeamSize)
                return false;

            return true;
        }
    }
}
