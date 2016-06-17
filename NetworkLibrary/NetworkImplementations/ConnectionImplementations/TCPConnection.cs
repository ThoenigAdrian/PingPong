using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Utility;
using System;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class TCPConnection : ConnectionInterface
    {
        public delegate void DataReceivedHandler(TCPConnection sender, byte[] data);
        public event DataReceivedHandler DataReceivedEvent;

        public IPEndPoint GetEndPoint { get { return ConnectionSocket.RemoteEndPoint as IPEndPoint; } }

        public TCPConnection(Socket socket, LogWriter logger = null) 
            : base(socket, logger)
        {
        }

        public virtual void Send(byte[] data)
        {
            SocketLock.WaitOne();
            try
            {
                if (!Disconnecting)
                    ConnectionSocket.Send(data);
            }
            finally { SocketLock.Release(); }
        }

        public void SendKeepAlive()
        {
            SocketLock.WaitOne();
            try
            {
                if (!Disconnecting)
                {
                    ConnectionSocket.Send(new byte[] { 123, 125 });
                    Log("Sent keepalive.");
                }
            }
            finally { SocketLock.Release(); }
        }

        protected override void PreReceiveSettings()
        {
            return;
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[NetworkConstants.MAX_PACKAGE_SIZE];

            int size = ConnectionSocket.Receive(data);

            if (size <= 0)
            {
                ReceiveThread = null;
                ReceiveErrorHandling(ConnectionEndpoint);
                return;
            }

            RaiseReceivedEvent(TrimData(data, size));
        }

        void RaiseReceivedEvent(byte[] data)
        {
            if (DataReceivedEvent != null)
                DataReceivedEvent.Invoke(this, data);
        }
    }
}
