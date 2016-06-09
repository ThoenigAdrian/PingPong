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

            InitializeSocket(connectionSocket);
        }

        protected abstract DataContainer<byte[]> InitializeDataContainer();

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

        public virtual void Send(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        public byte[] Receive()
        {
            return ReceivedData.Read();
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
                    ReceivedData.Write(ReceiveFromSocket());
                }
                catch (Exception ex)
                {
                    Log("Receive loop threw exception: " + ex.Message);
                    return;
                }
            }
        }

        private byte[] ReceiveFromSocket()
        {
            byte[] data = new byte[NetworkConstants.MAX_PACKAGE_SIZE];
            int size = ConnectionSocket.Receive(data);

            if (data.Length == size)
                return data;

            byte[] returnData = new byte[size];
            Array.Copy(data, 0, returnData, 0, size);
            return returnData;
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
