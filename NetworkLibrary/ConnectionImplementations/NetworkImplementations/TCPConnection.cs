using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public abstract class TCPConnection : DataNetwork
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
