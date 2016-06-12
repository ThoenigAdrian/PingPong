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

        public bool Connected
        {
            get
            {
                DisconnectLock.WaitOne();
                bool connected = ConnectionSocket != null &&
                        ConnectionSocket.Connected &&
                        !(ConnectionSocket.Poll(1000, SelectMode.SelectRead) && ConnectionSocket.Available == 0);

                DisconnectLock.Release();

                return connected;
            }
        }

        protected Socket ConnectionSocket { get; set; }

        protected Thread ReceiveThread { get; set; }
        protected bool AbortReceive { get; set; }

        Semaphore DisconnectLock;

        private ConnectionInterface()
        {
        }

        public ConnectionInterface(Socket connectionSocket, LogWriter logger)
        {
            DisconnectLock = new Semaphore(1, 1);

            Logger = logger;

            InitializeSocket(connectionSocket);
        }

        public void RestartConnection(Socket socket)
        {
            Disconnect();
            WaitForDisconnect();

            InitializeSocket(socket);
        }

        public virtual void InitializeConnection()
        {
            InitializeReceiving();
        }

        private void InitializeSocket(Socket socket)
        {
            ConnectionSocket = socket;
        }

        private void InitializeReceiving()
        {
            if (ReceiveThread != null && ReceiveThread.ThreadState != ThreadState.Unstarted)
                return;

            ReceiveThread = new Thread(ReceiveLoop);
            ReceiveThread.Start();
        }

        private void ReceiveLoop()
        {
            AbortReceive = false;

            while (!AbortReceive)
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
                    throw new ConnectionException("Receive loop threw exception: " + ex.Message, ex);
                }
            }
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
            DisconnectLock.WaitOne();

            AbortReceive = true;
            try
            {
                if (ConnectionSocket != null)
                {
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                    ConnectionSocket = null;
                    WaitForDisconnect();
                    Log("Disconnected.");
                }
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
                DisconnectLock.Release();
            }
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
