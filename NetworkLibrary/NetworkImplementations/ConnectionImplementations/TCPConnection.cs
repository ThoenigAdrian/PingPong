using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Utility;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class TCPConnection : ConnectionInterface
    {
        public delegate void DataReceivedHandler(byte[] data);

        public event DataReceivedHandler DataReceivedEvent;

        public IPEndPoint GetEndPoint { get { return ConnectionSocket.RemoteEndPoint as IPEndPoint; } }

        public TCPConnection(Socket socket, LogWriter logger = null) 
            : base(socket, logger)
        {
        }

        public virtual void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[NetworkConstants.MAX_PACKAGE_SIZE];

            int size = ConnectionSocket.Receive(data);

            RaiseReceivedEvent(TrimData(data, size));
        }

        void RaiseReceivedEvent(byte[] data)
        {
            if (DataReceivedEvent != null)
                DataReceivedEvent.Invoke(data);
        }
    }
}
