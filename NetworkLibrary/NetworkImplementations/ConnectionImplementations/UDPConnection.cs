using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public class UDPConnection: ConnectionInterface
    {
        DoubleBuffer<byte[]> ReceivedData { get; set; }
        Thread ReceiveThread;
        IPEndPoint ConnectionEndPoint { get; set; }

        public UDPConnection(IPEndPoint target) : base(new Socket(target.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            ConnectionEndPoint = target;

            ReceivedData = new DoubleBuffer<byte[]>();
        }

        public override byte[] Receive()
        {
            return ReceivedData.Read();
        }

        public override void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        public override void Initialize()
        {
            ConnectionSocket.Connect(ConnectionEndPoint);

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
                
                ReceivedData.Write(data);
            }
        }

        protected override void WaitForDisconnect()
        {
            ReceiveThread.Join();
        }

    }
}
