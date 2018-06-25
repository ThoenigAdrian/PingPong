using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.NetworkImplementations.Network;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract partial class NetworkInterface
    {
        protected class NetworkOutput
        {
            NetworkConnectionPool ClientConnections { get; set; }

            public NetworkOutput(NetworkConnectionPool connections)
            {
                ClientConnections = connections;
            }

            private bool CanSend()
            {
                return ClientConnections.Count > 0;
            }

            public void SendDataTCP(PackageInterface package, int session)
            {
                if (!CanSend())
                    return;

                NetworkConnection sessionConnection = ClientConnections[session];

                if (sessionConnection != null)
                    sessionConnection.SendTCP(package);
            }

            public void SendDataUDP(PackageInterface package, int session)
            {
                if (!CanSend())
                    return;

                NetworkConnection sessionConnection = ClientConnections[session];

                if (sessionConnection != null)
                    sessionConnection.SendUDP(package);
            }

            public void BroadCastTCP(PackageInterface package)
            {
                if (!CanSend())
                    return;

                foreach (NetworkConnection clientCon in ClientConnections.Values)
                {
                    clientCon.SendTCP(package);
                }
            }

            public void BroadCastUDP(PackageInterface package)
            {
                if (!CanSend())
                    return;

                foreach (NetworkConnection clientCon in ClientConnections.Values)
                {
                    clientCon.SendUDP(package);
                }
            }

            public void BroadCastKeepAlive()
            {
                foreach (NetworkConnection clientCon in ClientConnections.Values)
                {
                    clientCon.SendKeepAlive();
                }
            }

            public void BroadCastHolePunching()
            {
                foreach (NetworkConnection clientCon in ClientConnections.Values)
                {
                    clientCon.SendHolePunching();
                }
            }
        }
    }
}