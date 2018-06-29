using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        Dictionary<int, Filter> m_waitingForMatch = new Dictionary<int, Filter>();

        public bool AddRequestToQueue(NetworkConnection connection, int maxPlayerCount, int[] teamWishes)
        {
            Request request = new Request(connection.ClientSession.SessionID, maxPlayerCount, teamWishes);

            if (!IsRequestValid(request))
                return false;

            AddSearchingClient(request);
            return true;
        }
        
        private void AddSearchingClient(Request client)
        {
            int maxPlayerCount = client.MaxPlayerCount;

            Filter sameSizedFilter = null;

            if (GameSizeOpen(maxPlayerCount))
            {
                sameSizedFilter = m_waitingForMatch[maxPlayerCount];
            }
            else
            {
                sameSizedFilter = new Filter(maxPlayerCount);
                sameSizedFilter.FoundValidCombination += HandleFoundMatch;
                m_waitingForMatch.Add(maxPlayerCount, sameSizedFilter);
            }

            sameSizedFilter.AddRequest(client);
        } 

        private bool GameSizeOpen(int maxPlayerCount)
        {
            return m_waitingForMatch.ContainsKey(maxPlayerCount);
        } 

        public void FindMatch()
        {
            int maxPlayerCount = 8;

            AddSearchingClient(new Request(0, maxPlayerCount, new int[2] { 0, 0 }));
            AddSearchingClient(new Request(1, maxPlayerCount, new int[3] { 0, 0, 1 }));
            AddSearchingClient(new Request(2, maxPlayerCount, new int[3] { 0, 0, 1 }));


            foreach (Filter entry in m_waitingForMatch.Values)
            {
                UpdateFilter(entry);
            }
        }

        private void UpdateFilter(Filter filter)
        {
            filter.SearchNewCombinations();
            filter.SearchValidCombinations();
        }

        private void HandleFoundMatch(object sender, Request[] requests)
        {
            foreach (Request request in requests)
            {
                int[] placements = request.GetPlayerPlacements();
            }
        }

        bool IsRequestValid(Request request)
        {
            if (request.MaxPlayerCount % 2 != 0)
                return false;

            if (request.ClientPlayerCount > request.MaxPlayerCount)
                return false;

            if (request.Team1Count > request.TeamSize || request.Team2Count > request.TeamSize)
                return false;

            return true;
        }
    }
}
