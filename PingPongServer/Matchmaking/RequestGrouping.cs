using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class RequestGroup
        {
            public List<List<Request>> m_requests { get; private set; } = new List<List<Request>>();

            public void AddRequest(Request request)
            {
                foreach (List<Request> group in m_requests)
                {
                    if (group.Count > 0 && group[0].IsEqualRequest(request))
                    {
                        group.Add(request);
                        return;
                    }
                }

                List<Request> newGroup = new List<Request>();
                m_requests.Add(newGroup);
                newGroup.Add(request);
            }

            public void RemoveRequest(Request request)
            {
                foreach (List<Request> group in m_requests)
                {
                    if (group.Contains(request))
                    {
                        group.Remove(request);
                        if (group.Count <= 0)
                            m_requests.Remove(group);
                        return;
                    }
                }
            }
        }
    }
}
