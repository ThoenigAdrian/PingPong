using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class ClientData
        {
            public NetworkConnection ClientConnection { get; set; }
            public int MaxPlayerCount { get; set; }
            public int[] TeamWishes { get; set; }
            public int ClientPlayerCount { get { return TeamWishes.Length; } }
            public int TeamSize { get { return MaxPlayerCount / 2; } }

            private int m_team1Count = -1;

            public int Team1Count
            {
                get
                {
                    if (m_team1Count < 0)
                    {
                        m_team1Count = 0;
                        foreach (int wish in TeamWishes)
                        {
                            if (wish == 0)
                                m_team1Count++;
                        }
                    }
                    return m_team1Count;
                }
            }

            public int Team2Count { get { return TeamWishes.Length - Team1Count; } }

            public void SwapTeams()
            {
                m_team1Count = Team2Count;

                for(int i = 0; i < TeamWishes.Length; i++)
                {
                    if (TeamWishes[i] == 0)
                        TeamWishes[i] = 1;
                    else
                        TeamWishes[i] = 0;
                }
            }

            public bool IsEqualRequest(ClientData other)
            {
                return other.MaxPlayerCount == MaxPlayerCount
                    && other.ClientPlayerCount == ClientPlayerCount
                    && (other.Team1Count == Team1Count || other.Team2Count == Team1Count);
            }
        }

        private class Game
        {
            List<ClientData> m_clients = new List<ClientData>();

            public int MaxPlayerCount { get; set; }
            public int TeamSize { get { return MaxPlayerCount / 2; } }
            public int CurrentPlayerCount
            {
                get
                {
                    int currentPlayerCount = 0;
                    foreach (ClientData client in m_clients)
                        currentPlayerCount += client.ClientPlayerCount;

                    return currentPlayerCount;
                }
            }

            public void AddClient(ClientData client, int index = -1)
            {
                if (index < 0)
                {
                    m_clients.Add(client);
                }
                else
                {
                    m_clients.RemoveAt(index);
                    m_clients.Insert(index, client);
                }
            }

            public bool Full()
            {
                return CurrentPlayerCount == MaxPlayerCount;
            }

            public bool FitsIntoGame()
            {
                if (CurrentPlayerCount > MaxPlayerCount)
                    return false;

                if (m_clients.Count <= 0)
                    return true;

                int possibilities = 1 << (m_clients.Count - 1);

                for (int step = 0; step < possibilities; step++)
                {
                    for(int i = 0; i < m_clients.Count - 1; i++)
                    {
                        int clientIndex = m_clients.Count - 1 - i;

                        if (step % (2 << i) == (1 << i))
                            m_clients[clientIndex].SwapTeams();
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

                foreach(ClientData client in m_clients)
                {
                    team1 += client.Team1Count;
                    team2 += client.Team2Count;
                }

                return team1 <= TeamSize && team2 <= TeamSize;
            }

            public void ResetClients()
            {
                m_clients.Clear();
            }
        }

        private class MatchingEntry
        {
            public Game Game { get; set; }
            public RequestGroup GroupedClients { get; set; } = new RequestGroup();
        }
    }
}
