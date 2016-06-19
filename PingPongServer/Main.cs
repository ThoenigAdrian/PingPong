using System;
using System.Net.Sockets;

namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            Server Server = null;

            while (Server == null)
            {
                try
                {
                    Server = new Server();
                }
                catch (SocketException)
                {
                    Server = null;
                    Console.Out.WriteLine("Server initialization issue!\nPress enter to try again.");
                    Console.In.ReadLine();
                }
            }

            Server.Run();
        }
    }
}
