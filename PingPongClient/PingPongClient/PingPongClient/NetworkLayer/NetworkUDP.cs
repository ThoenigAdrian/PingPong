using NetworkLibrary.Utility;
using NetworkLibrary.DataStructs;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PingPongClient.NetworkLayer
{
    class NetworkUDP: DataNetwork<ServerDataUDP>
    {
        DoubleBuffer<ServerDataUDP> ServerData { get; set; }
        Thread ReceiveThread;

        public NetworkUDP(IPAddress serverIP) : base(serverIP)
        {
            ServerData = new DoubleBuffer<ServerDataUDP>();
            ConnectionSocket = new Socket(serverIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public override ServerDataUDP Receive()
        {
            return ServerData.Read();
        }

        public override void Send(ServerDataUDP data)
        {
            ConnectionSocket.Send(PacketAdapter.ServerData_Byte(data));
        }

        protected override void PostConnectActions()
        {
            ReceiveThread = new Thread(StartReceiveLoop);
            ReceiveThread.Start();
        }

        private void StartReceiveLoop()
        {
            byte[] data = new byte[0];
            while (!AbortReceive)
            {
                try
                {
                    ConnectionSocket.Receive(data);
                }
                catch
                {
                    Logger.Log("Receive loop threw exception");
                    return;
                }
                
                ServerData.Write(PacketAdapter.ServerData_Byte(data));
            }
        }

    }
}
