namespace PingPongServer
{
    class PingPongServer
    {
        static void Main(string[] args)
        {
            Server Server = new Server();
            try { Server.Run(); }
            catch { Server.Dispose(); }            
        }
    }
}
