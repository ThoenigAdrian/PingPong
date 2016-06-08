using NetworkLibrary.Utility;
using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.ConnectionImplementations.NetworkImplementations
{
    public abstract class DataNetwork
    {
        public LogWriter Logger { get; set; }

        public bool Connected { get { return ConnectionSocket.Connected; } }

        protected Socket ConnectionSocket { get; set; }
        
        protected bool AbortReceive { get; set; }

        private DataNetwork()
        {
        }

        public DataNetwork(Socket connectionSocket)
        {
            Logger = null;

            InitializeNetwork(connectionSocket);
        }

        public void RestartNetwork(Socket socket)
        {
            Disconnect();
            WaitForDisconnect();

            InitializeNetwork(socket);
        }

        private void InitializeNetwork(Socket socket)
        {
            AbortReceive = false;

            ConnectionSocket = socket;

            NetworkSpecificInitializing();

            if (!Connected)
                Log("Initializing failed.");
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

        abstract public byte[] Receive();

        abstract public void Send(byte[] data);

        abstract protected void NetworkSpecificInitializing();

        protected abstract void WaitForDisconnect();

        protected void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }

    }
}
