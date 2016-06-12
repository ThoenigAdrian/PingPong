using NetworkLibrary.Utility;
using System;
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
        LogWriter Logger { get; set; }

        protected Semaphore SocketLock;

        public bool Connected
        {
            get
            {
                SocketLock.WaitOne();
                bool connected = !Disconnecting && ConnectionSocket.Connected &&
                        !(ConnectionSocket.Poll(1000, SelectMode.SelectRead) && ConnectionSocket.Available == 0);

                SocketLock.Release();

                return connected;
            }
        }

        bool m_receiving = false;
        public bool Receiving
        {
            get { return m_receiving && !Disconnecting; }
            protected set
            {
                m_receiving = value;
            }
        }

        protected Socket ConnectionSocket { get; set; }

        protected Thread ReceiveThread { get; set; }
        protected bool Disconnecting { get; set; }

        private ConnectionInterface()
        {
        }

        public ConnectionInterface(Socket connectionSocket, LogWriter logger)
        {
            SocketLock = new Semaphore(1, 1);

            Logger = logger;

            InitializeSocket(connectionSocket);
        }

        public void RestartConnection(Socket socket)
        {
            Disconnect();
            WaitForDisconnect();

            InitializeSocket(socket);
            InitializeReceiving();
        }

        public void InitializeReceiving()
        {
            SocketLock.WaitOne();

            if (Disconnecting)
            {
                SocketLock.Release();
                throw new ConnectionException("Can not receive from a disconnected connection!");
            }

            if (!Receiving)
            {

                Receiving = true;
                PreReceiveSettings();
                StartReceiving();
            }

            SocketLock.Release();
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
                catch (SocketException ex)
                {
                    Log(ex.Message);
                }
                catch (Exception ex)
                {
                    Receiving = false;

                    if (Disconnecting)
                        return;

                    throw new ConnectionException("Receive loop threw exception: " + ex.Message, ex);
                }
            }

            Receiving = false;
        }

        protected byte[] TrimData(byte[] data, int size)
        {
            if (data.Length <= size)
                return data;

            byte[] returnData = new byte[size];
            Array.Copy(data, 0, returnData, 0, size);
            return returnData;
        }

        protected abstract void ReceiveFromSocket();

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
            if (ReceiveThread != null)
                ReceiveThread.Join();
        }

        protected void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }

    }
}
