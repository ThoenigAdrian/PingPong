using System;
using XSLibrary.Utility;

namespace NetworkLibrary.Utility
{
    public abstract class GameLogger : Logger
    {
        public abstract void NetworkLog(string text);
        public abstract void GameLog(string text);
    }

    public class LogWriterConsole : GameLogger
    {
        public override void Log(string text)
        {
            Console.Out.WriteLine(text);
        }

        public override void NetworkLog(string text)
        {
            Log("[NETWORK] " + text);
        }

        public override void GameLog(string text)
        {
            Log("[GAME] " + text);
        }
    }
}
