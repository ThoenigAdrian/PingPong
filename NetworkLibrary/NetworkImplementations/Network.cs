using NetworkLibrary.Utility;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary.NetworkImplementations
{
    abstract class DataNetwork
    {
        public LogWriter Logger { get; set; }

        public bool Connected { get { return ConnectionSocket.Connected; } }

        protected Socket ConnectionSocket { get; private set; }
        private IPEndPoint ServerEndPoint { get; set; }
        protected bool AbortReceive { get; set; }
        protected AddressFamily NetworkFamily { get { return ServerEndPoint.Address.AddressFamily; } }

        private DataNetwork()
        {

        }

        public DataNetwork(IPEndPoint server)
        {
            ServerEndPoint = server;
            AbortReceive = false;
            Logger = null;
            ConnectionSocket = InitializeSocket();
        }

        protected abstract Socket InitializeSocket();
   
        public void Connect()
        {
            AbortReceive = false;

            try
            {
                ConnectionSocket.Connect(ServerEndPoint);
                Log("Connected.");
                PostConnectActions();
            }
            catch
            {
                Log("Connecting error!");
            }
        }

        public void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }

        public void Disconnect()
        {
            AbortReceive = true;
            try
            {
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

        abstract protected void PostConnectActions();
    }
}
