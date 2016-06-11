﻿using NetworkLibrary.DataPackages.ServerSourcePackages;
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
        UDPConnection m_udpConnection;
        ServerNetwork m_network;

        Listening Listen;

        bool m_stopServer = false;

        Thread m_acceptThread;

        public SafeStack<string> m_commandStack;

        public Server()
        {
            m_commandStack = new SafeStack<string>();
            Listen = new Listening(4200);
        }

        public void StartServer()
        {
            m_udpConnection = new UDPConnection(new IPEndPoint(IPAddress.Any, 4200));

            m_network = new ServerNetwork(m_udpConnection);

            m_acceptThread = new Thread(Listen.AcceptLoop);
            m_acceptThread.Start();

            ServerCylce();
        }

        private void ServerCylce()
        {
            while (!m_stopServer)
            {
                AddAcceptedSocketsToNetwork();
                ExecuteCommand();
            }
        }

        private void AddAcceptedSocketsToNetwork()
        {
            Socket acceptedSocket;
            while ((acceptedSocket = Listen.m_socketQueue.Read()) != null)
            {
                TCPConnection tcpConnection = new TCPConnection(acceptedSocket);
                tcpConnection.InitializeConnection();

                NetworkConnection clientConnection = new NetworkConnection(tcpConnection);
                m_network.AddClientConnection(clientConnection);

                Console.Out.WriteLine("Client connected.");
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
                }
            }
        }

        private void Broadcast()
        {
            ServerDataPackage package = new ServerDataPackage();

            package.BallPosX = 100;
            package.BallPosY = 200;

            m_network.SendPositionData(package);

            Console.Out.WriteLine("Sent position data.");
        }

        public void Shutdown()
        {
            Console.Out.WriteLine("Shutting down server...");
            m_stopServer = true;
            Listen.Disconnect();

            Console.Out.WriteLine("Server shut down. Good night Mister Bond.");
        }
    }

    class Listening
    {
        public SafeStack<Socket> m_socketQueue;

        int m_listeningPort;
        Socket m_listeningSocket;

        bool m_initialized = false;
        bool m_abortAccepting = false;

        public Listening(int listeningPort)
        {
            m_socketQueue = new SafeStack<Socket>();

            m_listeningPort = listeningPort;

            InitialiseListening();
        }

        void InitialiseListening()
        {
            m_listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listeningSocket.Bind(new IPEndPoint(IPAddress.Any, m_listeningPort));
            m_listeningSocket.Listen(3);

            m_initialized = true;
        }

        public void AcceptLoop()
        {
            if (!m_initialized)
                return;

            while(!m_abortAccepting)
            {
                Socket acceptedSocket = m_listeningSocket.Accept();
                m_socketQueue.Write(acceptedSocket);
            }
        }

        public void Disconnect()
        {
            m_abortAccepting = true;

            try
            {
                m_listeningSocket.Shutdown(SocketShutdown.Receive);
                m_listeningSocket.Close();
            }
            catch (SocketException ex)
            {
                //nothing to see here, its supposed to work like this
            }
        }
    }
}