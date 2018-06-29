using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
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
