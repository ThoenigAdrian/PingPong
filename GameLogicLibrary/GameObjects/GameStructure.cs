using System.Collections.Generic;

namespace GameLogicLibrary.GameObjects
{
    public class GameStructure
    {
        public GameField GameField;
        public PlayBall Ball;
        public Dictionary<int,List<Player>> GameTeams = new Dictionary<int, List<Player>>();
        private int numberOfTeams = 2; // restrict to two teams for now
        public int maxPlayers;
        public int PlayersCount
        {
            get
            {
                int PlayersCount = 0;

                foreach(KeyValuePair<int, List<Player>> entry in GameTeams)
                    PlayersCount += entry.Value.Count;
                
                return PlayersCount;
            }
        }

        // i can not tell how many players the game will have when i construct the field - using default 2 
        public GameStructure()
        {
            this.maxPlayers = 2;
            GameField = new GameField();
            GameTeams.Add(0, new List<Player>());
            GameTeams.Add(1, new List<Player>());
            Ball = new PlayBall();
        }

        public void AddPlayer(Player Player, int Team)
        {
            try
            {
                GameTeams[Team].Add(Player);
            }
            catch(KeyNotFoundException)
            {
                GameTeams.Add(Team, new List<Player>());
                GameTeams[Team].Add(Player);
            }                            
        }

        public int GetFreeTeam()
        {
            foreach (KeyValuePair<int, List<Player>> entry in GameTeams)
            {
                if (entry.Value.Count < maxPlayers / numberOfTeams)
                    return entry.Key;
            }
            return 0;
        }

        public Player[] GetAllPlayers()
        {
            List<Player> allPlayers = new List<Player>();

            foreach (KeyValuePair<int, List<Player>> entry in GameTeams)
            {
                foreach (Player player in entry.Value)
                    allPlayers.Add(player);
            }

            return allPlayers.ToArray();
        }

    }
}
