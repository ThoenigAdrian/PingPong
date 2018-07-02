using System.Collections.Generic;

namespace PingPongServer.Matchmaking
{
    public partial class Matchmaker
    {
        Dictionary<int, Filter> m_waitingForMatch = new Dictionary<int, Filter>();

        public List<Request[]> Matches { get; private set; } = new List<Request[]>();

        public bool AddRequestToQueue(int id, int maxPlayerCount, int[] teamWishes)
        {
            return AddRequestToQueue(new Request(id, maxPlayerCount, teamWishes));
        }

        public bool AddRequestToQueue(Request request)
        {
            if (!IsRequestValid(request))
                return false;

            AddSearchingClient(request);
            return true;
        }
        
        public void RemoveSearchingClient(int clientSessionID)
        {
            List<Request> requestsToBeRemoved = new List<Request>();
            foreach (KeyValuePair<int, Filter> keyvalue in m_waitingForMatch)
            {
                foreach (RequestGroup requestGroup in keyvalue.Value.RequestGroups)
                {
                    foreach (Request request in requestGroup.m_requests)
                    {
                        if(request.ID == clientSessionID)
                        {
                            requestsToBeRemoved.Add(request);
                        }
                    }
                    foreach (Request requestToBeRemoved in requestsToBeRemoved)
                        requestGroup.m_requests.Remove(requestToBeRemoved);
                }
            }

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


        public int TotalPlayersSearching()
        {
            int TotalPlayers = 0;
            foreach (KeyValuePair<int, Filter> keyvalue in m_waitingForMatch)
            {
                foreach (RequestGroup requestGroup in keyvalue.Value.RequestGroups)
                {
                    foreach (Request request in requestGroup.m_requests)
                    {
                        TotalPlayers += request.ClientPlayerCount;
                    }
                }
            }
            return TotalPlayers;
        }

        private bool GameSizeOpen(int maxPlayerCount)
        {
            return m_waitingForMatch.ContainsKey(maxPlayerCount);
        } 

        public void FindMatches()
        {
            foreach (Filter entry in m_waitingForMatch.Values)
            {
                UpdateFilter(entry);
            }
        }

        private void UpdateFilter(Filter filter)
        {
            if (filter.Changes)
            {
                filter.SearchNewCombinations();
                filter.SearchValidCombinations();
            }
        }

        private void HandleFoundMatch(object sender, Request[] requests)
        {
            Matches.Add(requests);
        }

        public bool IsRequestValid(Request request)
        {
            if (request.MaxPlayerCount % 2 != 0)
                return false;

            if (request.ClientPlayerCount < 1)
                return false;

            if (request.ClientPlayerCount > request.MaxPlayerCount)
                return false;

            if (request.Team1Count > request.TeamSize || request.Team2Count > request.TeamSize)
                return false;

            return true;
        }
    }
}
