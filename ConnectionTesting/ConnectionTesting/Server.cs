using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConnectionTesting
{
    enum SendMode
    {
        TCP,
        UDP
    }

    class Server : Module
    {
        ServerNetwork m_network;
        SessionIDPool m_sessionPool;

        Listening Listen;
        Sending Sender;

        public Server(LogWriter logger) : base(logger)
        {
            m_sessionPool = new SessionIDPool();
            Listen = new Listening(4200);
            Sender = new Sending();
        }

        protected override void Initialize()
        {
            Logger.Log("Initializing server...");

            if (!Listen.InitialiseListening())
            {
                Logger.Log("Initializing error!\nServer shut down.");
                RaiseInitFailEvent();
                return;
            }

            m_network = new ServerNetwork(new UDPConnection(new IPEndPoint(IPAddress.Any, 4200)));
            m_network.SessionDied += ClientDisconnectHandler;

            Sender.m_network = m_network;

            Logger.Log("Server started.");
        }

        protected override void ExecuteModuleActions()
        {
            m_network.UpdateConnections();
            AddAcceptedSocketsToNetwork();
            ReceiveData();

            if (Sender.m_spammingActive)
                Sender.Broadcast();
        }

        private void AddAcceptedSocketsToNetwork()
        {
            Socket acceptedSocket;
            while ((acceptedSocket = Listen.m_socketQueue.Read()) != null)
            {
                Logger.Log("Client connected.");
                int sessionID = m_sessionPool.GetSessionID;
                NetworkConnection clientConnection = new NetworkConnection(new TCPConnection(acceptedSocket));
                clientConnection.ClientSession = new Session(sessionID);
                m_network.AddClientConnection(clientConnection);
                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = sessionID;
                clientConnection.SendTCP(response);
                Logger.Log("Sent client session ID: " + sessionID);
            }
        }

        private void ReceiveData()
        {
            Dictionary<int, PackageInterface[]> tcpPackages = m_network.CollectDataTCP();
            foreach (KeyValuePair<int, PackageInterface[]> entry in tcpPackages)
            {
                Logger.Log("Received " + entry.Value.Length + " TCP packages from session " + entry.Key + ".");
            }

            Dictionary<int, PackageInterface> udpPackages = m_network.CollectDataUDP();
            foreach (KeyValuePair<int, PackageInterface> entry in udpPackages)
            {
                Logger.Log("Received UDP package from session " + entry.Key + ".");
            }
        }

        protected override void ExecuteCommand(string cmd)
        {
            if (cmd.Contains("interval"))
            {
                string[] split = cmd.Split(' ');
                if (split.Length > 1)
                {
                    try { Sender.SendInterval = Convert.ToInt64(split[1]) * 1000; }
                    catch { Sender.SendInterval = 0; }
                }
            }
            else
            {
                switch (cmd)
                {
                    case "mode tcp":
                        Sender.m_sendMode = SendMode.TCP;
                        Logger.Log("Now sending in TCP mode.");
                        break;
                    case "mode udp":
                        Sender.m_sendMode = SendMode.UDP;
                        Logger.Log("Now sending in UDP mode.");
                        break;
                    case "send":
                        Sender.Broadcast();
                        break;
                    case "spam":
                        Sender.m_spammingActive = !Sender.m_spammingActive;
                        break;

                }
            }
        }



        private void ClientDisconnectHandler(NetworkInterface sender, int sessionID)
        {
            Console.Out.WriteLine("Client disconnected.");
        }

        protected override void ShutdownActions()
        {
            Console.Out.WriteLine("Shutting down server...");
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
                m_listeningSocket.Listen(10);
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
                    Thread.Sleep(100);
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

    class Sending
    {
        public bool m_spammingActive = false;
        public SendMode m_sendMode = SendMode.UDP;
        public long SendInterval { set { timer.TimerInterval(value); } }
        public ServerNetwork m_network;

        OneShotTimer timer = new OneShotTimer(0);

        Random m_random = new Random();
        float BallX = 0;
        float BallY = 0;
        float dirX = 1F;
        float dirY = 1F;

        public Sending()
        {
        }

        public void Broadcast()
        {
            if (m_network == null)
                return;

            if (timer == false)
            {
                return;
            }

            timer.Restart();

            ServerDataPackage package = new ServerDataPackage();

            BallX += dirX;
            if (BallX <= 0 || BallX > GameLogicLibrary.GameInitializers.BORDER_WIDTH)
                dirX *= -1;

            BallY += dirY;
            if (BallY <= 0 || BallY > GameLogicLibrary.GameInitializers.BORDER_HEIGHT)
                dirY *= -1;

            package.Ball.PositionX = BallX;
            package.Ball.PositionY = BallY;

            switch (m_sendMode)
            {
                case SendMode.UDP:
                    m_network.SendPositionDataUDP(package);
                    break;

                case SendMode.TCP:
                    m_network.SendPositionDataTCP(package);
                    break;
            }
            Console.Out.WriteLine("Sent position data.");
        }
    }

    class SessionIDPool
    {
        int m_nextSessionID = 0;

        public int GetSessionID
        {
            get
            {
                return m_nextSessionID++;
            }
        }
    }
}
