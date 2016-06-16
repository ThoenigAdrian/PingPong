﻿using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.Utility;
using PingPongClient.NetworkLayer;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConnectionTesting
{
    class Client : Module
    {
        ClientNetwork m_network;
        bool m_spamConnections;

        Semaphore m_disconnectLock = new Semaphore(1, 1);

        public Client(LogWriter logger) : base(logger)
        {
        }

        protected override void Initialize()
        {
            Logger.Log("Client started.");
        }

        private bool Connect(string ipAdress = "213.47.183.165")
        {
            if (m_network != null)
                Disconnect();


            IPAddress serverIP;
            try
            {
                serverIP = IPAddress.Parse(ipAdress);
            }
            catch
            {
                Logger.Log("Could not parse ip string: \"" + ipAdress + "\"");
                return false;
            }

            try
            {
                Logger.Log("Connecting to " + ipAdress);
                Socket connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connectSocket.Connect(new IPEndPoint(serverIP, 4200));

                m_network = new ClientNetwork(connectSocket, null);
                m_network.GetServerSessionResponse();
                m_network.SessionDied += ConnectionDied;
                Logger.Log("Connected with session ID " + m_network.ClientSession);
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
            m_disconnectLock.WaitOne();
            if (m_network != null)
            {
                m_network.Disconnect();
                m_network = null;
                Logger.Log("Disconnected.");
            }
            m_disconnectLock.Release();
        }

        private void ConnectionDied(NetworkInterface sender, int sessionID)
        {
            Disconnect();
            Logger.Log("Session with server died.");
        }

        protected override void ExecuteModuleActions()
        {
            if(m_network != null)
                ReceiveData();

            if (m_spamConnections)
            {
                Connect();
                Disconnect();
            }
        }

        protected override void ExecuteCommand(string cmd)
        {
            if (cmd.Length > 6 && cmd.Substring(0, 7) == "connect")
            {
                string[] split = cmd.Split();
                if (split.Length > 1)
                {
                    string ip = split[1];
                    Connect(ip);
                }
                else
                {
                    Connect();
                }
            }
            else
            {
                switch (cmd)
                {
                    case "spam":
                        m_spamConnections = !m_spamConnections;
                        break;
                    case "disconnect":
                        Disconnect();
                        break;
                }

                if (m_network != null)
                {
                    switch (cmd)
                    {
                        case "send start":
                            m_network.SendClientStart(2);
                            break;
                        case "send join":
                            m_network.SendClientJoin(1);
                            break;
                        case "send udp":
                            m_network.SendUDPTestData(new PlayerMovementPackage());
                            break;

                        case "send tcp":
                            m_network.SendPlayerMovement(new PlayerMovementPackage());
                            break;
                    }
                }
            }
        }


        private void ReceiveData()
        {
            Dictionary<int, PackageInterface[]> tcpPackages = m_network.CollectDataTCP();
            if (tcpPackages != null)
            {
                foreach (KeyValuePair<int, PackageInterface[]> entry in tcpPackages)
                {
                    Logger.Log("Received " + entry.Value.Length + " TCP packages from session " + entry.Key + ".");
                }
            }

            Dictionary<int, PackageInterface> udpPackages = m_network.CollectDataUDP();
            if (udpPackages != null)
            {
                foreach (KeyValuePair<int, PackageInterface> entry in udpPackages)
                {
                    Logger.Log("Received UDP package from session " + entry.Key + ".");
                }
            }
        }


        protected override void ShutdownActions()
        {
            Disconnect();
        }
    }
}
