using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.NetworkImplementations.Network;
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

            public void ConnectionDiedHandler(NetworkConnection sender)
            {
                sender.ConnectionDiedEvent -= ConnectionDiedHandler;

                RemoveConnection(sender);
                RaiseDeadSessionEvent(sender);
            }

            public void HandleUDPReceiveError(object sender, IPEndPoint receiveEndPoint)
            {
                NetworkConnection deadConnection = null;

                foreach (NetworkConnection clientCon in ClientConnections.Values)
                {
                    if (clientCon.ISConnectedTo(receiveEndPoint.Port))
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
                throw new System.Exception("y u du dis");
            }

            private void RemoveConnection(NetworkConnection connection)
            {
                NetworkConnection dummy;
                if (!ClientConnections.TryRemove(connection.ClientSession.SessionID, out dummy))
                    throw new ConnectionException("Could not remove network connection!");
            }

            private void RaiseDeadSessionEvent(NetworkConnection connection)
            {
                SessionDeathHandler SessionDiedCopy = ParentNetwork.SessionDied;
                if(SessionDiedCopy != null)
                    SessionDiedCopy.Invoke(ParentNetwork, connection.ClientSession.SessionID);
            }
        }
    }
}
