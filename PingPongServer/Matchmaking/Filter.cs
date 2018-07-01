using System.Collections.Generic;

namespace PingPongServer.Matchmaking
{
    public partial class Matchmaker
    {
        private class Filter
        {
            public delegate void ValidCombinationHandler(object sender, Request[] combination);
            public event ValidCombinationHandler FoundValidCombination;

            public List<RequestGroup> RequestGroups { get; private set; } = new List<RequestGroup>();
            List<ValidCombination> m_validCombinations = new List<ValidCombination>();

            public int MaxPlayerCount { get; private set; }
            public bool Changes { get; private set; }

            public Filter (int maxPlayerCount)
            {
                MaxPlayerCount = maxPlayerCount;
                Changes = false;
            }

            public void AddRequest(Request request)
            {
                foreach (RequestGroup group in RequestGroups)
                {
                    if (group.Push(request))
                        return;
                }

                RequestGroup newGroup = new RequestGroup(request);
                RequestGroups.Add(newGroup);
                Changes = true;
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
                PuzzleBox puzzleBox = new PuzzleBox(MaxPlayerCount, RequestGroups.ToArray());
                AddToValidCombinations(puzzleBox.SearchNewCombinations());
                Changes = false;
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
