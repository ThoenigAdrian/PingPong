using NetworkLibrary.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class UDPConnection: ConnectionInterface
    {
        DoubleBuffer<byte[]> ReceivedData { get; set; }
        Thread ReceiveThread;
        protected IPEndPoint connectionEnd;
        public IPEndPoint connectionLocal;

        public UDPConnection(IPEndPoint target, IPEndPoint local) : base(new Socket(target.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            connectionEnd = target;
            connectionLocal = local;

            ReceivedData = new DoubleBuffer<byte[]>();
        }

        public override byte[] Receive()
        {
            return ReceivedData.Read();
        }

        public override void Send(byte[] data)
        {
            ConnectionSocket.SendTo(data, connectionEnd);
        }

        public override void Initialize()
        {
            ReceiveThread = new Thread(StartReceiveLoop);
            ReceiveThread.Start();
        }

        private void StartReceiveLoop()
        {
            ConnectionSocket.Bind(connectionLocal);

            byte[] data;
            while (!AbortReceive)
            {
                try
                {
                    data = new byte[1024];
                    ConnectionSocket.Receive(data);
                    Log("UDP data received.");
                }
                catch (Exception ex)
                {
                    Log("Receive loop threw exception: " + ex.Message);
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
