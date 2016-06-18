using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Concurrent;
using System.Linq;

namespace NetworkLibrary.NetworkImplementations.Network
{
    public class NetworkConnectionPool : ConcurrentDictionary<int, NetworkConnection>
    {
        public int[] SessionIDs { get { return Keys.ToArray(); } }

        new public NetworkConnection this[int index] { get { return GetConnection(index); } }

        private NetworkConnection GetConnection(int session)
        {
            NetworkConnection connection = null;
            if (TryGetValue(session, out connection))
                return connection;
            else
                return null;
        }

        public int[] GetSessionIDs { get { return Keys.ToArray(); } }
    }
}
