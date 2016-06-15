using NetworkLibrary.Utility;
using PingPongClient.NetworkLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConnectionTesting
{
    class Client : Module
    {
        ClientNetwork m_network;
        bool m_spamConnections;

        public Client(LogWriter logger) : base(logger)
        {
        }

        protected override void Initialize()
        {
            Logger.Log("Client started.");
        }

        private bool Connect()
        {
            if (m_network != null)
                Disconnect();

            try
            {
                Logger.Log("Connecting to server...");
                Socket connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connectSocket.Connect(new IPEndPoint(IPAddress.Loopback, 4200));

                m_network = new ClientNetwork(connectSocket, null);
                Logger.Log("Connected.");
                return true;
            }
            catch (SocketException) 
            {
                Disconnect();
                m_network = null;
                Logger.Log("Failed to connect.");
            }

            return false;
        }

        private void Disconnect()
        {
            if (m_network != null)
            {
                Logger.Log("Disconnecting from server...");
                m_network.Disconnect();
                m_network = null;
                Logger.Log("Disconnected.");
            }
        }

        protected override void ExecuteModuleActions()
        {
            if (m_spamConnections)
            {
                Disconnect();
                Connect();
            }
        }

        protected override void ExecuteCommand(string cmd)
        {
            switch (cmd)
            {
                case "spam":
                    m_spamConnections = !m_spamConnections;
                    break;
                case "connect":
                    Connect();
                    break;
                case "disconnect":
                    Disconnect();
                    break;
            }
        }

        protected override void ShutdownActions()
        {
            Disconnect();
        }
    }
}
