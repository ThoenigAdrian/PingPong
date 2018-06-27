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
            public int MinTeamSize { get { return MaxPlayerCount / 2; } }
            public int MaxTeamSize { get { return (MaxPlayerCount % 2 == 0) ? MinTeamSize : MinTeamSize + 1; } }

            private int m_team1Count = -1;

            public int Team1Count
            {
                get
                {
                    if (m_team1Count < 0)
                    {
                        int m_team1Count = 0;
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
        }

        private class Game
        {
            List<ClientData> m_clients = new List<ClientData>();

            public int MaxPlayerCount { get; set; }
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

            public bool Full()
            {
                int playerCount = 0;
                foreach (ClientData client in m_clients)
                {
                    playerCount += client.ClientPlayerCount;
                }

                return playerCount >= MaxPlayerCount;
            }

            private bool FitsIntoGame(ClientData client)
            {
                if (CurrentPlayerCount + client.ClientPlayerCount > MaxPlayerCount)
                    return false;

                //if(client.Tea)

                return true;
            }

            public void ResetClients()
            {
                m_clients.Clear();
            }
        }

        private class MatchingEntry
        {
            public Game Game { get; set; }
            public List<ClientData> SearchingClients { get; set; } = new List<ClientData>();
        }
    }
}
