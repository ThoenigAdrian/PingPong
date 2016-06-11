using NetworkLibrary.Utility;
using System;
using System.Net;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class NetworkConnection : IDisposable
    {
        TCPConnection TcpConnection { get; set; }
        UDPConnection UdpConnection { get; set; }

        DataContainer<byte[]> TcpData { get; set; }
        DataContainer<byte[]> UdpData { get; set; }

        IPEndPoint RemoteEndPoint { get { return TcpConnection.GetEndPoint; } }

        public bool Connected { get { return TcpConnection.Connected; } }

        public NetworkConnection(TCPConnection tcpConnection)
        {
            TcpData = new SafeStack<byte[]>();

            TcpConnection = tcpConnection;

            TcpConnection.DataReceivedEvent += ReceiveTCP;
        }

        public void SetUDPConnection(UDPConnection udpConnection)
        {
            UdpData = new DoubleBuffer<byte[]>();

            UdpConnection = udpConnection;
            UdpConnection.DataReceivedEvent += ReceiveUDP;
        }

        public void CloseConnection()
        {
            TcpConnection.DataReceivedEvent -= ReceiveTCP;
            UdpConnection.DataReceivedEvent -= ReceiveUDP;

            TcpConnection.Disconnect();
        }

        public void SendTCP(byte[] data)
        {
            TcpConnection.Send(data);
        }

        public void SendUDP(byte[] data)
        {
            UdpConnection.Send(data, RemoteEndPoint);
        }

        public byte[] ReadTCP()
        {
            return TcpData.Read();
        }

        public byte[] ReadUDP()
        {
            return UdpData.Read();
        }

        private void ReceiveUDP(byte[] data, IPEndPoint source)
        {
            if (RemoteEndPoint.Port == source.Port)
                UdpData.Write(data);
        }

        private void ReceiveTCP(byte[] data)
        {
            TcpData.Write(data);
        }

        void IDisposable.Dispose()
        {
            CloseConnection();
        }
    }
}
