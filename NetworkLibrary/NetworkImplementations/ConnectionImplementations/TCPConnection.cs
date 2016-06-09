using NetworkLibrary.Utility;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class TCPConnection : ConnectionInterface
    {
        public TCPConnection(Socket socket) 
            : base(socket)
        {
        }

        protected override DataContainer<byte[]> InitializeDataContainer()
        {
            return new SafeStack<byte[]>();
        }
    }
}
