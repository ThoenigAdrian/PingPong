using NetworkLibrary.Utility;
using System;
using System.Net.Sockets;
using System.Threading;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public abstract class ConnectionInterface
    {
        public LogWriter Logger { get; set; }

        public bool Connected { get { return ConnectionSocket.Connected; } }

        public Socket ConnectionSocket { get; set; }

        protected DataContainer<byte[]> ReceivedData { get; set; }
        protected Thread ReceiveThread { get; set; }
        protected bool AbortReceive { get; set; }

        private ConnectionInterface()
        {
        }

        public ConnectionInterface(Socket connectionSocket)
        {
            Logger = null;

            ReceivedData = InitializeDataContainer();

            InitializeConnection(connectionSocket);
        }

        protected abstract DataContainer<byte[]> InitializeDataContainer();

        public void RestartConnection(Socket socket)
        {
            Disconnect();
            WaitForDisconnect();

            InitializeConnection(socket);
        }

        private void InitializeConnection(Socket socket)
        {
            InitializeSocket(socket);
            InitializeReceiving();
        }

        protected virtual void InitializeSocket(Socket socket)
        {
            ConnectionSocket = socket;
        }

        public virtual void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        public virtual byte[] Receive()
        {
            return ReceivedData.Read();
        }

        private void InitializeReceiving()
        {
            if (ReceiveThread.ThreadState != ThreadState.Unstarted)
                return;

            ReceiveThread = new Thread(ReceiveLoop);
            ReceiveThread.Start();
        }

        private void ReceiveLoop()
        {
            AbortReceive = false;

            byte[] data;
            while (!AbortReceive)
            {
                try
                {
                    data = new byte[NetworkConstants.MAX_PACKAGE_SIZE];
                    ConnectionSocket.Receive(data);
                }
                catch (Exception ex)
                {
                    Log("Receive loop threw exception: " + ex.Message);
                    return;
                }

                ReceivedData.Write(data);
            }
        }

        public void Disconnect()
        {
            AbortReceive = true;
            try
            {
                if (ConnectionSocket != null)
                    ConnectionSocket.Close();
                Log("Disconnected.");
            }
            catch
            {
                Log("Disconnecting error!");
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
