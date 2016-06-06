using NetworkLibrary.Utility;
using NetworkLibrary.DataStructs;
using System.Net;
using System.Net.Sockets;

namespace PingPongClient.NetworkLayer
{
    class NetworkUDP
    {
        DoubleBuffer<ServerDataUDP> ServerData { get; set; }
        Socket ConnectionSocket { get; set; }
        IPAddress ServerIP { get; set; }

        public NetworkUDP(IPAddress serverIP)
        {
            ServerData = new DoubleBuffer<ServerDataUDP>();
            ServerIP = serverIP;
            ConnectionSocket = new Socket(serverIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Connect()
        {
            ConnectionSocket.Connect(new IPEndPoint(ServerIP, )
        }

        public ServerDataUDP Read()
        {
            return ServerData.Read();
        }
    }
}
