using NetworkLibrary.NetworkImplementations.ConnectionImplementations;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class Request
        {
            public NetworkConnection ClientConnection { get; private set; }
            public int MaxPlayerCount { get; private set; }
            int[] TeamWishes { get; set; }
            public int ClientPlayerCount { get { return TeamWishes.Length; } }
            public int TeamSize { get { return MaxPlayerCount / 2; } }

            private bool ReverseTeams { get; set; }

            private int m_team1Count = -1;
            public int Team1Count
            {
                get
                {
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
                    CalculatateTeamCount();

                    if (ReverseTeams)
                        return m_team1Count;
                    else
                        return TeamWishes.Length - m_team1Count;
                }
            }

            public int Team1Offset { get; set; }
            public int Team2Offset { get; set; }

            public Request(/*NetworkConnection connection,*/ int maxPlayerCount, int[] teamWishes)
            {
                //ClientConnection = connection;
                MaxPlayerCount = maxPlayerCount;
                TeamWishes = teamWishes;
            }

            public int[] GetPlayerPlacements()
            {
                int[] playerPlacements = new int[TeamWishes.Length];
                for (int i = 0; i < TeamWishes.Length; i++)
                {
                    if ((TeamWishes[i] == 0) != ReverseTeams)
                        playerPlacements[i] = 1;
                    else
                        playerPlacements[i] = 0;
                }
                return playerPlacements;
            }

            public void SwapTeams()
            {
                ReverseTeams = !ReverseTeams;
            }

            private void CalculatateTeamCount()
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
            }

            public bool IsEqualRequest(Request other)
            {
                return other.MaxPlayerCount == MaxPlayerCount
                    && other.ClientPlayerCount == ClientPlayerCount
                    && (other.Team1Count == Team1Count || other.Team2Count == Team1Count);
            }
        }


        private class MatchingEntry
        {
            public Game Game { get; set; }
            public RequestGroup GroupedRequests { get; set; } = new RequestGroup();
        }
    }
}
