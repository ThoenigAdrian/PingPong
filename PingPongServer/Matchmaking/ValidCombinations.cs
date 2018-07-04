using System.Collections.Generic;

namespace PingPongServer.Matchmaking
{
    public partial class Matchmaker
    {
        private class ValidCombination
        {
            private class Entry
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

            public bool IsEqual(ValidCombination other)
            {
                if (other.Combination.Count != Combination.Count)
                    return false;

                List<Entry> entriesCopy = new List<Entry>(Combination);

                foreach (Entry otherEntry in other.Combination)
                {
                    bool found = false;

                    foreach (Entry entry in entriesCopy)
                    {
                        if (entry.Group == otherEntry.Group && entry.Reversed == otherEntry.Reversed)
                        {
                            found = true;
                            entriesCopy.Remove(entry);
                            break;
                        }
                    }

                    if (!found)
                        return false;
                }

                return true;
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

                    request.ReverseTeams = entry.Reversed;

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

            public override string ToString()
            {
                string str = "";

                foreach (Entry entry in Combination)
                {
                    if (str.Length > 0)
                        str += " | ";

                    str += entry.Group.GroupType.Team1Count + "-" + entry.Group.GroupType.Team2Count;
                }

                return str;
            }
        }
    }
}
