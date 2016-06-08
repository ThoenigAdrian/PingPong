using NetworkLibrary.Utility;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public abstract class ConnectionInterface
    {
        public LogWriter Logger { get; set; }

        public bool Connected { get { return ConnectionSocket.Connected; } }

        protected Socket ConnectionSocket { get; set; }
        
        protected bool AbortReceive { get; set; }

        private ConnectionInterface()
        {
        }

        public ConnectionInterface(Socket connectionSocket)
        {
            Logger = null;

            InitializeConnection(connectionSocket);
        }

        public void RestartConnection(Socket socket)
        {
            Disconnect();
            WaitForDisconnect();

            InitializeConnection(socket);
        }

        private void InitializeConnection(Socket socket)
        {
            AbortReceive = false;

            ConnectionSocket = socket;

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

        abstract public void Initialize();

        protected abstract void WaitForDisconnect();

        protected void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }

    }
}
