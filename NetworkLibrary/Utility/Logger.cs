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
        public abstract void LoadBalancerLog(string text);
        public abstract void GamesExecutorLog(string text);
    }

    public class LogWriterConsole : GameLogger
    {
        public int? GameID;
        public int? GamesExecutorID;
        public LogWriterConsole()
        {

        }
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
            if(GameID.HasValue)
                Log("[GAME]  ID: " + GameID.ToString() + " " + text);
            else
                Log("[GAME]  " + text);
        }

        public override void GamesExecutorLog(string text)
        {
            if(GamesExecutorID.HasValue)
                Log("[GAMES_EXECUTOR]  ID: " + GamesExecutorID.ToString() + " " + text);
            else
                Log("[GAMES_EXECUTOR]  " + text);
        }

        public override void LoadBalancerLog(string text)
        {
            Log("[GAMES_EXECUTOR_LOAD_BALANCER]  " + text);
        }
    }
}
