using System;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class Request
        {
            public int ID { get; private set; }
            public int MaxPlayerCount { get; private set; }
            int[] TeamWishes { get; set; }
            public int ClientPlayerCount { get { return TeamWishes.Length; } }
            public int TeamSize { get { return MaxPlayerCount / 2; } }

            public bool ReverseTeams { get; private set; }

            private int m_team1Count = -1;
            public int Team1Count
            {
                get
                {
                    if (m_team1Count < 0)
                        CalculatateTeamCount();

                    if (ReverseTeams)
                        return TeamWishes.Length - m_team1Count;
                    else
                        return m_team1Count;
                }
            }

            public int Team2Count
            {
                get
                {
                    if (m_team1Count < 0)
                        CalculatateTeamCount();

                    if (ReverseTeams)
                        return m_team1Count;
                    else
                        return TeamWishes.Length - m_team1Count;
                }
            }

            public int Team1Offset { get; set; }
            public int Team2Offset { get; set; }

            public Request(int id, int maxPlayerCount, int[] teamWishes)
            {
                ID = id;
                MaxPlayerCount = maxPlayerCount;
                TeamWishes = teamWishes;

                ReverseTeams = false;

                NormalizeRequest();
            }

            public int[] GetPlayerPlacements()
            {
                int[] playerPlacements = new int[TeamWishes.Length];
                Array.Copy(TeamWishes, playerPlacements, TeamWishes.Length);
                if (ReverseTeams)
                    SwapTeams(playerPlacements);
                return playerPlacements;
            }

            private void NormalizeRequest()
            {
                if (m_team1Count < 0)
                    CalculatateTeamCount();

                if (Team1Count > Team2Count)
                    SwapTeams();
            }

            private void SwapTeams()
            {
                SwapTeams(TeamWishes);

                m_team1Count = TeamWishes.Length - m_team1Count;
            }

            private void SwapTeams(int[] teamWishes)
            {
                for (int i = 0; i < teamWishes.Length; i++)
                {
                    if (teamWishes[i] == 0)
                        teamWishes[i] = 1;
                    else
                        teamWishes[i] = 0;
                }
            }

            public void Reverse()
            {
                ReverseTeams = !ReverseTeams;
            }

            private void CalculatateTeamCount()
            {
                m_team1Count = 0;
                foreach (int wish in TeamWishes)
                {
                    if (wish == 0)
                        m_team1Count++;
                }
            }

            public bool IsEqualRequest(Request other)
            {
                return other.MaxPlayerCount == MaxPlayerCount
                    && other.ClientPlayerCount == ClientPlayerCount
                    && (other.Team1Count == Team1Count || other.Team2Count == Team1Count);
            }

            public Request Copy()
            {
                Request copy = new Request(-1, MaxPlayerCount, TeamWishes);
                copy.m_team1Count = m_team1Count;
                copy.ReverseTeams = ReverseTeams;
                return copy;
            }
        }
    }
}
