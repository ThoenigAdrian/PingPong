using NetworkLibrary.Utility;
using NetworkLibrary.DataStructs;

namespace PingPongClient.NetworkLayer
{
    class NetworkUDP
    {
        DoubleBuffer<ServerDataUDP> ServerData { get; set; }

        public NetworkUDP()
        {
            ServerData = new DoubleBuffer<ServerDataUDP>();
        }

        public ServerDataUDP Read()
        {
            return ServerData.Read();
        }
    }
}
