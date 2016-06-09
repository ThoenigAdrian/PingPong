using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System.Net.Sockets;
using NetworkLibrary.PackageAdapters;

namespace PingPongServer
{
    class ServerNetwork : NetworkInterface
    {
        public ServerNetwork(Socket acceptedSocket)
            : base(new TCPServerConnection(acceptedSocket),
                 new UDPConnection(acceptedSocket.RemoteEndPoint),
                 null)
        {

        }

        public ServerNetwork(Socket acceptedSocket, LogWriter logger)
            : base(new TCPServerConnection(acceptedSocket),
                 new UDPConnection(acceptedSocket.RemoteEndPoint),
                 logger)
        {

        }

        protected override PackageAdapter InitializeAdapter()
        {
            return new PackageAdapter();
        }
    }
}
