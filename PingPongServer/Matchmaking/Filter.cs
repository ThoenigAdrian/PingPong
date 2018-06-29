using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class Filter
        {
            public delegate void ValidCombinationHandler(object sender, Request[] combination);
            public event ValidCombinationHandler FoundValidCombination;

            public List<RequestGroup> m_requestGroups { get; private set; } = new List<RequestGroup>();
            List<ValidCombination> m_validCombinations = new List<ValidCombination>();

            public int MaxPlayerCount { get; private set; }

            public Filter (int maxPlayerCount)
            {
                MaxPlayerCount = maxPlayerCount;
            }

            public void AddRequest(Request request)
            {
                foreach (RequestGroup group in m_requestGroups)
                {
                    if (group.Push(request))
                        return;
                }

                RequestGroup newGroup = new RequestGroup(request);
                m_requestGroups.Add(newGroup);
            }

            public void SearchValidCombinations()
            {
                foreach(ValidCombination combination in m_validCombinations)
                {
                    Request[] realCombination;
                    while ((realCombination = combination.GetRequests()) != null)
                    {
                        FoundValidCombination?.Invoke(this, realCombination);
                    }
                }
            }

            public void SearchNewCombinations()
            {
                PuzzleBox puzzleBox = new PuzzleBox(MaxPlayerCount, m_requestGroups.ToArray());
                AddToValidCombinations(puzzleBox.SearchNewCombinations());
            }

            private void AddToValidCombinations(ValidCombination[] foundCombinations)
            {
                foreach (ValidCombination combination in foundCombinations)
                {
                    if (!Contains(combination))
                        m_validCombinations.Add(combination);
                }
            }

            private bool Contains(ValidCombination combination)
            {
                foreach(ValidCombination existingCombination in m_validCombinations)
                {
                    if (existingCombination.IsEqual(combination))
                        return true;
                }

                return false;
            }
        }
    }
}
