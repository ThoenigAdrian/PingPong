using NetworkLibrary.Utility;
using System;
using System.Threading;

namespace ConnectionTesting
{
    class Program
    {
        static Module m_activeModule;
        static Server server;
        static Client client;

        static LogWriter Logger = new LogWriterConsole();

        static void Main(string[] args)
        {
            string cmd;
            while((cmd = Console.In.ReadLine()) != "exit")
            {
                switch (cmd)
                {
                    case "server":
                        ShutdownModule();
                        StartNewServer();
                        m_activeModule = server;
                        break;

                    case "client":
                        ShutdownModule();
                        StartNewClient();
                        m_activeModule = client;
                        break;

                    case "stop":
                        ShutdownModule();
                        break;

                    default:
                        SendCommand(cmd);
                        break;
                }
            }

            ShutdownModule();

            Console.In.ReadLine();
        }

        static void ShutdownModule()
        {
            if (server != null)
                server.Shutdown();

            if (client != null)
                client.Shutdown();

            m_activeModule = null;
        }

        static void SendCommand(string cmd)
        {
            if(m_activeModule != null)
                m_activeModule.CommandStack.Write(cmd);
        }

        static void StartNewServer()
        {
            server = new Server(Logger);
            server.InitFailedEvent += InitErrorHandler;
            new Thread(server.Run).Start();
        }

        static void StartNewClient()
        {
            client = new Client(Logger);
            client.InitFailedEvent += InitErrorHandler;
            new Thread(client.Run).Start();
        }

        static void InitErrorHandler(Module sender)
        {
            sender.InitFailedEvent -= InitErrorHandler;
            m_activeModule = null;
        }
    }
}
