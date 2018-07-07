using System.Collections.Generic;
using XSLibrary.ThreadSafety.Containers;

namespace PingPongServer.Matchmaking
{
    public partial class Matchmaker
    {
        Dictionary<int, Filter> m_waitingForMatch = new Dictionary<int, Filter>();

        public SafeStack<Request[]> Matches { get; private set; } = new SafeStack<Request[]>();

        public bool AddRequestToQueue(int id, int maxPlayerCount, int[] teamWishes)
        {
            return AddRequestToQueue(new Request(id, maxPlayerCount, teamWishes));
        }

        public bool AddRequestToQueue(Request request)
        {
            if (!IsRequestValid(request))
                return false;

            AddRequest(request);
            return true;
        }
        
        public void RemoveSearchingClient(int requestID)
        {
            foreach (KeyValuePair<int, Filter> keyvalue in m_waitingForMatch)
            {
                foreach (RequestGroup requestGroup in keyvalue.Value.RequestGroups)
                {
                    foreach (Request request in requestGroup.m_requests)
                    {
                        if(request.ID == requestID)
                        {
                            requestGroup.m_requests.Remove(request);
                            return;
                        }
                    }
                }
            }
        }
        
        private void AddRequest(Request request)
        {
            int maxPlayerCount = request.MaxPlayerCount;

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

            sameSizedFilter.AddRequest(request);
        }

        private Request FindRequest(int requestID)
        {
            foreach (KeyValuePair<int, Filter> keyvalue in m_waitingForMatch)
            {
                foreach (RequestGroup requestGroup in keyvalue.Value.RequestGroups)
                {
                    foreach (Request request in requestGroup.m_requests)
                    {
                        if (request.ID == requestID)
                            return request;
                    }
                }
            }

            return null;
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
                entry.FindMatches();
        }

        private void HandleFoundMatch(object sender, Request[] requests)
        {
            Matches.Write(requests);
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

            if (FindRequest(request.ID) != null)
                return false;

            return true;
        }
    }
}
