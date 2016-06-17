using System;

namespace NetworkLibrary.Utility
{
    public abstract class LogWriter
    {
        public abstract void Log(string text);
        public abstract void NetworkLog(string text);
        public abstract void GameLog(string text);
    }

    public class LogWriterConsole : LogWriter
    {
        public override void Log(string text)
        {
            Console.Out.WriteLine(text);
        }

        public override void NetworkLog(string text)
        {
            Console.Out.WriteLine("[NETWORK] " + text);
        }

        public override void GameLog(string text)
        {
            Console.Out.WriteLine("[GAME] " + text);
        }
    }
}
