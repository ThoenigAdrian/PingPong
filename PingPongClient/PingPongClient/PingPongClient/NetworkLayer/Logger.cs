using System;

namespace PingPongClient.NetworkLayer
{
    class LogWriter
    {
        public virtual void Log(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}
