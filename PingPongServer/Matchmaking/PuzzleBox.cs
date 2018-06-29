using System.Collections.Generic;

namespace PingPongServer.Matchmaking
{
    public partial class Matchmaker
    {
        private class PuzzleBox
        {
            public int MaxPlayerCount { get { return m_game.MaxPlayerCount; } }
            RequestGroup[] m_requestGroups;
            Game m_game;

            List<ValidCombination> FoundCombinations { get; set; } = new List<ValidCombination>();

            public PuzzleBox(int maxPlayerCount, RequestGroup[] requestGroups)
            {
                m_requestGroups = requestGroups;
                m_game = new Game(maxPlayerCount);
            }

            public ValidCombination[] SearchNewCombinations()
            {
                FoundCombinations.Clear();

                for (int maxDepth = 0; maxDepth < MaxPlayerCount; maxDepth++)
                {
                    m_game.ResetRequests();
                    FindCombinationsRecursive(0, maxDepth);
                }

                return FoundCombinations.ToArray();
            }

            private void FindCombinationsRecursive(int depth, int maxDepth)
            {
                for (int i = 0; i < m_requestGroups.Length; i++)
                {
                    m_game.ReplaceRequest(m_requestGroups[i].GroupType, depth);

                    if (depth == maxDepth)
                    {
                        if (m_game.GameReady())
                            CreateValidCombination();
                    }
                    else
                        FindCombinationsRecursive(depth + 1, maxDepth);
                }
            }

            private void CreateValidCombination()
            {
                Request[] newCombination = m_game.GetRequests();
                ValidCombination validCombination = new ValidCombination();

                foreach (Request request in newCombination)
                {
                    foreach (RequestGroup group in m_requestGroups)
                    {
                        if (group.IsEqualRequest(request))
                        {
                            validCombination.AddToCombination(group, request.ReverseTeams);
                            break;
                        }
                    }
                }

                FoundCombinations.Add(validCombination);
            }
        }
    }
}
