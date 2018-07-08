using System;
using XSLibrary.Utility;

namespace NetworkLibrary.Utility
{
    public abstract class GameLogger : Logger
    {
        public abstract void NetworkLog(string text);
        public abstract void GameLog(string text);
        public abstract void GamesManagerLog(string text);
        public abstract void ServerLog(string text);
        public abstract void RegistrationLog(string text);
        public abstract void ConfigurationLog(string text);
    }

    public class LogWriterConsole : GameLogger
    {
        public override void Log(string text)
        {
            Console.Out.WriteLine(text);
        }

        public override void ConfigurationLog(string text)
        {
            Log("[CONFIGURATION]  " + text);
        }

        public override void ServerLog(string text)
        {
            Log("[SERVER]  " + text);
        }

        public override void GamesManagerLog(string text)
        {
            Log("[GAMES_MANAGER]  " + text);
        }

        public override void NetworkLog(string text)
        {
            Log("[NETWORK]  " + text);
        }

        public override void RegistrationLog(string text)
        {
            Log("[REGISTRATION]  " + text);
        }

        public override void GameLog(string text)
        {
            Log("[GAME]  " + text);
        }
    }
}
