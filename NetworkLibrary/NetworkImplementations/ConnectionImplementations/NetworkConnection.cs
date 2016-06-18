using NetworkLibrary.DataPackages;
using NetworkLibrary.PackageAdapters;
using NetworkLibrary.Utility;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class NetworkConnection : IDisposable
    {
        public delegate void ConnectionDiedHandler(NetworkConnection sender);
        public event ConnectionDiedHandler ConnectionDiedEvent;

        public Session ClientSession { get; set; }

        TCPConnection TcpConnection { get; set; }
        UDPConnection UdpConnection { get; set; }

        DataContainer<PackageInterface> TcpPackages { get; set; }
        DataContainer<DataWrapper<byte[]>> UdpData { get; set; }

        PackageAdapter Adapter { get; set; }

        public IPEndPoint RemoteEndPoint { get; set; }

        volatile bool m_connected;
        public bool Connected { get { return m_connected && TcpConnection.Connected; } }
        Semaphore m_disconnectLock;

        SafeList<ResponseRequest> m_openResponses;

        public NetworkConnection(TCPConnection tcpConnection) : this(tcpConnection, null) { }
        public NetworkConnection(TCPConnection tcpConnection, ResponseRequest responseRequest) : this(tcpConnection, responseRequest, new JSONAdapter()) { }
        public NetworkConnection(TCPConnection tcpConnection, ResponseRequest responseRequest, PackageAdapter adapter)
        {
            Adapter = adapter;

            TcpPackages = new SafeStack<PackageInterface>();
            m_openResponses = new SafeList<ResponseRequest>();

            if (responseRequest != null)
                m_openResponses.Add(responseRequest);

            ClientSession = null;

            TcpConnection = tcpConnection;
            TcpConnection.DataReceivedEvent += ReceiveTCP;
            TcpConnection.ReceiveErrorEvent += HandleTCPReceiveError;
            TcpConnection.InitializeReceiving();

            RemoteEndPoint = new IPEndPoint(TcpConnection.GetEndPoint.Address, TcpConnection.GetEndPoint.Port);

            m_connected = true;
            m_disconnectLock = new Semaphore(1, 1);
        }

        public void IssueResponse(ResponseRequest responseHandler)
        {
            m_openResponses.Add(responseHandler);
        }

        public bool ISConnectedTo(int port)
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
            m_disconnectLock.WaitOne();

            try
            {
                if (m_connected)
                {
                    m_connected = false;

                    TcpConnection.ReceiveErrorEvent -= HandleTCPReceiveError;
                    TcpConnection.DataReceivedEvent -= ReceiveTCP;
                    if(UdpConnection != null)
                        UdpConnection.DataReceivedEvent -= ReceiveUDP;

                    TcpConnection.Disconnect();

                    RaiseConnectionDiedEvent();
                }
            }
            finally
            {
                m_disconnectLock.Release();
            }
        }

        private void RaiseConnectionDiedEvent()
        {
            if (ConnectionDiedEvent != null)
                ConnectionDiedEvent.Invoke(this);
        }

        public void SendTCP(PackageInterface package)
        {
            try { TcpConnection.Send(Adapter.CreateNetworkDataFromPackage(package)); }
            catch (SocketException)
            { CloseConnection(); }
            catch (IOException)
            { CloseConnection(); }
        }

        public void SendUDP(PackageInterface package)
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            try { UdpConnection.Send(Adapter.CreateNetworkDataFromPackage(package), RemoteEndPoint); }
            catch (SocketException)
            { CloseConnection(); }
            catch (IOException)
            { CloseConnection(); }
        }

        public void SendKeepAlive()
        {
            try { TcpConnection.SendKeepAlive(); }
            catch (SocketException)
            { CloseConnection(); }
            catch (IOException)
            { CloseConnection(); }
        }

        public PackageInterface ReadTCP()
        {
            return TcpPackages.Read();
        }

        public PackageInterface ReadUDP()
        {
            if (UdpConnection == null)
                throw new ConnectionException("Network connection does not have an UDP connection!");

            DataWrapper<byte[]> dataWrapper = dataWrapper = UdpData.Read();
            if (dataWrapper != null && !dataWrapper.Read)
                return Adapter.CreatePackageFromNetworkData(dataWrapper.Data);

            return null;
        }

        private void ReceiveUDP(UDPConnection sender, byte[] data, IPEndPoint source)
        {
            if (RemoteEndPoint.Port == source.Port)
                UdpData.Write(new DataWrapper<byte[]>(data));
        }

        private void ReceiveTCP(TCPConnection sender, byte[] data)
        {
            PackageInterface[] packages = Adapter.CreatePackagesFromStream(data);
            if (packages == null)
                return;

            foreach (PackageInterface package in packages)
            {
                if(!CheckForResponse(package))
                    TcpPackages.Write(package);
            }
        }
        
        private bool CheckForResponse(PackageInterface package)
        {
            foreach (ResponseRequest responseRequest in m_openResponses.Entries)
            {
                if (responseRequest.InspectPackage(package) || responseRequest.State == ResponseRequest.ResponseState.Timeout)
                {
                    m_openResponses.Remove(responseRequest);
                    return true;
                }
            }

            return false;
        }

        private void HandleTCPReceiveError(ConnectionInterface sender, IPEndPoint source)
        {
            CloseConnection();
        }

        void IDisposable.Dispose()
        {
            CloseConnection();
        }
    }
}
