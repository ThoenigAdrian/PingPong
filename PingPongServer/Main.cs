namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            Server Server = new Server();
            try { Server.Run(); }
            finally { Server.Dispose(); }            
        }
    }
}
