using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class TCPConnection : ConnectionInterface
    {
        public IPEndPoint GetEndPoint { get { return ConnectionSocket.RemoteEndPoint as IPEndPoint; } }

        public TCPConnection(Socket socket) 
            : base(socket)
        {
        }

        public virtual void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        protected override DataContainer<byte[]> InitializeDataContainer()
        {
            return new SafeStack<byte[]>();
        }
    }
}
