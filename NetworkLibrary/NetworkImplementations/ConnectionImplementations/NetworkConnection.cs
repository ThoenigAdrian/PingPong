using NetworkLibrary.DataPackages;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using System;
using System.Net;
using XSLibrary.Network.Connections;
using XSLibrary.ThreadSafety;
using XSLibrary.ThreadSafety.Containers;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class NetworkConnection : IDisposable
    {
        public delegate void ConnectionDiedHandler(NetworkConnection sender);
        public event ConnectionDiedHandler ConnectionDiedEvent
        {
            add
            {
                OnDisconnect += value;

                m_raiseDisconnect.Fire(() =>
                {
                    return !Connected && RaiseConnectionDiedEvent();
                });
            }
            remove { OnDisconnect -= value; }
        }

        private event ConnectionDiedHandler OnDisconnect;

        public Session ClientSession { get; set; }

        TCPPacketConnection TcpConnection { get; set; }
        UDPConnection UdpConnection { get; set; }

        DataContainer<PackageInterface> TcpPackages { get; set; }
        DataContainer<DataWrapper<byte[]>> UdpData { get; set; }

        PackageAdapter Adapter { get; set; }

        public IPEndPoint RemoteEndPoint { get; set; }

        public bool Connected { get { return TcpConnection.Connected; } }

        SafeList<ResponseRequest> m_openResponses;
        OneShotCondition m_raiseDisconnect = new OneShotCondition();

        public NetworkConnection(TCPPacketConnection tcpConnection) : this(tcpConnection, null) { }
        public NetworkConnection(TCPPacketConnection tcpConnection, ResponseRequest responseRequest) : this(tcpConnection, responseRequest, new JSONAdapter()) { }
        public NetworkConnection(TCPPacketConnection tcpConnection, ResponseRequest responseRequest, PackageAdapter adapter)
        {
            Adapter = adapter;

            TcpPackages = new SafeStack<PackageInterface>();
            m_openResponses = new SafeList<ResponseRequest>();

            if (responseRequest != null)
                m_openResponses.Add(responseRequest);

            ClientSession = null;

            TcpConnection = tcpConnection;
            RemoteEndPoint = tcpConnection.Remote;

            TcpConnection.DataReceivedEvent += ReceiveTCP;
            TcpConnection.OnDisconnect += HandleDisconnect;
            TcpConnection.InitializeReceiving();
        }

        public void IssueResponse(ResponseRequest responseHandler)
        {
            m_openResponses.Add(responseHandler);
        }

        public bool IsConnectedTo(int port)
        {
            return RemoteEndPoint.Port == port;
        }

        public void SetUDPConnection(UDPConnection udpConnection)
        {
            UdpData = new SingleBuffer<DataWrapper<byte[]>>();

            UdpConnection = udpConnection;
            UdpConnection.DataReceivedEvent += ReceiveUDP;
        }

        public void CloseConnection()
        {
            TcpConnection.OnDisconnect -= HandleDisconnect;
            TcpConnection.DataReceivedEvent -= ReceiveTCP;
            if (UdpConnection != null)
                UdpConnection.DataReceivedEvent -= ReceiveUDP;

            if (m_raiseDisconnect.Fire(() => {  return OnDisconnect != null; }))
                RaiseConnectionDiedEvent();
        }

        private bool RaiseConnectionDiedEvent()
        {
            ConnectionDiedHandler handler = OnDisconnect;
            if (handler != null)
            {
                handler(this);
                return true;
            }
            return false;
        }

        public void SendTCP(PackageInterface package)
        {
            TcpConnection.Send(Adapter.CreateNetworkDataFromPackage(package));
        }

        public void SendUDP(PackageInterface package)
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            UdpConnection.Send(Adapter.CreateNetworkDataFromPackage(package), RemoteEndPoint);
        }

        public void SendKeepAlive()
        {
            TcpConnection.SendKeepAlive();
        }

        public void SendHolePunching()
        {
            UdpConnection.HolePunching(RemoteEndPoint);
        }

        public PackageInterface ReadTCP()
        {
            return TcpPackages.Read();
        }

        public PackageInterface ReadUDP()
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            // not threadsafe - one thread can set "Read" after the other thread checks flag here --- not an issue here though as it will just evaluate UDP twice which is... fine
            DataWrapper<byte[]> dataWrapper = dataWrapper = UdpData.Read();
            if (dataWrapper != null && !dataWrapper.Read)
                return Adapter.CreatePackageFromNetworkData(dataWrapper.Data);

            return null;
        }

        private void ReceiveUDP(object sender, byte[] data, IPEndPoint source)
        {
            if (RemoteEndPoint.Port == source.Port)
                UdpData.Write(new DataWrapper<byte[]>(data));
        }

        private void ReceiveTCP(object sender, byte[] data, IPEndPoint source)
        {
            PackageInterface package = Adapter.CreatePackageFromNetworkData(data);
            if (package == null)
                return;

            if (!CheckForResponse(package))
                TcpPackages.Write(package);
        }

        private bool CheckForResponse(PackageInterface package)
        {
            foreach (ResponseRequest responseRequest in m_openResponses.Entries)
            {
                if (responseRequest.InspectPackage(package)
                    || responseRequest.State == ResponseRequest.ResponseState.Timeout
                    || responseRequest.State == ResponseRequest.ResponseState.Canceled)
                {
                    m_openResponses.Remove(responseRequest);
                    return true;
                }
            }

            return false;
        }

        private void HandleDisconnect(object sender, IPEndPoint source)
        {
            CloseConnection();
        }

        void IDisposable.Dispose()
        {
            CloseConnection();
        }
    }
}
