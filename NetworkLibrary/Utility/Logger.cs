using System;

namespace NetworkLibrary.Utility
{
    abstract class LogWriter
    {
        public abstract void Log(string text);
    }

    class LogWriterConsole : LogWriter
    {
        public override void Log(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}
