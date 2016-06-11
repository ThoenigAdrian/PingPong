using System.Net;

namespace PingPongClient
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Control game = new Control())
            {
                game.Run();
            }
        }
    }
#endif
}

