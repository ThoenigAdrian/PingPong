using System;

namespace NetworkLibrary.Utility
{
    public abstract class LogWriter
    {
        public abstract void Log(string text);
    }

    public class LogWriterConsole : LogWriter
    {
        public override void Log(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}
