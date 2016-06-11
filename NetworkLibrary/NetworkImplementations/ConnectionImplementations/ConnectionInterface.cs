using NetworkLibrary.Utility;
using System;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class ConnectionException : Exception
    {
        public string ErrorMessage { get; private set; }

        Exception BaseException { get; set; }

        public ConnectionException(string exceptionMessage) : this (exceptionMessage, null)
        {
        }

        public ConnectionException(string exceptionMessage, Exception innerException)
        {
            ErrorMessage = exceptionMessage;
            BaseException = innerException;
        }
    }


    public abstract class ConnectionInterface
    {
        LogWriter Logger { get; set; }

        public bool Connected { get { return ConnectionSocket.Connected; } }

        protected Socket ConnectionSocket { get; set; }

        protected Thread ReceiveThread { get; set; }
        protected bool AbortReceive { get; set; }

        private ConnectionInterface()
        {
        }

        public ConnectionInterface(Socket connectionSocket, LogWriter logger)
        {
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
            AbortReceive = true;
            try
            {
                if (ConnectionSocket != null)
                    ConnectionSocket.Close();
                Log("Disconnected.");
            }
            catch (SocketException ex)
            {
                Log(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ConnectionException("Exception while disconnecting! Exception message: " + ex.Message, ex);
            }
        }

        protected virtual void WaitForDisconnect()
        {
            ReceiveThread.Join();
        }

        protected void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }

    }
}
