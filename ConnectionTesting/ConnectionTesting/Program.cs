using System;
using System.Threading;

namespace ConnectionTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Thread serverThread = new Thread(server.StartServer);
            serverThread.Start();

            string cmd;
            while((cmd = Console.In.ReadLine()) != "exit")
            {
                server.m_commandStack.Write(cmd);
            }

            server.Shutdown();

            Console.In.ReadLine();
        }
    }
}
