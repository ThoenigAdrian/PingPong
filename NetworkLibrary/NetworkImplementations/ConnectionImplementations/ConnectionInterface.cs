using NetworkLibrary.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string exceptionMessage) : this (exceptionMessage, null)
        {
        }

        public ConnectionException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }

    public abstract class ConnectionInterface
    {
        public delegate void ReceiveErrorHandler(ConnectionInterface sender, IPEndPoint endPoint);
        public event ReceiveErrorHandler ReceiveErrorEvent;

        LogWriter Logger { get; set; }

        protected Semaphore SocketLock;

        public bool Connected
        {
            get
            {
                SocketLock.WaitOne();
                try { return (!Disconnecting && ConnectionSocket.Connected); }
                finally { SocketLock.Release(); }
            }
        }

        volatile bool m_receiving = false;
        public bool Receiving
        {
            get { return m_receiving && !Disconnecting; }
            protected set
            {
                m_receiving = value;
            }
        }

        protected Socket ConnectionSocket { get; set; }
        protected IPEndPoint ConnectionEndpoint { get; set; }

        protected Thread ReceiveThread { get; set; }

        private volatile bool m_disconnected;
        protected bool Disconnecting
        {
            get { return m_disconnected; }
            set { m_disconnected = value; }
        }

        private ConnectionInterface()
        {
        }

        public ConnectionInterface(Socket connectionSocket, LogWriter logger) : this(connectionSocket, connectionSocket.RemoteEndPoint as IPEndPoint, logger)
        {
        }

        public ConnectionInterface(Socket connectionSocket, IPEndPoint remote, LogWriter logger)
        {
            Logger = logger;

            SocketLock = new Semaphore(1, 1);

            ConnectionEndpoint = new IPEndPoint(remote.Address, remote.Port);
            InitializeSocket(connectionSocket);
        }

        public void InitializeReceiving()
        {
            SocketLock.WaitOne();

            try
            {
                if (Disconnecting)
                {
                    throw new ConnectionException("Can not receive from a disconnected connection!");
                }

                if (!Receiving)
                {
                    Receiving = true;
                    PreReceiveSettings();
                    StartReceiving();
                }
            }
            finally
            {
                SocketLock.Release();
            }
        }

        protected abstract void PreReceiveSettings();

        private void InitializeSocket(Socket socket)
        {
            SocketLock.WaitOne();
            Disconnecting = false;
            ConnectionSocket = socket;
            SocketLock.Release();
        }

        private void StartReceiving()
        {
            if (ReceiveThread != null && ReceiveThread.ThreadState != ThreadState.Unstarted)
                return;

            ReceiveThread = new Thread(ReceiveLoop);
            ReceiveThread.Start();
        }

        private void ReceiveLoop()
        {
            while (!Disconnecting)
            {
                try
                {
                    ReceiveFromSocket();
                }
                catch (SocketException)
                {
                    ReceiveThread = null;
                    ReceiveErrorHandling(ConnectionEndpoint);
                }
                catch (ObjectDisposedException)
                {
                    ReceiveThread = null;
                    ReceiveErrorHandling(ConnectionEndpoint);
                }
            }

            Receiving = false;
        }

        protected abstract void ReceiveFromSocket();

        protected void ReceiveErrorHandling(IPEndPoint source)
        {
            ReceiveErrorHandler threadCopy = ReceiveErrorEvent;
            if(threadCopy != null)
                ReceiveErrorEvent.Invoke(this, source);
        }

        protected byte[] TrimData(byte[] data, int size)
        {
            if (data.Length <= size)
                return data;

            byte[] returnData = new byte[size];
            Array.Copy(data, 0, returnData, 0, size);
            return returnData;
        }
        
        public bool ConnectionPolling()
        {
            if (Disconnecting)
                return false;

            bool connected = false;
            if (ConnectionSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] data = new byte[1];
                connected = ConnectionSocket.Receive(data, SocketFlags.Peek) != 0;
            }

            return connected;
        }

        public void Disconnect()
        {
            SocketLock.WaitOne();

            if (!Disconnecting)
            {
                Disconnecting = true;

                try
                {
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                }
                catch (SocketException ex)
                {
                    Log(ex.Message);
                }
                catch (Exception ex)
                {
                    throw new ConnectionException("Exception while disconnecting! Exception message: " + ex.Message, ex);
                }
                finally
                {
                    SocketLock.Release();
                }

                WaitForDisconnect();
                Log("Disconnected.");
            }
            else
                SocketLock.Release();
        }

        protected virtual void WaitForDisconnect()
        {
            Thread threadCopy = ReceiveThread;
            if (threadCopy != null)
                ReceiveThread.Join();
        }

        protected void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }

    }
}
