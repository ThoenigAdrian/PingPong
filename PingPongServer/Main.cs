namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            Server Server = new Server();
            Server.Run();
            Server.Dispose();
        }
    }
}
