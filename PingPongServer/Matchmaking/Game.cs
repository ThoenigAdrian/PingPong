using System.Collections.Generic;
using System;

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
                    if (CurrentlyFitting())
                        return true;

                    for (int i = 0; i < m_requests.Count - 1 && step < possibilities - 1; i++)
                    {
                        if (step % (1 << i) == (1 << i) - 1)
                            m_requests[m_requests.Count - 1 - i].SwapTeams();
                    }
                }

                return false;
            }

            private bool OneTeamSizeExceeds(List<Request> allRequests, int maxPlayerSize)
            {
                int maxTeamSize = maxPlayerSize / 2;
                Request maximumRequest = getRequestWithMaximumPlayersInTeam(allRequests);
                allRequests.Remove(maximumRequest);
                List<Request> allRequestsWithoutMaximum = cloneRequestList(allRequests);
                allRequestsWithoutMaximum.Remove(maximumRequest);
                int minimumPossibleTeamSize = getMaximumPlayersOfRequestList(allRequests) + getSumOfMinimumPlayersOfRequestList(allRequestsWithoutMaximum);
                return minimumPossibleTeamSize > maxTeamSize;
            }

            private int getSumOfMinimumPlayersOfRequestList(List<Request> requestList)
            {
                int sumOfMinimumPlayersOfRequestList = 0;
                foreach (Request request in requestList)
                {
                    sumOfMinimumPlayersOfRequestList += getMinimumPlayersOfRequest(request);
                }
                return sumOfMinimumPlayersOfRequestList;
            }

            private Request getRequestWithMaximumPlayersInTeam(List<Request> requestList)
            {
                int maximumPlayersOfCurrentRequest = 0;
                int maximumPlayers = 0;
                Request maxRequest = null;
                foreach (Request request in requestList)
                {
                    maximumPlayersOfCurrentRequest = getMaximumPlayersOfRequest(request);
                    if (maximumPlayersOfCurrentRequest > maximumPlayers)
                    {
                        maximumPlayers = maximumPlayersOfCurrentRequest;
                        maxRequest = request;
                    }
                }
                return maxRequest;
            }

            private List<Request> cloneRequestList(List<Request> requestList)
            {
                List<Request> clonedList = new List<Request>();
                foreach (Request request in requestList)
                {
                    clonedList.Add(request);
                }
                return clonedList;
            }


            private int getMinimumPlayersOfRequestList(List<Request> requestList)
            {
                int minimumPlayersOfCurrentRequest = 0;
                int minimumPlayers = 10000;
                foreach (Request request in requestList)
                {
                    minimumPlayersOfCurrentRequest = getMinimumPlayersOfRequest(request);
                    if (minimumPlayersOfCurrentRequest < minimumPlayers)
                    {
                        minimumPlayers = minimumPlayersOfCurrentRequest;
                    }
                }
                return minimumPlayers;
            }

            private int getMaximumPlayersOfRequestList(List<Request> requestList)
            {
                int maximumPlayersOfCurrentRequest = 0;
                int maximumPlayers = 0;
                foreach (Request request in requestList)
                {
                    maximumPlayersOfCurrentRequest = getMaximumPlayersOfRequest(request);
                    if (maximumPlayersOfCurrentRequest > maximumPlayers)
                    {
                        maximumPlayers = maximumPlayersOfCurrentRequest;
                    }
                }
                return maximumPlayers;
            }

            private int getMinimumPlayersOfRequest(Request request)
            {
                return Math.Min(request.Team1Count, request.Team2Count);
            }

            private int getMaximumPlayersOfRequest(Request request)
            {
                return Math.Min(request.Team1Count, request.Team2Count);
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
