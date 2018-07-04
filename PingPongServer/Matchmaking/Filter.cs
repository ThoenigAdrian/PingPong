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
            bool RequestChanges { get; set; }
            bool GroupChanges { get; set; }

            public Filter (int maxPlayerCount)
            {
                MaxPlayerCount = maxPlayerCount;
                GroupChanges = false;
            }

            public void AddRequest(Request request)
            {
                RequestChanges = true;

                foreach (RequestGroup group in RequestGroups)
                {
                    if (group.Push(request))
                        return;
                }

                GroupChanges = true;
                RequestGroup newGroup = new RequestGroup(request);
                RequestGroups.Add(newGroup);
            }

            public void FindMatches()
            {
                if (RequestChanges)
                    SearchValidCombinations();

                if (GroupChanges)
                {
                    if (SearchNewCombinations())    // look for new combinations
                        SearchValidCombinations();  // combine latest findings as well
                }

                GroupChanges = false;
            }

            private void SearchValidCombinations()
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

            private bool SearchNewCombinations()
            {
                PuzzleBox puzzleBox = new PuzzleBox(MaxPlayerCount, RequestGroups.ToArray());
                return AddToValidCombinations(puzzleBox.SearchNewCombinations());
            }

            private bool AddToValidCombinations(ValidCombination[] foundCombinations)
            {
                bool changes = false;
                foreach (ValidCombination combination in foundCombinations)
                {
                    if (!Contains(combination))
                    {
                        m_validCombinations.Add(combination);
                        changes = true;
                    }
                }

                return changes;
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
