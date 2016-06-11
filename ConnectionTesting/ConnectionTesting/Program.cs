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
                if (cmd == "start" && server == null)
                {
                    server = new Server();
                    new Thread(server.StartServer).Start();
                }
                if (cmd == "disconnect" && server != null)
                {
                    server.Shutdown();
                    server = null;
                }
                else
                    server.m_commandStack.Write(cmd);
            }

            if(server != null)
                server.Shutdown();

            Console.In.ReadLine();
        }
    }
}
