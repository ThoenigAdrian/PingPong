using System;
using System.Threading;

namespace ConnectionTesting
{
    class Program
    {
        static Server server;
        static void Main(string[] args)
        {
            CreateServer();

            string cmd;
            while((cmd = Console.In.ReadLine()) != "exit")
            {
                // server offline commands
                if (cmd == "start")
                {
                    CreateServer();
                }

                // server online commands
                else if(server != null)
                {
                    if (cmd == "stop")
                    {
                        server.Shutdown();
                        server = null;
                    }
                    else
                        server.m_commandStack.Write(cmd);
                }
            }

            if (server != null)
                server.Shutdown();

            Console.In.ReadLine();
        }

        static void CreateServer()
        {
            if (server == null)
            {
                server = new Server();
                server.ServerInitError += InitErrorHandler;
                new Thread(server.StartServer).Start();
            }
        }

        static void InitErrorHandler()
        {
            server.ServerInitError -= InitErrorHandler;
            server = null;
        }
    }
}
