using NetworkLibrary.DataStructs;
using System;

namespace PingPongClient.NetworkLayer
{
    public class PacketAdapter
    {
        public static byte[] ServerData_Byte(ServerDataUDP data)
        {
            byte[] byteData = new byte[1] { Convert.ToByte(data.TestValue) };
            return byteData;
        }

        public static ServerDataUDP ServerData_Byte(byte[] data)
        {
            ServerDataUDP serverData = new ServerDataUDP();
            serverData.TestValue = data[0];
            return serverData;
        }
    }
}
