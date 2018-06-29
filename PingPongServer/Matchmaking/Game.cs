using System.Collections.Generic;
using System;

namespace PingPongServer.Matchmaking
{
    public partial class Matchmaker
    {
        private class Game
        {
            List<Request> m_requests = new List<Request>();

            public int MaxPlayerCount { get; private set; }
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

            public Game(int maxPlayerCount)
            {
                MaxPlayerCount = maxPlayerCount;
            }

            public void AddRequest(Request client)
            {
                m_requests.Add(client);
            }

            public void ReplaceRequest(Request client, int index)
            {
                if(index < m_requests.Count)
                    m_requests.RemoveAt(index);
                m_requests.Insert(index, client);
            }

            public Request[] GetRequests()
            {
                return m_requests.ToArray();
            }

            public bool GameReady()
            {
                bool ready = Full();
                ready &= !OneTeamSizeExceeds();
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
                    if (CurrentlyFitting())
                        return true;

                    for (int i = 0; i < m_requests.Count - 1 && step < possibilities - 1; i++)
                    {
                        if (step % (1 << i) == (1 << i) - 1)
                            m_requests[m_requests.Count - 1 - i].Reverse();
                    }
                }

                return false;
            }

            private bool OneTeamSizeExceeds()
            {
                Request maximumRequest = GetRequestWithBiggestTeam();
                int minimumPossibleTeamSize = GetCountBigTeam(maximumRequest) + (GetMinimumSum() - GetCountSmallTeam(maximumRequest));
                return minimumPossibleTeamSize > TeamSize;
            }

            private int GetMinimumSum()
            {
                int minimumSum = 0;
                foreach (Request request in m_requests)
                {
                    minimumSum += GetCountSmallTeam(request);
                }
                return minimumSum;
            }

            private Request GetRequestWithBiggestTeam()
            {
                int maximumPlayers = 0;
                Request maxRequest = null;
                foreach (Request request in m_requests)
                {
                    int countBigTeam = GetCountBigTeam(request);
                    if (countBigTeam > maximumPlayers)
                    {
                        maximumPlayers = countBigTeam;
                        maxRequest = request;
                    }
                }
                return maxRequest;
            }

            private int GetCountBiggestTeam()
            {
                return GetCountBigTeam(GetRequestWithBiggestTeam());
            }

            private int GetCountSmallTeam(Request request)
            {
                return Math.Min(request.Team1Count, request.Team2Count);
            }

            private int GetCountBigTeam(Request request)
            {
                return Math.Max(request.Team1Count, request.Team2Count);
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
                ApplyPlayerPositions(m_requests.ToArray());
            }

            public static void ApplyPlayerPositions(Request[] requests)
            {
                int currentTeam1Offset = 0;
                int currentTeam2Offset = 0;
                foreach (Request request in requests)
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
