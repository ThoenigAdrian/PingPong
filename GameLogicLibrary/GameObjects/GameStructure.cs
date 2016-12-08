using System.Collections.Generic;
using System;
using System.Threading;

namespace GameLogicLibrary.GameObjects
{
    public class GameStructure
    {
        BasicStructure Structure { get; set; }
        public GameField GameField { get { return Structure.Field; } }
        public Ball Ball { get { return Structure.Ball; } }
        List<Player> Players { get { return Structure.Players; } }

        public TeamsDictionary GameTeams = new TeamsDictionary();
        public const int TEAM_COUNT = 2; // restrict to two teams for now
        public int maxPlayers;
        public bool friendlyFire = false;
        public bool scoring = true;

        

        public int PlayersCount
        {
            get
            {
                int PlayersCount = 0;

                foreach(KeyValuePair<int, GameTeam> entry in GameTeams)
                    PlayersCount += entry.Value.PlayerList.Count;
                
                return PlayersCount;
            }
        }

        public int MissingPlayersCount
        {
            get
            {
               return maxPlayers - PlayersCount;
            }
        }

        public GameStructure(int maxPlayers)
        {
            Structure = new BasicStructure(new GameField(), new Ball());
            this.maxPlayers = maxPlayers;
            GameTeams.Add(0, new GameTeam());
            GameTeams.Add(1, new GameTeam());
        }

                                

        public void AddPlayer(Player Player, int Team)
        {
            try
            {
                GameTeams[Team].PlayerList.Add(Player);
            }
            catch(KeyNotFoundException)
            {
                GameTeams.Add(Team, new GameTeam());
                GameTeams[Team].PlayerList.Add(Player);
            }                            
        }

        public int GetFreeTeam()
        {
            foreach (KeyValuePair<int, GameTeam> Team in GameTeams)
            {
                if (Team.Value.PlayerList.Count < maxPlayers / TEAM_COUNT)
                    return Team.Key;
            }
            return 0;
        }

        public Player[] GetAllPlayers()
        {
            List<Player> allPlayers = new List<Player>();

            foreach (KeyValuePair<int, GameTeam> entry in GameTeams)
            {
                foreach (Player player in entry.Value.PlayerList)
                    allPlayers.Add(player);
            }

            return allPlayers.ToArray();
        }

        public class Circle
        {
            public float PositionX;
            public float PositionY;
            public float Radius;

            public Circle(float PositionX, float PositionY, float Radius)
            {
                this.PositionX = PositionX;
                this.PositionY = PositionY;
                this.Radius = Radius;
            }
        }

        public class Rectangle
        {
            public float PositionX;
            public float PositionY;
            public float Height;
            public float Width;

            public Rectangle(float PositionX, float PositionY, float Height, float Width)
            {
                this.PositionX = PositionX;
                this.PositionY = PositionY;
                this.Height = Height;
                this.Width = Width;

            }
        }
        
        public class Point
        {
            public float PositionX;
            public float PositionY;
            
            public Point(float PositionX, float PositionY)
            {
                this.PositionX = PositionX;
                this.PositionY = PositionY;
            }
        }

        public class GameTeam
        {
            public int score;
            public List<Player> PlayerList;

            public GameTeam()
            {
                score = 0;
                PlayerList = new List<Player>();
            }

            public override string ToString()
            {
                string s = "";
                foreach (Player p in PlayerList)
                {
                    s += "\n\t Player : " + p.ToString();
                }
                return s;
            }
        }

        public class TeamsDictionary : Dictionary<int, GameTeam>
        {
            public override string ToString()
            {
                string s = "";
                foreach (KeyValuePair<int, GameTeam> Team in this)
                {
                    s += "Team : " + Team.Key.ToString();
                    s += "\n" + Team.Value.ToString();
                    
                }
                return s;
            }
            
            
        }

    }
}
