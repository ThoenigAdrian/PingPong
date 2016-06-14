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
            set { }
        }

        public GameStructure(int maxPlayers)
        {
            this.maxPlayers = maxPlayers;
            GameField = new GameField();
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
            return -1;
        }
                
    }
}
