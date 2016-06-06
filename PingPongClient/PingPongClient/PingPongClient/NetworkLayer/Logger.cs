using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingPongClient.NetworkLayer
{
    class LogWriter
    {
        public void Log(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}
