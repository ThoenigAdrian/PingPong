using System;

namespace NetworkLibrary
{
    class LogWriter
    {
        public virtual void Log(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}
