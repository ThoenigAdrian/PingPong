using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class Game
        {
            List<Request> m_requests = new List<Request>();

            public int MaxPlayerCount { get; set; }
            public int TeamSize { get { return MaxPlayerCount / 2; } }
            public int CurrentPlayerCount
            {
                get
                {
                    int currentPlayerCount = 0;
                    foreach (Request client in m_requests)
                        currentPlayerCount += client.ClientPlayerCount;

                    return currentPlayerCount;
                }
            }

            public void AddRequest(Request client, int index = -1)
            {
                m_requests.Add(client);
            }

            public void ReplaceRequest(Request client, int index)
            {
                m_requests.RemoveAt(index);
                m_requests.Insert(index, client);
            }

            public bool GameReady()
            {
                bool ready = Full();
                ready &= FitsIntoGame();

                if (ready)
                    ApplyPlayerPositions();

                return ready;
            }

            private bool Full()
            {
                return CurrentPlayerCount == MaxPlayerCount;
            }

            private bool FitsIntoGame()
            {
                if (CurrentPlayerCount > MaxPlayerCount)
                    return false;

                if (m_requests.Count <= 0)
                    return true;

                int possibilities = 1 << (m_requests.Count - 1);

                for (int step = 0; step < possibilities; step++)
                {
                    for (int i = 0; i < m_requests.Count - 1; i++)
                    {
                        int clientIndex = m_requests.Count - 1 - i;

                        if (step % (2 << i) == (1 << i))
                            m_requests[clientIndex].SwapTeams();
                    }

                    if (CurrentlyFitting())
                        return true;
                }

                return false;
            }

            private bool CurrentlyFitting()
            {
                int team1 = 0;
                int team2 = 0;

                foreach (Request request in m_requests)
                {
                    team1 += request.Team1Count;
                    team2 += request.Team2Count;
                }

                return team1 <= TeamSize && team2 <= TeamSize;
            }

            private void ApplyPlayerPositions()
            {
                int currentTeam1Offset = 0;
                int currentTeam2Offset = 0;
                foreach (Request request in m_requests)
                {
                    request.Team1Offset = currentTeam1Offset;
                    request.Team2Offset = currentTeam2Offset;

                    currentTeam1Offset += request.Team1Count;
                    currentTeam2Offset += request.Team2Count;
                }
            }

            public void ResetRequests()
            {
                m_requests.Clear();
            }
        }
    }
}
