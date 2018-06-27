using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class RequestGroup
        {
            public List<List<ClientData>> m_clients { get; private set; } = new List<List<ClientData>>();

            public void AddRequest(ClientData request)
            {
                foreach (List<ClientData> group in m_clients)
                {
                    if (group.Count > 0 && group[0].IsEqualRequest(request))
                    {
                        group.Add(request);
                        return;
                    }
                }

                List<ClientData> newGroup = new List<ClientData>();
                m_clients.Add(newGroup);
                newGroup.Add(request);
            }

            public void RemoveRequest(ClientData request)
            {
                foreach (List<ClientData> group in m_clients)
                {
                    if (group.Contains(request))
                    {
                        group.Remove(request);
                        if (group.Count <= 0)
                            m_clients.Remove(group);
                        return;
                    }
                }
            }
        }
    }
}
