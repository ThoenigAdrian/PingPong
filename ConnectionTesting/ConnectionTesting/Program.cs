using System;
using System.Threading;

namespace ConnectionTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            new Thread(server.StartServer).Start();

            string cmd;
            while((cmd = Console.In.ReadLine()) != "exit")
            {
                // server offline commands
                if (server == null)
                {
                    if (cmd == "start")
                    {
                        server = new Server();
                        new Thread(server.StartServer).Start();
                    }
                }

                // server online commands
                else
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
    }
}
