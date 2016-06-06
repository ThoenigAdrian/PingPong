using NetworkLibrary;
using System.Net;
using System.Net.Sockets;

namespace PingPongClient.NetworkLayer
{
    abstract class DataNetwork <T>
    {
        public LogWriter Logger { get; set; }

        public bool Connected { get { return ConnectionSocket.Connected; } }

        protected Socket ConnectionSocket { get; set; }
        private IPAddress ServerIP { get; set; }
        protected bool AbortReceive { get; set; }


        private DataNetwork()
        {

        }

        public DataNetwork(IPAddress serverIP)
        {
            ServerIP = serverIP;
            AbortReceive = false;
            Logger = null;
        }

        public void Connect()
        {
            AbortReceive = false;

            try
            {
                ConnectionSocket.Connect(new IPEndPoint(ServerIP, NetworkConstants.SERVER_PORT));
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

        abstract public T Receive();

        abstract public void Send(T data);

        abstract protected void PostConnectActions();
    }
}
