using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.NetworkImplementations.Network;
using System;
using System.Net;
using XSLibrary.Network.Connections;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract partial class NetworkInterface
    {
        protected class NetworkErrorHandling
        {
            NetworkInterface ParentNetwork { get; set; }
            NetworkConnectionPool ClientConnections { get; set; }

            public NetworkErrorHandling(NetworkConnectionPool connections, NetworkInterface parent)
            {
                ParentNetwork = parent;
                ClientConnections = connections;
            }

            public void ConnectionDiedHandler(object sender, EventArgs e)
            {
                NetworkConnection connection = sender as NetworkConnection;

                connection.ConnectionDiedEvent -= ConnectionDiedHandler;
                RemoveConnection(connection);

                RaiseDeadSessionEvent(connection);
            }

            public void HandleUDPReceiveError(object sender, IPEndPoint receiveEndPoint)
            {
                NetworkConnection deadConnection = null;

                foreach (NetworkConnection clientCon in ClientConnections.Values)
                {
                    if (clientCon.IsConnectedTo(receiveEndPoint.Port))
                    {
                        deadConnection = clientCon;
                        break;
                    }
                }

                if (deadConnection != null)
                    deadConnection.CloseConnection();
            }

            public void HandleUDPSendError(object sender, IPEndPoint remote)
            {
                throw new Exception("y u du dis");
            }

            private void RemoveConnection(NetworkConnection connection)
            {
                if (!ClientConnections.TryRemove(connection.ClientSession.SessionID, out NetworkConnection dummy))
                    throw new ConnectionException("Could not remove network connection!");
            }

            private void RaiseDeadSessionEvent(NetworkConnection connection)
            {
                ParentNetwork.SessionDied?.Invoke(ParentNetwork, connection.ClientSession.SessionID);
            }
        }
    }
}
