using System.Net;
using static NetworkLibrary.NetworkImplementations.NetworkInterface;

namespace PingPongClient.NetworkLayer
{
    public class SessionConnectParameters
    {
        public IPAddress ServerIP { get; private set; }
        public int SessionID { get; private set; }
        public SessionDeathHandler SessionDeathHandler { get; private set; }
        public bool Reconnect { get; set; }

        public SessionConnectParameters(IPAddress ip, SessionDeathHandler sessionDeathHandler)
        {
            ServerIP = ip;
            SessionDeathHandler = sessionDeathHandler;
            Reconnect = false;
        }

        public SessionConnectParameters(IPAddress ip, SessionDeathHandler sessionDeathHandler, int reconnectSessionID)
        {
            ServerIP = ip;
            SessionDeathHandler = sessionDeathHandler;
            SessionID = reconnectSessionID;
            Reconnect = true;
        }
    }
}
