using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations
{
    class NetworkUDP: DataNetwork
    {
        DoubleBuffer<byte[]> ServerData { get; set; }
        Thread ReceiveThread;

        public NetworkUDP(IPEndPoint serverIP)
            : base(serverIP)
        {
            ServerData = new DoubleBuffer<byte[]>();
        }

        protected override Socket InitializeSocket()
        {
            return new Socket(NetworkFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public override byte[] Receive()
        {
            return ServerData.Read();
        }

        public override void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
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
                
                ServerData.Write(data);
            }
        }

    }
}
