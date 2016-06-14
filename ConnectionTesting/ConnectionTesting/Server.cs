﻿using NetworkLibrary.DataPackages;
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

    class Server
    {
        ServerNetwork m_network;
        SessionIDPool m_sessionPool;

        Listening Listen;
        Sending Sender;

        LogWriter m_logger = new LogWriterConsole();

        bool m_stopServer = false;


        public SafeStack<string> m_commandStack;

        public delegate void ServerInitErrorHandle();
        public event ServerInitErrorHandle ServerInitError;

        public Server()
        {
            m_sessionPool = new SessionIDPool();
            m_commandStack = new SafeStack<string>();
            Listen = new Listening(4200);
            Sender = new Sending();
        }

        public void StartServer()
        {
            m_logger.Log("Initializing server...");

            if (!Listen.InitialiseListening())
            {
                m_logger.Log("Initializing error!\nServer shut down.");
                if (ServerInitError != null)
                    ServerInitError.Invoke();

                return;
            }

            m_network = new ServerNetwork(new UDPConnection(new IPEndPoint(IPAddress.Any, 4200)));
            m_network.SessionDied += ClientDisconnectHandler;

            Sender.m_network = m_network;

            m_logger.Log("Server started.");

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

                if (Sender.m_spammingActive)
                    Sender.Broadcast();
            }
        }

        private void AddAcceptedSocketsToNetwork()
        {
            Socket acceptedSocket;
            while ((acceptedSocket = Listen.m_socketQueue.Read()) != null)
            {
                int sessionID = m_sessionPool.GetSessionID;
                NetworkConnection clientConnection = new NetworkConnection(new TCPConnection(acceptedSocket), sessionID);
                ServerSessionResponse response = new ServerSessionResponse();
                response.ClientSessionID = sessionID;
                clientConnection.SendTCP(response);
                m_network.AddClientConnection(clientConnection);

                m_logger.Log("Client connected.");
            }
        }

        private void ReceiveData()
        {
            Dictionary<int, PackageInterface[]> tcpPackages = m_network.GetAllSessionDataTCP();
            if(tcpPackages != null)
            {
                foreach (KeyValuePair<int, PackageInterface[]> entry in tcpPackages)
                {
                    m_logger.Log("Received " + entry.Value.Length + " TCP packages from session " + entry.Key + ".");
                }
            }

            Dictionary<int, PackageInterface> udpPackages = m_network.GetAllSessionDataUDP();
            if (udpPackages != null)
            {
                foreach (KeyValuePair<int, PackageInterface> entry in udpPackages)
                {
                    m_logger.Log("Received UDP package from session " + entry.Key + ".");
                }
            }
        }

        private void ExecuteCommand()
        {
            string cmd;
            while ((cmd = m_commandStack.Read()) != default(string))
            {
                if (cmd.Contains("interval"))
                {
                    string[] split = cmd.Split(' ');
                    if (split.Length > 1)
                    {
                        try { Sender.m_sendInterval = Convert.ToInt32(split[1]); }
                        catch { Sender.m_sendInterval = 0; }
                    }
                }
                else
                {
                    switch (cmd)
                    {
                        case "mode tcp":
                            Sender.m_sendMode = SendMode.TCP;
                            m_logger.Log("Now sending in TCP mode.");
                            break;
                        case "mode udp":
                            Sender.m_sendMode = SendMode.UDP;
                            m_logger.Log("Now sending in UDP mode.");
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
        }



        private void ClientDisconnectHandler(NetworkInterface sender, int sessionID)
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

    class Sending
    {
        public bool m_spammingActive = false;
        public SendMode m_sendMode = SendMode.UDP;
        public int m_sendInterval = 0;
        DateTime m_lastSendStamp;
        public ServerNetwork m_network;

        Random m_random = new Random();
        float BallX = 0;
        float BallY = 0;
        float dirX = 0.1F;
        float dirY = 0.1F;

        public Sending()
        {
            m_lastSendStamp = DateTime.Now;
        }

        public void Broadcast()
        {
            if (m_network == null)
                return;

            DateTime currentTime = DateTime.Now;

            if (currentTime.Ticks - (m_sendInterval * 10000) < m_lastSendStamp.Ticks)
                return;

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

            m_lastSendStamp = currentTime;
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