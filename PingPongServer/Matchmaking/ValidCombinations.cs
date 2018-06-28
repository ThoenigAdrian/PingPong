using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        private class ValidCombination
        {
            public class Entry
            {
                public RequestGroup Group { get; private set; }
                public bool Reversed { get; private set; }

                public Entry(RequestGroup group, bool reversed)
                {
                    Group = group;
                    Reversed = reversed;
                }
            }

            List<Entry> Combination { get; set; } = new List<Entry>();

            public void AddToCombination(RequestGroup requestGroup, bool reversed)
            {
                Combination.Add(new Entry(requestGroup, reversed));
            }

            public Request[] GetRequests()
            {
                Request[] requests = new Request[Combination.Count];

                int index = 0;
                foreach (Entry entry in Combination)
                {
                    RequestGroup group = entry.Group;
                    Request request = group.Pop();

                    if (request == null)
                    {
                        CleanUpFailure(requests);
                        return null;
                    }

                    if (entry.Reversed != request.ReverseTeams)
                        request.Reverse();

                    requests[index] = request;
                    index++;
                }

                Game.ApplyPlayerPositions(requests);

                return requests;
            }

            private void CleanUpFailure(Request[] requests)
            {
                foreach (Request request in requests)
                {
                    foreach (Entry entry in Combination)
                    {
                        if (request != null && entry.Group.PushFront(request))
                            break;
                    }
                }
            }
        }
    }
}
