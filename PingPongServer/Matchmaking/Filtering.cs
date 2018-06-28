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
                puzzleBox.SearchNewCombinations();
                m_validCombinations.AddRange(puzzleBox.FoundCombinations);
            }
        }

        private class RequestGroup
        {
            public Request GroupType { get; private set; }

            List<Request> m_requests = new List<Request>();

            public int CombinationMaxDepth { get; set; } = -1;

            public RequestGroup(Request request)
            {
                GroupType = request.Copy();
                m_requests.Add(request);
            }

            public bool IsEqualRequest(Request request)
            {
                return GroupType.IsEqualRequest(request);
            }

            public bool Push(Request request)
            {
                if (IsEqualRequest(request))
                {
                    m_requests.Add(request);
                    return true;
                }

                return false;
            }

            public bool PushFront(Request request)
            {
                if (IsEqualRequest(request))
                {
                    m_requests.Insert(0, request);
                    return true;
                }

                return false;
            }

            public Request Pop()
            {
                Request poppedRequest = null;

                if (m_requests.Count > 0)
                {
                    poppedRequest = m_requests[0];
                    m_requests.RemoveAt(0);
                }

                return poppedRequest;
            }
        }
    }
}
