using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Utility;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class UDPConnection: ConnectionInterface
    {
        public delegate void DataReceivedHandler(UDPConnection sender, byte[] data, IPEndPoint endPoint);
        public event DataReceivedHandler DataReceivedEvent;

        public UDPConnection(IPEndPoint local, LogWriter logger = null) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp), local, logger)
        {
        }

        public virtual void Send(byte[] data, IPEndPoint remoteEndPoint)
        {
            SocketLock.WaitOne();

            try
            {
                if (!Disconnecting)
                    ConnectionSocket.SendTo(data, remoteEndPoint);
            }
            finally
            {
                SocketLock.Release();
            }
        }

        protected override void PreReceiveSettings()
        {
            ConnectionSocket.Bind(ConnectionEndpoint);
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[NetworkConstants.MAX_PACKAGE_SIZE];
            EndPoint source = new IPEndPoint(ConnectionEndpoint.Address, ConnectionEndpoint.Port);

            int size = ConnectionSocket.ReceiveFrom(data, ref source);

            RaiseReceivedEvent(TrimData(data, size), source as IPEndPoint);    
        }

        private void RaiseReceivedEvent(byte[] data, IPEndPoint source)
        {
            if (DataReceivedEvent != null)
                DataReceivedEvent.Invoke(this, data, source);
        }
    }
}
