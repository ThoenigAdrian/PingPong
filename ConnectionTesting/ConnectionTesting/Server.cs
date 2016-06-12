using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConnectionTesting
{
    class Server
    {
        ServerNetwork m_network;

        Listening Listen;

        bool m_stopServer = false;
        bool m_spammingActive = false;

        public SafeStack<string> m_commandStack;

        public delegate void ServerInitErrorHandle();
        public event ServerInitErrorHandle ServerInitError;

        public Server()
        {
            m_commandStack = new SafeStack<string>();
            Listen = new Listening(4200);
        }

        public void StartServer()
        {
            Console.Out.WriteLine("Initializing server...");

            if (!Listen.InitialiseListening())
            {
                Console.Out.WriteLine("Initializing error!\nServer shut down.");
                if (ServerInitError != null)
                    ServerInitError.Invoke();

                return;
            }


            m_network = new ServerNetwork(new UDPConnection(new IPEndPoint(IPAddress.Any, 4200)));
            m_network.SessionDied += ClientDisconnectHandler;

            Console.Out.WriteLine("Server started.");

            ServerCylce();
        }

        private void ServerCylce()
        {
            while (!m_stopServer)
            {
                m_network.UpdateConnections();
                AddAcceptedSocketsToNetwork();
                ReceiveData();
                ExecuteCommand();

                if (m_spammingActive)
                    Broadcast();
            }
        }

        private void AddAcceptedSocketsToNetwork()
        {
            Socket acceptedSocket;
            while ((acceptedSocket = Listen.m_socketQueue.Read()) != null)
            {
                NetworkConnection clientConnection = new NetworkConnection(new TCPConnection(acceptedSocket));
                m_network.AddClientConnection(clientConnection);

                Console.Out.WriteLine("Client connected.");
            }
        }

        private void ReceiveData()
        {
            for (int i = 0; i < m_network.ClientCount; i++)
            {
                if(m_network.ReceivePackageUDP(i) != null)
                    Console.Out.WriteLine("Received UDP package from session " + i + ".");

                PackageInterface[] tcpPackages = m_network.ReceivePackageTCP(i);

                if(tcpPackages.Length > 0)
                    Console.Out.WriteLine("Received " + tcpPackages.Length + " TCP packages from session " + i + ".");
            }
        }

        private void ExecuteCommand()
        {
            string cmd;
            while ((cmd = m_commandStack.Read()) != default(string))
            {
                switch(cmd)
                {
                    case "send":
                        Broadcast();
                        break;
                    case "spam":
                        m_spammingActive = !m_spammingActive;
                        break;
                }
            }
        }

        private void Broadcast()
        {
            ServerDataPackage package = new ServerDataPackage();

            package.Ball.PositionX = 100;
            package.Ball.PositionY = 200;

            m_network.SendPositionData(package);

            Console.Out.WriteLine("Sent position data.");
        }

        private void ClientDisconnectHandler(int sessionID)
        {
            Console.Out.WriteLine("Client disconnected.");
        }

        public void Shutdown()
        {
            Console.Out.WriteLine("Shutting down server...");
            m_stopServer = true;
            Listen.Disconnect();
            m_network.Disconnect();

            Console.Out.WriteLine("Server shut down. Good night Mister Bond.");
        }
    }

    class Listening
    {
        public SafeStack<Socket> m_socketQueue;

        int m_listeningPort;
        Socket m_listeningSocket;

        bool m_abortAccepting = false;

        public Listening(int listeningPort)
        {
            m_socketQueue = new SafeStack<Socket>();

            m_listeningPort = listeningPort;
        }

        public bool InitialiseListening()
        {
            try
            {
                m_listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_listeningSocket.Bind(new IPEndPoint(IPAddress.Any, m_listeningPort));
                m_listeningSocket.Listen(3);
            }
            catch
            {
                return false;
            }

            new Thread(AcceptLoop).Start();

            return true;
        }

       void AcceptLoop()
        {
            while(!m_abortAccepting)
            {
                Socket acceptedSocket;
                try
                {
                    acceptedSocket = m_listeningSocket.Accept();
                    m_socketQueue.Write(acceptedSocket);
                }
                catch (SocketException)
                {
                    //nothing to see here, its supposed to work like this
                }

            }
        }

        public void Disconnect()
        {
            m_abortAccepting = true;
            m_listeningSocket.Close();
        }
    }
}
