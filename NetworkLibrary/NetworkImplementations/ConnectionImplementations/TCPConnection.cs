using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public abstract class TCPConnection : ConnectionInterface
    {
        public TCPConnection(Socket socket) 
            : base(socket)
        {
        }

        public override void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        public override byte[] Receive()
        {
            byte[] data = null;
            ConnectionSocket.Receive(data);
            return data;
        }

        protected override void WaitForDisconnect()
        {
            return;
        }
    }
}
